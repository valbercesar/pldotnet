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
using System.Runtime.Loader;
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
            public Action<List<IntPtr>, IntPtr> UserProcedure;
        }

        static uint FunctionId;
        static MemoryStream MemStream;
        static IDictionary<uint, CachedFunction> FuncBuiltCodeDict;
        static CachedFunction Cached;
        static Action<List<IntPtr>, IntPtr> UserProcedure;

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

        public static string GetDatumConversionFunction(string dotnet_type)
        {
            switch (dotnet_type)
            {
                case "short":
                    return "pldotnet_getInt16";
                case "int":
                    return "pldotnet_getInt32";
                case "long":
                    return "pldotnet_getInt64";
                case "float":
                    return "pldotnet_getFloat";
                case "double":
                    return "pldotnet_getDouble";
                case "bool":
                    return "pldotnet_getBoolean";
                case "NpgsqlPoint":
                    return "pldotnet_BuildNpgsqlPoint";
                case "NpgsqlLine":
                    return "pldotnet_BuildNpgsqlLine";
                case "NpgsqlLSeg":
                    return "pldotnet_BuildNpgsqlLSeg";
                case "NpgsqlBox":
                    return "pldotnet_BuildNpgsqlBox";
                default:
                    throw new NotImplementedException($"Datum to {dotnet_type} is not supported! Check GetDatumConversionFunction");
            }
        }

        // DONUT
        public static string BuildCreateArguments(string funcName, List<Tuple<string, string>> sqlParams)
        {
            // WARNING: this is completely wrong.
            // - the individual IntPtr are not GCHandles, only Datum
            // - creating a GCHandle from it is wrong
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"// BEGIN create arguments for {funcName}");
            int argc = sqlParams.Count;

            for (int i = 0; i < argc; i++)
            {
                var argname = $"argument_{i}";
                var value = $"arguments[{i}]";
                var dotnet_type = sqlParams[i].Item1;
                var datum_conversion_function = GetDatumConversionFunction(dotnet_type);

                sb.AppendLine($"var {argname} = {datum_conversion_function}({value});");

            }
            sb.Append($"// END create arguments for {funcName}");
            return sb.ToString();
        }

        // DONUT
        public static string BuildFreeArguments(string funcName, List<Tuple<string, string>> sqlParams)
        {
            // TODO(rosicley/todd) - we need to check how we will free the list...
            //     // WARNING: this is completely wrong.
            // // - the individual IntPtr are not GCHandles, so there's nothing to free
            // // - The list needs to be pinned (currently is not)
            // // - Only the list needs to be freed
            //     var sb = new System.Text.StringBuilder();
            //     sb.AppendLine($"// BEGIN free arguments for {funcName}");
            //     int argc = sqlParams.Count;

            //     for (int i = 0; i < argc; i++)
            //     {
            //         var argname = $"argument_{i}";
            //         sb.AppendLine($"gch_{argname}.Free();");
            //     }

            //     sb.AppendLine($"// END free arguments for {funcName}");
            //     return sb.ToString();
            return "";
        }

        public static string BuildFunctionCall(string funcName, List<Tuple<string, string>> sqlParams)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"{funcName}(");
            int argc = sqlParams.Count;

            for (int i = 0; i < argc; i++)
            {
                sb.Append($"({sqlParams[i].Item1}) argument_{i}");
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
            string setResult = "var result_datum = ";
            switch (returnType)
            {
                case "short":
                    setResult +=
                    $"pldotnet_createDatumInt16(({returnType})result);\n";
                    break;
                case "int":
                    setResult +=
                    $"pldotnet_createDatumInt32(({returnType})result);\n";
                    break;
                case "long":
                    setResult +=
                    $"pldotnet_createDatumInt64(({returnType})result);\n";
                    break;
                case "float":
                    setResult +=
                    $"pldotnet_createDatumFloat(({returnType})result);\n";
                    break;
                case "double":
                    setResult +=
                    $"pldotnet_createDatumDouble(({returnType})result);\n";
                    break;
                case "bool":
                    setResult +=
                    $"pldotnet_createDatumBoolean(({returnType})result);\n";
                    break;
                case "NpgsqlPoint":
                    setResult +=
                    $"pldotnet_createDatumPoint((double)result.X, "
                    + "(double)result.Y);\n";
                    break;
                case "NpgsqlLine":
                    setResult +=
                    $"pldotnet_createDatumLine((double)result.A, "
                    + "(double)result.B,(double)result.C);\n";
                    break;
                case "NpgsqlLSeg":
                    setResult +=
                    $"pldotnet_createDatumLineSegment((double)result.Start.X,"
                    + "(double)result.Start.Y, (double)result.End.X, "
                    + "(double)result.End.Y);\n";
                    break;
                case "NpgsqlBox":
                    setResult +=
                    $"pldotnet_createDatumBox((double)result.UpperRight.X, "
                    +"(double)result.UpperRight.Y, (double)result.LowerLeft.X, "
                    + "(double)result.LowerLeft.Y);\n";
                    break;
                default:
                    throw new NotImplementedException($"It is not possible to return a {returnType} type! Check BuildCallSetResult.");
            }
            setResult += "pldotnet_SetDatumResult(result_datum, result_datum == null, output);";
            return setResult;
        }

        public static string BuildSourceCode(IntPtr Name, IntPtr ReturnType, IntPtr Params, IntPtr Body)
        {
            string funcName = Marshal.PtrToStringAuto(Name);
            string returnType = Marshal.PtrToStringAuto(ReturnType);

            var sqlParams = GetSqlParamsFromString(Marshal.PtrToStringAuto(Params));

            string paramsStr = GetParamsString(sqlParams);

            string body = Marshal.PtrToStringAuto(Body);

            pldotnet_Info($"Compiling function {funcName}");
            pldotnet_Info($"Return type: {returnType}");
            pldotnet_Info($"Params: {paramsStr}");
            pldotnet_Info($"Body: {body}");

            // dummy template, we need to use a real template later
            // including the boilerplate code for the user function
            string rawFunctionDecl = $"public static {returnType} {funcName}({paramsStr}) {{\n{body}\n}}";

            if (!File.Exists(CSharpTemplatePath))
            {
                string msg = $"Csharp template file '{CSharpTemplatePath}' not found";
                pldotnet_Info(msg);
                throw new SystemException(msg);
            }

            pldotnet_Info($"Loading template from {CSharpTemplatePath}");
            var template = File.ReadAllText(CSharpTemplatePath);
            var withArgumentsCreated = template.Replace("// $create_arguments", BuildCreateArguments(funcName, sqlParams));
            var withFunctionDecl = withArgumentsCreated.Replace("// $user_function_declaration$", rawFunctionDecl);
            var withFunctionCall = withFunctionDecl.Replace("// $user_function_call$", BuildFunctionCall(funcName, sqlParams));
            var withArgumentsDeleted = withFunctionCall.Replace("// $free_arguments", BuildFreeArguments(funcName, sqlParams));
            var withResultsSet = withArgumentsDeleted.Replace("// $call_set_result$", BuildCallSetResult(returnType));
            return withResultsSet;
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

            var trustedAssembliesPathsArray = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator);
            List<string> trustedAssembliesPaths = new();
            trustedAssembliesPaths.AddRange(trustedAssembliesPathsArray);
            trustedAssembliesPaths.Add(typeof(NpgsqlPoint).Assembly.Location);

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

            List<PortableExecutableReference> references = new List<PortableExecutableReference>();
            foreach (var na in neededAssemblies)
            {
                foreach (var tap in trustedAssembliesPaths)
                {
                    if (Path.GetFileNameWithoutExtension(tap) == na)
                    {
                        var mr = MetadataReference.CreateFromFile(tap);
                        pldotnet_Info($"trustedAssembliesPath {tap} matches neededAssembly {na}; adding MetadataReference {mr}");
                        references.Add(mr);
                    }
                }
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

            Assembly myAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(typeof(NpgsqlPoint).Assembly.Location);

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

        public static Action<List<IntPtr>, IntPtr> GetDirectDelegate(MemoryStream memoryStream)
        {
            var compiledAssembly = Assembly.Load(memoryStream.GetBuffer());

            Type procClassType = compiledAssembly.GetType("PlDotNETUserSpace.UserClass");

            if (null == procClassType)
            {
                pldotnet_Warning($"Failed to get type PlDotNETUserSpace.UserClass");
                return null;
            }

            MethodInfo procMethod = procClassType.GetMethod("CallUserFunction");

            return (Action<List<IntPtr>, IntPtr>)Delegate.CreateDelegate(
                typeof(Action<List<IntPtr>, IntPtr>),
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
            GCHandle gch_list = GCHandle.FromIntPtr(arguments);
            var argument_list = (List<IntPtr>)gch_list.Target;
            Engine.Cached.UserProcedure(argument_list, output);

            return 0;
        }

        public delegate int DelRunUserFunction(uint functionId, IntPtr arguments, IntPtr output);
    }
}
