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
            public Action ResetFuncExpandDo;

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
    static List<dynamic> funcExpandDo = new List<dynamic>();

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

    public static List<dynamic> Execute(string cmd, long limit)
    {
        SPI.pldotnet_SPIExecute(cmd, limit);
        return SPI.funcExpandDo;
    }

    public static void ResetFuncExpandDo()
    {
        SPI.funcExpandDo = new List<dynamic>();
    }

    public static T ReadValue<T>(IntPtr handle)
    {
        if (typeof(T) == typeof(string))
        {
            return (T)(object)Marshal.PtrToStringUTF8(handle);
        }
        if (typeof(T) == typeof(decimal))
        {
            return (T)(object)Convert.ToDecimal(Marshal.PtrToStringAnsi(handle));
        }
        return Marshal.PtrToStructure<T>(handle);
    }
    public static void AddProperty(IntPtr arg, int funcoid)
    {
        PropertyValue prop = Marshal.PtrToStructure<PropertyValue>(arg);
        if(SPI.funcExpandDo.Count < prop.nrow + 1)
        {
            SPI.funcExpandDo.Add(new ExpandoObject());
        }
        switch((TypeOid)prop.type)
        {
            case TypeOid.BOOLOID:
                ((IDictionary<String,Object>)SPI.funcExpandDo[prop.nrow])
                    .Add(prop.name, SPI.ReadValue<bool>(prop.value) );
                break;
            case TypeOid.INT2OID:
                ((IDictionary<String,Object>)SPI.funcExpandDo[prop.nrow])
                    .Add(prop.name, SPI.ReadValue<short>(prop.value) );
                break;
            case TypeOid.INT4OID:
                ((IDictionary<String,Object>)SPI.funcExpandDo[prop.nrow])
                    .Add(prop.name, SPI.ReadValue<int>(prop.value) );
                break;
            case TypeOid.INT8OID:
                ((IDictionary<String,Object>)SPI.funcExpandDo[prop.nrow])
                    .Add(prop.name, SPI.ReadValue<long>(prop.value) );
                break;
            case TypeOid.FLOAT4OID:
                ((IDictionary<String,Object>)SPI.funcExpandDo[prop.nrow])
                    .Add(prop.name, SPI.ReadValue<float>(prop.value));
                        break;
            case TypeOid.FLOAT8OID:
                ((IDictionary<String,Object>)SPI.funcExpandDo[prop.nrow])
                    .Add(prop.name, SPI.ReadValue<double>(prop.value));
                break;
            case TypeOid.NUMERICOID:
                ((IDictionary<String,Object>)SPI.funcExpandDo[prop.nrow])
                    .Add(prop.name, SPI.ReadValue<decimal>(prop.value) );
                break;
            case TypeOid.VARCHAROID:
                ((IDictionary<String,Object>)SPI.funcExpandDo[prop.nrow])
                    .Add(prop.name, SPI.ReadValue<string>(prop.value) );
                break;
        }
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
                Engine.cachedFunction.ResetFuncExpandDo();
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
            MethodInfo procMethod3 = procClassType2.GetMethod("ResetFuncExpandDo");

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
                ResetFuncExpandDo = (Action) Delegate.CreateDelegate(
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
