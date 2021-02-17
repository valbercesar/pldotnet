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

    public class DbRecord : IDataRecord
    {
        private Dictionary<string, object> Record;
        private List<object> Values;
        private List<string> ColumnNames;
        private List<PropertyValue> properties;
        private PropertyValueReader reader;

        public DbRecord()
        {
            Record = new Dictionary<string, object>();
            Values = new List<object>();
            ColumnNames = new List<string>();
            reader = new PropertyValueReader();
        }

        public void AddProperty(PropertyValue prop)
        {
            var value = reader.Read(prop);
            if (Record.TryAdd(prop.name, value))
            {
                Values.Add(value);
                ColumnNames.Add(prop.name);
            }
        }

        public object this[int ordinal] => GetValue(ordinal);
        public object this[string name] => GetValue(name);
        public int FieldCount => Values.Count;
        public bool GetBoolean(int ordinal) => (bool) GetValue(ordinal);
        public byte GetByte(int ordinal) => (byte) GetValue(ordinal);
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }
        public char GetChar(int ordinal) => (char) GetValue(ordinal);
        public long GetChars(int ordinal, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }
        public IDataReader GetData(int ordinal)
        {
            throw new NotImplementedException();
        }
        public string GetDataTypeName(int ordinal) => GetFieldType(ordinal).ToString();
        public DateTime GetDateTime(int ordinal) => (DateTime) GetValue(ordinal);
        public decimal GetDecimal(int ordinal) => (decimal) GetValue(ordinal);
        public double GetDouble(int ordinal) => (double) GetValue(ordinal);
        public Type GetFieldType(int ordinal) => GetValue(ordinal).GetType();
        public float GetFloat(int ordinal) => (float) GetValue(ordinal);
        public Guid GetGuid(int ordinal) => (Guid) GetValue(ordinal);
        public short GetInt16(int ordinal) => (short) GetValue(ordinal);
        public int GetInt32(int ordinal) => (int) GetValue(ordinal);
        public long GetInt64(int ordinal) => (long) GetValue(ordinal);
        public string GetName(int ordinal) => ColumnNames[ordinal];
        public int GetOrdinal(string name) => ColumnNames.IndexOf(name);
        public string GetString(int ordinal) => (string) GetValue(ordinal);
        public object GetValue(int ordinal) => Values[ordinal];
        public object GetValue(string name) => Record[name];
        public int GetValues(object[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            var count = Math.Min(Values.Count, values.Length);
            for (var ordinal = 0; ordinal < count; ordinal++)
                values[ordinal] = GetValue(ordinal);
            return count;
        }

        public bool IsDBNull(int ordinal) => null == GetValue(ordinal);
    }

    static List<DbRecord> records = new List<DbRecord>();

    public static List<DbRecord> Execute(string cmd, long limit)
    {
        SPI.pldotnet_SPIExecute(cmd, limit);
        return SPI.records;
    }

    public static void ResetFuncRecords()
    {
        SPI.records = new List<DbRecord>();
    }

    public static void AddProperty(IntPtr arg, int funcoid)
    {
        PropertyValue prop = Marshal.PtrToStructure<PropertyValue>(arg);
        if(SPI.records.Count < prop.nrow + 1)
        {
            SPI.records.Add(new DbRecord());
        }
        DbRecord record = SPI.records[prop.nrow];
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

            userTree = SyntaxFactory.ParseSyntaxTree(node.ToFullString());

            var trustedAssembliesPaths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator);

            var neededAssemblies = new[]
            {
                "System.Runtime",
                "System.Private.CoreLib",
                "System.Console",
                "System.Linq",
                "System.Data.SqlClient",
                "System.Data.Common",
                "System.Data",
                "System.ObjectModel",      /* For Expando/dynamic */
                "netstandard",             /* For Expando/dynamic */
                "System.Linq.Expressions", /* For Expando/dynamic */
                "Microsoft.CSharp",        /* For Expando/dynamic */
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
                Console.WriteLine("\n********ERROR************\n");
                foreach(var diagnostic in compileResult.Diagnostics)
                {
                    Console.WriteLine(diagnostic.ToString());
                }
                Console.WriteLine("\n********ERROR************\n");
                return 1;
            }

            Engine.SetDelegate(Engine.memStream, sourceCode, libArgs.FuncOid);

            Engine.SendToRemoteStorage(sourceCode, libArgs.FuncOid);

            Engine.funcBuiltCodeDict[libArgs.FuncOid] = Engine.cachedFunction;

            return 0;
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
        }
    }
}
