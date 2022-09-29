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
 * DotNetEngine/src/csharp/Engine.cs - pldotnet assembly compiler and runner
 *
 */

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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

using NpgsqlTypes;

namespace PlDotNET
{
    public static class Engine
    {
        public struct CachedFunction
        {
            public string SourceCode;
            public Action<IntPtr, IntPtr> UserProcedure;
        }

        static uint FunctionId;
        static MemoryStream MemStream;
        static IDictionary<uint, CachedFunction> FuncBuiltCodeDict;
        static CachedFunction Cached;
        static Action<IntPtr, IntPtr> UserProcedure;

        static string CSharpTemplatePath = "@CSHARP_TEMPLATE_DIR/csharp.tcs";

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

        public static List<Tuple<string, string>> GetSqlParamsFromString(string paramsStr)
        {
            string parameters = paramsStr == null ? "" : paramsStr;
            return parameters.Split(',')
                .Where(p => !string.IsNullOrEmpty(p))
                .Select(p =>
                {
                    var splitedBySpace = p.Split(' ');
                    return new Tuple<string, string>(splitedBySpace[1], splitedBySpace[0]);
                }).ToList();
        }

        public static string GetParamsString(List<Tuple<string, string>> sqlParams)
        {
            return string.Join(", ", sqlParams.Select((p) => $"{p.Item1} {p.Item2}").ToList());
        }

        public static string ParseTemplate(string rawFunctionDecl, string funcName, List<Tuple<string, string>> sqlParams, string returnType)
        {
            if (File.Exists(CSharpTemplatePath))
            {
                pldotnet_Info($"Loading template from {CSharpTemplatePath}");
                var template = File.ReadAllText(CSharpTemplatePath);
                var withFunctionDecl = template.Replace("// $user_function_declaration$", rawFunctionDecl);
                var withFunctionCall = withFunctionDecl.Replace("// $user_function_call$", BuildFunctionCall(funcName, sqlParams));
                return withFunctionCall.Replace("// $call_set_result$", BuildCallSetResult(returnType));
            }
            else
            {
                pldotnet_Info($"Template file {CSharpTemplatePath} not found");
            }
            return rawFunctionDecl;
        }

        public static string BuildFunctionCall(string funcName, List<Tuple<string, string>> sqlParams)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"{funcName}(");
            int argc = sqlParams.Count;

            for (int i = 0; i < argc; i++)
            {
                sb.Append($"({sqlParams[i].Item1}) argsList[{i}]");
                if (i < argc - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(");");
            return sb.ToString();
        }

        public static string BuildCallSetResult(string returnType)
        {
            if (returnType == "int")
            {
                return "pldotnet_SetInt32Result(result, result == null, output);";
            }
            else if (returnType == "bool")
            {
                return "pldotnet_SetBooleanResult(result, result == null, output);";
            }
            else if (returnType == "NpgsqlPolygon")
            {
                return "SetPolygonResult(result, output);";
            }
            else if(returnType == "short")
            {
                return "pldotnet_SetInt16Result(result, result == null, output);";
            }
            else if(returnType == "long")
            {
                return "pldotnet_SetInt64Result(result, result == null, output);";
            }
            else if(returnType == "float")
            {
                return "pldotnet_SetFloatResult(result, result == null, output);";
            }
            else if(returnType == "double")
            {
                return "pldotnet_SetDoubleResult(result, result == null, output);";
            }
            else
            {
                throw new NotImplementedException("Only int and boolean return types are supported");
            }
        }

        public static string BuildSourceCode(IntPtr Name, IntPtr ReturnType, IntPtr Params, IntPtr Body)
        {
            string name = Marshal.PtrToStringAuto(Name);
            string returnType = Marshal.PtrToStringAuto(ReturnType);

            var sqlParams = GetSqlParamsFromString(Marshal.PtrToStringAuto(Params));

            string paramsStr = GetParamsString(sqlParams);

            string body = Marshal.PtrToStringAuto(Body);

            pldotnet_Info($"Compiling function {name}");
            pldotnet_Info($"Return type: {returnType}");
            pldotnet_Info($"Params: {paramsStr}");
            pldotnet_Info($"Body: {body}");

            // dummy template, we need to use a real template later
            // including the boilerplate code for the user function
            string rawFunctionDecl = $"public static {returnType} {name}({paramsStr}) {{\n{body}\n}}";

            return ParseTemplate(rawFunctionDecl, name, sqlParams, returnType);
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

        public static Microsoft.CodeAnalysis.Emit.EmitResult CompileSourceCode(string sourceCode, MemoryStream MemStream)
        {
            pldotnet_Info("===========================");
            pldotnet_Info("Compiling source code");
            pldotnet_Info($"Source code: {sourceCode}");
            pldotnet_Info("===========================");

            var userTree = SyntaxFactory.ParseSyntaxTree(sourceCode);

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
                "System.Globalization",
                "Npgsql"
            };

            List<PortableExecutableReference> references = trustedAssembliesPaths
                .Where(p => neededAssemblies.Contains(Path.GetFileNameWithoutExtension(p)))
                .Select(p => MetadataReference.CreateFromFile(p))
                .ToList();

            references.Add(
                MetadataReference.CreateFromFile(
                    typeof(NpgsqlPoint).Assembly.Location
                )
            );

            foreach (var refer in references)
            {
                pldotnet_Info($"Reference: {refer.Display}");
            }

            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Release)
                .WithConcurrentBuild(true);

            CSharpCompilation compilation = CSharpCompilation.Create(
                "plnetproc.dll",
                options: compilationOptions,
                syntaxTrees: new[] { userTree },
                references: references);


            Microsoft.CodeAnalysis.Emit.EmitResult compileResult = compilation.Emit(MemStream);

            if (!compileResult.Success)
            {
                var lines = sourceCode.Split('\n');
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("\n********ERROR************\n");
                foreach (var diagnostic in compileResult.Diagnostics)
                    sb.AppendLine(GetCompilationError(diagnostic, lines));
                sb.AppendLine("\n********ERROR************\n");
                pldotnet_Warning(sb.ToString());
                return null;
            }

            return compileResult;
        }

