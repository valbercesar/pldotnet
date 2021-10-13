/*
 * PL/.NET (pldotnet) - PostgreSQL support for .NET C# and F# as
 *                      procedural languages (PL)
 *
 *
 * Copyright 2019-2020 Brick Abode
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * DotNetEngine/src/csharp/Lib.cs - pldotnet assembly compiler and runner
 *
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Net.Http;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Net.Client;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace PlDotNET
{
    public static class Engine
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct LibArgs
        {
            public IntPtr SourceCode;
            public int Number;
            public uint FuncOid;
        }

        public struct CachedFunction
        {
            public string SourceCode;
            public Func<IntPtr, int, int> CallFunction;
            public Action<IntPtr, int> AddProperty;
            public Action ResetFuncRecords;

            public bool needsReset;
        }

        static uint funcOid;

        static MemoryStream memStream;
        static Assembly compiledAssembly;

        static IDictionary<uint, CachedFunction> funcBuiltCodeDict;
        static Storage.StorageClient client;

        static CachedFunction cachedFunction;

        static Func<IntPtr, int, int> userFunction;

        static bool needsReset = true;

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_Elog(int level, string nessage);

        public static void pldotnet_Info(string message)
        {
            pldotnet_Elog(17, message);
        }

        public static void pldotnet_Warning(string message)
        {
            pldotnet_Elog(19, message);
        }

        public static int Compile(IntPtr arg, int argLength)
        {
            string spiSrc = @"
public static class SPI
{
    [DllImport(""@PKG_LIBDIR/pldotnet.so"")]
    public static extern int pldotnet_SPIExecute (string cmd, long limit);

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct PropertyValue
    {
        public IntPtr value;
        public string name;
        public int type;
        public int nrow;
    }

    public class PropertyValueReader
    {
        public PropertyValueReader() {}

        public object ReadValue<T>(IntPtr handle)
        {
            if (typeof(T) == typeof(string))
            {
                return (object) Marshal.PtrToStringUTF8(handle);
            }
            if (typeof(T) == typeof(decimal))
            {
                return (object) Convert.ToDecimal(Marshal.PtrToStringAnsi(handle));
            }
            return (object) Marshal.PtrToStructure<T>(handle);
        }

        public object Read(PropertyValue prop)
        {
            switch((TypeOid) prop.type)
            {
                case TypeOid.BOOLOID:
                    return ReadValue<bool>(prop.value);
                case TypeOid.INT2OID:
                    return ReadValue<short>(prop.value);
                case TypeOid.INT4OID:
                    return ReadValue<int>(prop.value);
                case TypeOid.INT8OID:
                    return ReadValue<long>(prop.value);
                case TypeOid.FLOAT4OID:
                    return ReadValue<float>(prop.value);
                case TypeOid.FLOAT8OID:
                    return ReadValue<double>(prop.value);
                case TypeOid.NUMERICOID:
                    return ReadValue<decimal>(prop.value);
                case TypeOid.VARCHAROID:
                    return ReadValue<string>(prop.value);
                default:
                    return null;
            }
        }
    }

    public class PropertyValueWriter
    {
        public PropertyValueWriter() {}

        public void WriteValue<T>(T value, IntPtr handle)
        {
            if (typeof(T) == typeof(string))
            {
                // TODO
                throw new NotImplementedException();
            }
            if (typeof(T) == typeof(decimal))
            {
                // TODO
                throw new NotImplementedException();
            }
            Marshal.StructureToPtr<T>(value, handle, false);
        }
        public void Write(object value, PropertyValue prop)
        {
            switch((TypeOid) prop.type)
            {
                case TypeOid.BOOLOID:
                    WriteValue<bool>((bool) value, prop.value);
                    break;
                case TypeOid.INT2OID:
                    WriteValue<short>((short) value,  prop.value);
                    break;
                case TypeOid.INT4OID:
                    WriteValue<int>((int) value, prop.value);
                    break;
                case TypeOid.INT8OID:
                    WriteValue<long>((long) value, prop.value);
                    break;
                case TypeOid.FLOAT4OID:
                    WriteValue<float>((float) value, prop.value);
                    break;
                case TypeOid.FLOAT8OID:
                    WriteValue<double>((double) value, prop.value);
                    break;
                case TypeOid.NUMERICOID:
                    WriteValue<decimal>((decimal) value, prop.value);
                    break;
                case TypeOid.VARCHAROID:
                    WriteValue<string>((string) value, prop.value);
                    break;
                default:
                    throw new NotSupportedException(string.Format(""Type not supported {0}"", prop.type));
            }
        }
    }

    public abstract class DbUpdatableRecordErrorMessages
    {
        public const string ADP_InvalidSourceBufferIndex = ""Source buffer is not valid (size of {0}) offset: {1}"";
        public const string ADP_InvalidDestinationBufferIndex = ""Destination buffer is not valid (size of {0}) offset: {1}"";
        public const string ADP_InvalidDataLength = ""Data length '{0}' is less than 0."";
    }

    public abstract class BpgsqlDbUpdatableRecord : IDataRecord
    {
        protected IDictionary<string, object> Record;
        protected List<object> Values;
        protected List<string> ColumnNames;

        public BpgsqlDbUpdatableRecord()
        {
            Record = new Dictionary<string, object>();
            Values = new List<object>();
            ColumnNames = new List<string>();
        }

        public int FieldCount => Values.Count;

        public object this[int ordinal]
        {
            get { return GetValue(ordinal); }
        }

        public object this[string name]
        {
            get { return GetValue(GetOrdinal(name)); }
        }

        public bool GetBoolean(int ordinal) => (bool) GetValue(ordinal);

        public byte GetByte(int ordinal) => (byte) GetValue(ordinal);
        [SuppressMessage(""Microsoft.Usage"", ""CA2201:DoNotRaiseReservedExceptionTypes"")]

        public long GetBytes(int ordinal, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            byte[] tempBuffer;
            tempBuffer = (byte[]) GetValue(ordinal);

            if (buffer == null)
            {
                return tempBuffer.Length;
            }

            var srcIndex = (int) dataIndex;
            var byteCount = Math.Min(tempBuffer.Length - srcIndex, length);

            if (srcIndex < 0)
            {
                throw new ArgumentOutOfRangeException(
                    ""dataIndex"",
                    string.Format(
                        DbUpdatableRecordErrorMessages.ADP_InvalidSourceBufferIndex,
                        tempBuffer.Length.ToString(CultureInfo.InvariantCulture),
                        ((long) srcIndex).ToString(CultureInfo.InvariantCulture)
                    )
                );
            }
            else if ((bufferIndex < 0)
                     || (bufferIndex > 0 && bufferIndex >= buffer.Length))
            {
                throw new ArgumentOutOfRangeException(
                    ""bufferIndex"",
                    string.Format(
                        DbUpdatableRecordErrorMessages.ADP_InvalidDestinationBufferIndex,
                        buffer.Length.ToString(CultureInfo.InvariantCulture),
                        bufferIndex.ToString(CultureInfo.InvariantCulture)
                    )
                );
            }

            if (0 < byteCount)
            {
                Array.Copy(tempBuffer, dataIndex, buffer, bufferIndex, byteCount);
            }
            else if (length < 0)
            {
                throw new IndexOutOfRangeException(
                    string.Format(
                        DbUpdatableRecordErrorMessages.ADP_InvalidDataLength,
                        ((long)length).ToString(CultureInfo.InvariantCulture)
                    )
                );
            }
            else
            {
                byteCount = 0;
            }
            return byteCount;
        }

        public char GetChar(int ordinal) => (char) GetValue(ordinal);
        [SuppressMessage(""Microsoft.Usage"", ""CA2201:DoNotRaiseReservedExceptionTypes"")]

        public long GetChars(int ordinal, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            char[] tempBuffer;
            tempBuffer = (char[])GetValue(ordinal);

            if (buffer == null)
            {
                return tempBuffer.Length;
            }

            var srcIndex = (int)dataIndex;
            var charCount = Math.Min(tempBuffer.Length - srcIndex, length);
            if (srcIndex < 0)
            {
                throw new ArgumentOutOfRangeException(
                    ""dataIndex"",
                    string.Format(
                        DbUpdatableRecordErrorMessages.ADP_InvalidSourceBufferIndex,
                        tempBuffer.Length.ToString(CultureInfo.InvariantCulture),
                        ((long) srcIndex).ToString(CultureInfo.InvariantCulture)
                    )
                );
            }
            else if ((bufferIndex < 0)
                     || (bufferIndex > 0 && bufferIndex >= buffer.Length))
            {
                throw new ArgumentOutOfRangeException(
                    ""bufferIndex"",
                    string.Format(
                        DbUpdatableRecordErrorMessages.ADP_InvalidDestinationBufferIndex,
                        buffer.Length.ToString(CultureInfo.InvariantCulture),
                        bufferIndex.ToString(CultureInfo.InvariantCulture)
                    )
                );
            }

            if (0 < charCount)
            {
                Array.Copy(tempBuffer, dataIndex, buffer, bufferIndex, charCount);
            }
            else if (length < 0)
            {
                throw new IndexOutOfRangeException(
                    string.Format(
                        DbUpdatableRecordErrorMessages.ADP_InvalidDataLength,
                        ((long)length).ToString(CultureInfo.InvariantCulture)
                    )
                );
            }
            else
            {
                charCount = 0;
            }
            return charCount;
        }
        IDataReader IDataRecord.GetData(int ordinal) => throw new NotSupportedException();
        protected DbDataReader GetDbDataReader(int ordinal) => throw new NotSupportedException();

        public string GetDataTypeName(int ordinal) => (GetFieldType(ordinal)).Name;

        public DateTime GetDateTime(int ordinal) => (DateTime) GetValue(ordinal);

        public Decimal GetDecimal(int ordinal) => (Decimal) GetValue(ordinal);

        public double GetDouble(int ordinal) => (double) GetValue(ordinal);

        public Type GetFieldType(int ordinal) => GetValue(ordinal).GetType();

        public float GetFloat(int ordinal) => (float) GetValue(ordinal);

        public Guid GetGuid(int ordinal) => (Guid) GetValue(ordinal);

        public Int16 GetInt16(int ordinal) => (Int16) GetValue(ordinal);

        public Int32 GetInt32(int ordinal) => (Int32) GetValue(ordinal);

        public Int64 GetInt64(int ordinal) => (Int64) GetValue(ordinal);

        public string GetName(int ordinal) => ColumnNames[ordinal];

        public int GetOrdinal(string name) => ColumnNames.IndexOf(name);

        public string GetString(int ordinal) => (string) GetValue(ordinal);

        public object GetValue(int ordinal) => Values[ordinal];

        public int GetValues(object[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            var count = Math.Min(Values.Count, values.Length);
            for (var ordinal = 0; ordinal < count; ordinal++)
                values[ordinal] = GetValue(ordinal);
            return count;
        }

        public bool IsDBNull(int ordinal) => (GetValue(ordinal) == DBNull.Value);

        public void SetBoolean(int ordinal, bool value) => SetValue(ordinal, value);

        public void SetByte(int ordinal, byte value) => SetValue(ordinal, value);

        public void SetChar(int ordinal, char value) => SetValue(ordinal, value);

        public void SetDataRecord(int ordinal, IDataRecord value) => SetValue(ordinal, value);

        public void SetDateTime(int ordinal, DateTime value) => SetValue(ordinal, value);

        public void SetDecimal(int ordinal, Decimal value) => SetValue(ordinal, value);

        public void SetDouble(int ordinal, Double value) => SetValue(ordinal, value);

        public void SetFloat(int ordinal, float value) => SetValue(ordinal, value);

        public void SetGuid(int ordinal, Guid value) => SetValue(ordinal, value);

        public void SetInt16(int ordinal, Int16 value) => SetValue(ordinal, value);

        public void SetInt32(int ordinal, Int32 value) => SetValue(ordinal, value);

        public void SetInt64(int ordinal, Int64 value) => SetValue(ordinal, value);

        public void SetString(int ordinal, string value) => SetValue(ordinal, value);

        public void SetValue(int ordinal, object value) => SetRecordValue(ordinal, value);

        public int SetValues(params Object[] values)
        {
            var minValue = Math.Min(values.Length, FieldCount);
            for (var i = 0; i < minValue; i++)
            {
                SetRecordValue(i, values[i]);
            }
            return minValue;
        }

        public void SetDBNull(int ordinal) => SetRecordValue(ordinal, DBNull.Value);

        public DbDataRecord GetDataRecord(int ordinal) => (DbDataRecord) GetValue(ordinal);

        public DbDataReader GetDataReader(int ordinal) => GetDbDataReader(ordinal);

        protected abstract void SetRecordValue(int ordinal, object value);
    }

    public class BpgsqlDbRecord : BpgsqlDbUpdatableRecord
    {
        private PropertyValueReader reader;
        private PropertyValueWriter writer;
        private List<PropertyValue> Properties;

        public BpgsqlDbRecord() : base()
        {
            reader = new PropertyValueReader();
            writer = new PropertyValueWriter();
            Properties = new List<PropertyValue>();
        }

        public void AddProperty(PropertyValue property)
        {
            var value = reader.Read(property);
            Record[property.name] = value;
            ColumnNames.Add(property.name);
            Values.Add(value);
            Properties.Add(property);
        }

        protected override void SetRecordValue(int ordinal, object value)
        {
            Values[ordinal] = value;
            Record[ColumnNames[ordinal]] = value;
            writer.Write(value, Properties[ordinal]);
        }

        public object GetValue(string name) => Record[name];
    }

    static List<BpgsqlDbRecord> records = new List<BpgsqlDbRecord>();

    public static List<BpgsqlDbRecord> Execute(string cmd, long limit)
    {
        SPI.pldotnet_SPIExecute(cmd, limit);
        return SPI.records;
    }

    public static void ResetFuncRecords()
    {
        SPI.records = new List<BpgsqlDbRecord>();
    }

    public static void AddProperty(IntPtr arg, int funcoid)
    {
        PropertyValue prop = Marshal.PtrToStructure<PropertyValue>(arg);
        if(SPI.records.Count < prop.nrow + 1)
        {
            SPI.records.Add(new BpgsqlDbRecord());
        }
        BpgsqlDbRecord record = SPI.records[prop.nrow];
        record.AddProperty(prop);
    }
}";
            LibArgs libArgs = Marshal.PtrToStructure<LibArgs>(arg);
            string sourceCode = Marshal.PtrToStringAuto(libArgs.SourceCode);

            if (Engine.funcBuiltCodeDict == null)
                Engine.funcBuiltCodeDict = new Dictionary<uint, CachedFunction>();
            else {
                // Code has not changed then it is not needed to build it
                try {
                    Engine.funcBuiltCodeDict.TryGetValue(libArgs.FuncOid, out CachedFunction cached);
                    if  (cached.SourceCode == sourceCode) {
                        Engine.funcOid = libArgs.FuncOid;
                        Engine.cachedFunction = cached;
                        Engine.userFunction = Engine.cachedFunction.CallFunction;
                        return 0;
                    }
                }catch{}
            }

            if (Engine.RetrieveFromRemoteStorage(sourceCode, libArgs.FuncOid))
            {
                return 0;
            }

            SyntaxTree userTree = SyntaxFactory.ParseSyntaxTree(sourceCode);

            NamespaceDeclarationSyntax parentNamespace =
                userTree.GetRoot().DescendantNodes()
                .OfType<NamespaceDeclarationSyntax>().FirstOrDefault();

            ClassDeclarationSyntax newSPIClassNode =
                SyntaxFactory.ParseSyntaxTree(spiSrc).GetRoot()
                .DescendantNodes().OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();

            SyntaxNode node = userTree.GetRoot().ReplaceNode(parentNamespace,parentNamespace.AddMembers(newSPIClassNode).NormalizeWhitespace());

            var rawSourceCode = node.ToFullString();
            userTree = SyntaxFactory.ParseSyntaxTree(rawSourceCode);

            var trustedAssembliesPaths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
                .Split(Path.PathSeparator);

            var neededAssemblies = new[]
            {
                "System.Runtime",
                "System.Private.CoreLib",
                "System.Console",
                "System.Linq",
                "System.Data.SqlClient",
                "System.Data",
                "System.Data.Common",
                "System.Collections.Generic",
                "System.Diagnostics.CodeAnalysis",
                "System.Globalization"
            };

            List<PortableExecutableReference> references = trustedAssembliesPaths
                .Where(p => neededAssemblies.Contains(Path.GetFileNameWithoutExtension(p)))
                .Select(p => MetadataReference.CreateFromFile(p))
            .ToList();

            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Release)
                .WithConcurrentBuild(true);

            CSharpCompilation compilation = CSharpCompilation.Create(
                "plnetproc.dll",
                options: compilationOptions,
                syntaxTrees: new[] { userTree },
                references: references);

            Engine.memStream = new MemoryStream();
            Microsoft.CodeAnalysis.Emit.EmitResult compileResult = compilation.Emit(Engine.memStream);

            if(!compileResult.Success)
            {
                var lines = rawSourceCode.Split('\n');
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("\n********ERROR************\n");
                foreach(var diagnostic in compileResult.Diagnostics)
                    sb.AppendLine(GetCompilationError(diagnostic, lines));
                sb.AppendLine("\n********ERROR************\n");
                pldotnet_Warning(sb.ToString());
                return 1;
            }

            Engine.SetDelegate(Engine.memStream, sourceCode, libArgs.FuncOid);

            Engine.SendToRemoteStorage(sourceCode, libArgs.FuncOid);

            Engine.funcBuiltCodeDict[libArgs.FuncOid] = Engine.cachedFunction;

            return 0;
        }

        public static string GetCompilationError(Diagnostic diagnostic, string[] lines)
        {
            string pattern = @"\d+,\d+";
            string message = diagnostic.ToString();
            Match m = Regex.Match(message, pattern, RegexOptions.IgnoreCase);
            if (m.Success)
            {
                var sb = new System.Text.StringBuilder();
                var split = m.Value.Split(',');
                if (split.Length > 0)
                {
                    var l0 = Int32.Parse(split[0]);
                    var line = lines[l0 - 1].TrimEnd();
                    sb.AppendLine($" > {line}");
                    sb.AppendLine($" ^ {message}");
                    return sb.ToString();
                }
            }

            return message;
        }

        public static int InvokeAddProperty(IntPtr arg, int argLength)
        {
            if (needsReset)
            {
                Engine.cachedFunction.ResetFuncRecords();
                needsReset = false;
            }
            Engine.cachedFunction.AddProperty(arg, argLength);
            return 0;
        }

        public static int Run(IntPtr arg, int argLength)
        {
            // The functionID is an additional int32_t appended into arg
            var functionId = (uint) Marshal.ReadInt32(arg, argLength);

            if (functionId != Engine.funcOid)
            {
                try {
                    if (Engine.funcBuiltCodeDict.TryGetValue(functionId, out CachedFunction cached))
                    {
                        Engine.funcOid = functionId;
                        Engine.userFunction = cached.CallFunction;
                        Engine.cachedFunction = cached;
                        Engine.needsReset = true;
                        cached.CallFunction(arg, argLength);
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }

                }catch{
                    return 2;
                }
            }

            Engine.needsReset = true;
            Engine.userFunction(arg, argLength);
            return 0;
        }

        static private void SendToRemoteStorage(string sourceCode, uint functionId)
        {
            try
            {
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

                // The port number(5000) must match the port of the gRPC server.
                if (Engine.client == null)
                {
                    var channel = GrpcChannel.ForAddress("http://localhost:5000");
                    Engine.client =  new Storage.StorageClient(channel);
                }
                var reply = Engine.client.Save(
                            new StorageRequest {
                                Procedure = new ProcedureInfo {
                                    FunctionId = functionId,
                                    Source = sourceCode
                                },
                                Content = new StreamContent {
                                    Asm = ByteString.CopyFrom(Engine.memStream.GetBuffer())
                                }
                            }
                );
            }
            catch
            {
            }
        }

        static private bool RetrieveFromRemoteStorage(string sourceCode, uint functionId)
        {
            try
            {
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

                // The port number(5000) must match the port of the gRPC server.
                if (Engine.client == null)
                {
                    var channel = GrpcChannel.ForAddress("http://localhost:5000");
                    Engine.client =  new Storage.StorageClient(channel);
                }
                var reply = Engine.client.Retrieve(
                                new ProcedureInfo {
                                    FunctionId = functionId,
                                    Source =sourceCode
                                }
                );
                if (reply.Asm.Length > 0) {
                    var buffer = reply.Asm.ToByteArray();
                    if (buffer.Length > 500)
                    {
                        Engine.memStream = new MemoryStream();
                        Engine.memStream.Write(buffer, 0, buffer.Length);
                        if (Engine.funcBuiltCodeDict == null)
                        {
                            Engine.funcBuiltCodeDict = new Dictionary<uint, CachedFunction>();
                        }
                        Engine.SetDelegate(Engine.memStream, sourceCode, functionId);
                        Engine.funcBuiltCodeDict[functionId] = Engine.cachedFunction;
                        return true;
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        public static void SetDelegate(MemoryStream memoryStream, string sourceCode, uint functionId)
        {
            Engine.compiledAssembly = Assembly.Load(memStream.GetBuffer());

            Type procClassType = Engine.compiledAssembly.GetType("PlDotNETUserSpace.UserClass");
            MethodInfo procMethod = procClassType.GetMethod("CallFunction");
            Type procClassType2 = Engine.compiledAssembly.GetType("PlDotNETUserSpace.SPI");
            MethodInfo procMethod2 = procClassType2.GetMethod("AddProperty");
            MethodInfo procMethod3 = procClassType2.GetMethod("ResetFuncRecords");

            Engine.cachedFunction = new CachedFunction() {
                SourceCode = sourceCode,
                CallFunction = (Func<IntPtr, int, int>) Delegate.CreateDelegate(
                    typeof(Func<IntPtr, int, int>),
                    null,
                    procMethod
                ),
                AddProperty = (Action<IntPtr, int>) Delegate.CreateDelegate(
                    typeof(Action<IntPtr, int>),
                    null,
                    procMethod2
                ),
                ResetFuncRecords = (Action) Delegate.CreateDelegate(
                    typeof(Action),
                    null,
                    procMethod3
                ),
            };

            Engine.userFunction = cachedFunction.CallFunction;
            Engine.funcOid = functionId;

            // Elog functions
            Engine.SetElogFunctions(procClassType);
        }

        public static void SetElogFunctions(Type procClassType)
        {
            MethodInfo setInfoMethod = procClassType.GetMethod("SetInfo");

            Action<Action<string>> setInfo = (Action<Action<string>>) Delegate.CreateDelegate(
                typeof(Action<Action<string>>),
                null,
                setInfoMethod
            );

            setInfo((Action<string>) pldotnet_Info);

            MethodInfo setWarningMethod = procClassType.GetMethod("SetWarning");

            Action<Action<string>> setWarning = (Action<Action<string>>) Delegate.CreateDelegate(
                typeof(Action<Action<string>>),
                null,
                setWarningMethod
            );

            setWarning((Action<string>) pldotnet_Warning);
        }
    }
}