        public delegate int DelCompileUserFunction(uint FunctionId, IntPtr Name, IntPtr ReturnType, IntPtr Params, IntPtr Body);

        public static int CompileUserFunction(uint FunctionId, IntPtr Name, IntPtr ReturnType, IntPtr Params, IntPtr Body)
        {
            string sourceCode = BuildSourceCode(Name, ReturnType, Params, Body);

            if (Engine.FuncBuiltCodeDict == null)
                Engine.FuncBuiltCodeDict = new Dictionary<uint, CachedFunction>();
            else
            {
                // Code has not changed then it is not needed to build it
                try
                {
                    Engine.FuncBuiltCodeDict.TryGetValue(FunctionId, out CachedFunction cached);
                    if (cached.SourceCode == sourceCode)
                    {
                        Engine.FunctionId = FunctionId;
                        Engine.Cached = cached;
                        Engine.UserProcedure = cached.UserProcedure;
                        return 0;
                    }
                }
                catch
                {
                }
            }

            SyntaxTree userTree = SyntaxFactory.ParseSyntaxTree(sourceCode);

            SyntaxNode node = userTree.GetRoot().NormalizeWhitespace();

            var rawSourceCode = node.ToFullString();

            Engine.MemStream = new MemoryStream();
            var compileResult = Engine.CompileSourceCode(rawSourceCode, Engine.MemStream);

            Engine.Cached = new CachedFunction()
            {
                SourceCode = rawSourceCode,
                UserProcedure = GetDirectDelegate(Engine.MemStream)
            };

            Engine.FunctionId = FunctionId;
            Engine.FuncBuiltCodeDict[FunctionId] = Engine.Cached;
            Engine.UserProcedure = Engine.Cached.UserProcedure;

            pldotnet_Info("================================");
            pldotnet_Info($"\nNormalized source code: \n{Engine.Cached.SourceCode}");
            pldotnet_Info("================================");

            return 0;
        }

        public static Action<IntPtr, IntPtr> GetDirectDelegate(MemoryStream memoryStream)
        {
            var compiledAssembly = Assembly.Load(memoryStream.GetBuffer());

            Type procClassType = compiledAssembly.GetType("PlDotNETUserSpace.UserClass");

            if (null == procClassType)
            {
                pldotnet_Warning($"Failed to get type PlDotNETUserSpace.UserClass");
                return null;
            }

            MethodInfo procMethod = procClassType.GetMethod("CallUserFunction");

            return (Action<IntPtr, IntPtr>)Delegate.CreateDelegate(
                typeof(Action<IntPtr, IntPtr>),
                null,
                procMethod
            );
        }

        /// <summary>
        /// This function should be called from C code
        /// It tries to get the cached function by its id.
        /// If it is not found, it returns a number different from zero
        /// Otherwise, it calls the user function compiled by Roslyn
        /// </summary>

        public static unsafe int RunUserFunction(uint functionId, IntPtr arguments, IntPtr output)
        {
            // TODO make it thread safe
            // if (functionId != Engine.FunctionId)
            // {
            //     try
            //     {
            //         pldotnet_Info("Inside try structure\n");
            //         if (Engine.FuncBuiltCodeDict.TryGetValue(functionId, out CachedFunction cached))
            //         {
            //             Engine.FunctionId = functionId;
            //             Engine.Cached = cached;
            //             Engine.UserProcedure = cached.UserProcedure;
            //         }
            //         else
            //         {
            //             return 1;
            //         }
            //     }
            //     catch
            //     {
            //         pldotnet_Info("Entering catch structure\n");
            //         return 2;
            //     }
            // }

            // not hread safe for now
            Engine.Cached.UserProcedure(arguments, output);

            return 0;
        }

        public delegate int DelRunUserFunction(uint functionId, IntPtr arguments, IntPtr output);
    }
}
