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
using PlDotNET_Handler;

namespace PlDotNET
{
    public static class Engine
    {
        public static bool AlwaysNullable = false;

        public struct CachedFunction
        {
            public string UserHandlerSourceCode;
            public string UserFunctionSourceCode;
            public Action<List<IntPtr>, IntPtr, bool[]> UserProcedure;
            public bool SupportNullInput;
            public AssemblyLoadContext UserAssemblyLoadContext;
        }

        public static Dictionary<OID, OID> HandleArray =
                       new Dictionary<OID, OID>()
        {
            {OID.BOOLARRAYOID, OID.BOOLOID},
            {OID.INT2ARRAYOID, OID.INT2OID},
            {OID.INT4ARRAYOID, OID.INT4OID},
            {OID.INT8ARRAYOID, OID.INT8OID},
            {OID.FLOAT4ARRAYOID, OID.FLOAT4OID},
            {OID.FLOAT8ARRAYOID, OID.FLOAT8OID},
            {OID.POINTARRAYOID, OID.POINTOID},
            {OID.LINEARRAYOID, OID.LINEOID},
            {OID.LSEGARRAYOID, OID.LSEGOID},
            {OID.BOXARRAYOID, OID.BOXOID},
            {OID.POLYGONARRAYOID, OID.POLYGONOID},
            {OID.TEXTARRAYOID, OID.TEXTOID},
            {OID.PATHARRAYOID, OID.PATHOID},
            {OID.CIRCLEARRAYOID, OID.CIRCLEOID},
            {OID.DATEARRAYOID, OID.DATEOID},
            {OID.TIMEARRAYOID, OID.TIMEOID},
            {OID.TIMETZARRAYOID, OID.TIMETZOID},
            {OID.TIMESTAMPARRAYOID, OID.TIMESTAMPOID},
            {OID.TIMESTAMPTZARRAYOID, OID.TIMESTAMPTZOID},
            {OID.INTERVALARRAYOID, OID.INTERVALOID},
            {OID.MACADDRARRAYOID, OID.MACADDROID},
            {OID.MACADDR8ARRAYOID, OID.MACADDR8OID},
            {OID.INETARRAYOID, OID.INETOID},
            {OID.CIDRARRAYOID, OID.CIDROID},
            {OID.MONEYARRAYOID, OID.MONEYOID},
            {OID.VARBITARRAYOID, OID.VARBITOID},
            {OID.BITARRAYOID, OID.BITOID},
            {OID.BYTEAARRAYOID, OID.BYTEAOID},
            {OID.BPCHARARRAYOID, OID.BPCHAROID},
            {OID.VARCHARARRAYOID, OID.VARCHAROID},
            {OID.XMLARRAYOID, OID.XMLOID},
            {OID.JSONARRAYOID, OID.JSONOID},
            {OID.UUIDARRAYOID, OID.UUIDOID},
            {OID.INT4RANGEARRAYOID, OID.INT4RANGEOID},
            {OID.NUMRANGEARRAYOID, OID.NUMRANGEOID},
            {OID.TSRANGEARRAYOID, OID.TSRANGEOID},
            {OID.TSTZRANGEARRAYOID, OID.TSTZRANGEOID},
            {OID.DATERANGEARRAYOID, OID.DATERANGEOID},
            {OID.INT8RANGEARRAYOID, OID.INT8RANGEOID},
            {OID.INT4MULTIRANGEARRAYOID, OID.INT4MULTIRANGEOID},
            {OID.NUMMULTIRANGEARRAYOID, OID.NUMMULTIRANGEOID},
            {OID.TSMULTIRANGEARRAYOID, OID.TSMULTIRANGEOID},
            {OID.TSTZMULTIRANGEARRAYOID, OID.TSTZMULTIRANGEOID},
            {OID.DATEMULTIRANGEARRAYOID, OID.DATEMULTIRANGEOID},
            {OID.INT8MULTIRANGEARRAYOID, OID.INT8MULTIRANGEOID},
        };

        public static Dictionary<OID, string> OidTypes =
                       new Dictionary<OID, string>()
        {
            {OID.BOOLOID,"bool"},
            {OID.INT2OID, "short"},
            {OID.INT4OID, "int"},
            {OID.INT8OID, "long"},
            {OID.FLOAT4OID, "float"},
            {OID.FLOAT8OID, "double"},
            {OID.POINTOID, "NpgsqlPoint"},
            {OID.LINEOID, "NpgsqlLine"},
            {OID.LSEGOID, "NpgsqlLSeg"},
            {OID.BOXOID, "NpgsqlBox"},
            {OID.POLYGONOID, "NpgsqlPolygon"},
            {OID.TEXTOID, "string"},
            {OID.PATHOID, "NpgsqlPath"},
            {OID.CIRCLEOID, "NpgsqlCircle"},
            {OID.DATEOID, "DateOnly"},
            {OID.TIMEOID, "TimeOnly"},
            {OID.TIMETZOID, "DateTimeOffset"},
            {OID.TIMESTAMPOID, "DateTime"},
            {OID.TIMESTAMPTZOID, "DateTime"},
            {OID.INTERVALOID, "NpgsqlInterval"},
            {OID.MACADDROID, "PhysicalAddress"},
            {OID.MACADDR8OID, "PhysicalAddress"},
            {OID.INETOID, "(IPAddress Address, int Netmask)"},
            {OID.CIDROID, "(IPAddress Address, int Netmask)"},
            {OID.MONEYOID, "decimal"},
            {OID.VARBITOID, "BitArray"},
            {OID.BITOID, "BitArray"},
            {OID.BYTEAOID, "byte[]"},
            {OID.BPCHAROID, "string"},
            {OID.VARCHAROID, "string"},
            {OID.XMLOID, "string"},
            {OID.JSONOID, "string"},
            {OID.UUIDOID, "Guid"},
            {OID.INT4RANGEOID, "NpgsqlRange<int>"},
            {OID.INT8RANGEOID, "NpgsqlRange<long>"},
            {OID.TSRANGEOID, "NpgsqlRange<DateTime>"},
            {OID.TSTZRANGEOID, "NpgsqlRange<DateTime>"},
            {OID.DATERANGEOID, "NpgsqlRange<DateOnly>"},
            {OID.VOIDOID, "void"}
        };

        public static IDictionary<uint, CachedFunction> FuncBuiltCodeDict = new Dictionary<uint, CachedFunction>();

        public static string CSharpTemplateUserHandler = "@CSHARP_TEMPLATE_DIR/UserHandler.tcs";

        public static string CSharpTemplateUserFunction = "@CSHARP_TEMPLATE_DIR/UserFunction.tcs";

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_Elog(int level, string nessage);

        /// <summary>
        /// Reports an information message in PostgreSQL.
        /// </summary> 
        public static void pldotnet_Info(string message)
        {
            pldotnet_Elog(17, message);
        }

        /// <summary>
        /// Reports an warning message in PostgreSQL.
        /// </summary> 
        public static void pldotnet_Warning(string message)
        {
            pldotnet_Elog(19, message);
        }

        /// <summary>
        /// It returns a list with of tuple, each tuple being the variable type
        /// and its name.
        /// </summary> 
        public static List<Tuple<string, string>> GetSqlParams(List<string> paramNames, List<int> paramTypes)
        {
            List<Tuple<string, string>> arguments = new List<Tuple<string, string>>();
            for (int i = 0; i < paramNames.Count(); i++)
            {
                string type = HandleArray.ContainsKey((OID)paramTypes[i]) ? "Array" : OidTypes[(OID)paramTypes[i]];
                arguments.Add(new Tuple<string, string>(type, paramNames[i]));
            }
            return arguments;
        }

        /// <summary>
        /// It creates the argument list of the user function.
        /// </summary> 
        public static string GetParamsString(List<Tuple<string, string>> sqlParams, bool supportNullInput)
        {
            if (supportNullInput || Engine.AlwaysNullable)
                return string.Join(", ", sqlParams.Select((p) => $"{p.Item1}? {p.Item2}").ToList());
            return string.Join(", ", sqlParams.Select((p) => $"{p.Item1} {p.Item2}").ToList());
        }

        /// <summary>
        /// Returns the handler object NAME for the specified OID.
        /// </summary> 
        public static string GetTypeHandler(int id)
        {
            switch (id)
            {
                case (int)OID.BOOLOID:
                    return "BoolHandler";
                case (int)OID.INT2OID:
                    return "ShortHandler";
                case (int)OID.INT4OID:
                    return "IntHandler";
                case (int)OID.INT8OID:
                    return "LongHandler";
                case (int)OID.FLOAT4OID:
                    return "FloatHandler";
                case (int)OID.FLOAT8OID:
                    return "DoubleHandler";
                case (int)OID.POINTOID:
                    return "PointHandler";
                case (int)OID.LINEOID:
                    return "LineHandler";
                case (int)OID.LSEGOID:
                    return "LineSegmentHandler";
                case (int)OID.BOXOID:
                    return "BoxHandler";
                case (int)OID.PATHOID:
                    return "PathHandler";
                case (int)OID.POLYGONOID:
                    return "PolygonHandler";
                case (int)OID.CIRCLEOID:
                    return "CircleHandler";
                case (int)OID.DATEOID:
                    return "DateHandler";
                case (int)OID.TIMEOID:
                    return "TimeHandler";
                case (int)OID.TIMETZOID:
                    return "TimeTzHandler";
                case (int)OID.TIMESTAMPOID:
                    return "TimestampHandler";
                case (int)OID.TIMESTAMPTZOID:
                    return "TimestampTzHandler";
                case (int)OID.INTERVALOID:
                    return "IntervalHandler";
                case (int)OID.MACADDROID:
                    return "MacaddrHandler";
                case (int)OID.MACADDR8OID:
                    return "Macaddr8Handler";
                case (int)OID.INETOID:
                    return "InetHandler";
                case (int)OID.CIDROID:
                    return "CidrHandler";
                case (int)OID.TEXTOID:
                    return "TextHandler";
                case (int)OID.MONEYOID:
                    return "MoneyHandler";
                case (int)OID.VARBITOID:
                    return "VarBitStringHandler";
                case (int)OID.BITOID:
                    return "BitStringHandler";
                case (int)OID.BYTEAOID:
                    return "ByteaHandler";
                case (int)OID.BPCHAROID:
                    return "CharHandler";
                case (int)OID.VARCHAROID:
                    return "CharVaryingHandler";
                case (int)OID.XMLOID:
                    return "XmlHandler";
                case (int)OID.JSONOID:
                    return "JsonHandler";
                case (int)OID.UUIDOID:
                    return "UuidHandler";
                case (int)OID.INT4RANGEOID:
                    return "IntRangeHandler";
                case (int)OID.INT8RANGEOID:
                    return "LongRangeHandler";
                case (int)OID.TSRANGEOID:
                    return "TimestampRangeHandler";
                case (int)OID.TSTZRANGEOID:
                    return "TimestampTzRangeHandler";
                case (int)OID.DATERANGEOID:
                    return "DateRangeHandler";
                default:
                    if (HandleArray.ContainsKey((OID)id))
                        return GetTypeHandler((int)HandleArray[(OID)id]);
                    throw new NotImplementedException($"Datum to {(OID)id} is not supported! Check GetTypeHandler");
            }
        }

        /// <summary>
        /// This function creates the code to call the handler objects, which
        /// do the process of converting a Postgres type to an equivalente .NET
        /// type. 
        /// </summary> 
        public static string BuildCreateArguments(string funcName, List<int> paramTypes, bool supportNullInput)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"// BEGIN create arguments for {funcName}");
            int argc = paramTypes.Count;

            for (int i = 0; i < argc; i++)
            {
                var argname = $"argument_{i}";
                var value = $"arguments[{i}]";
                var type = paramTypes[i];
                var typeHandler = GetTypeHandler(type);
                if (HandleArray.ContainsKey((OID)type))
                {
                    if (supportNullInput || Engine.AlwaysNullable)
                        sb.AppendLine($"var {argname} = {typeHandler}Obj.InputNullableArray({value}, isnull[{i}]);");
                    else
                        sb.AppendLine($"var {argname} = {typeHandler}Obj.InputArray({value});");
                }
                else
                {
                    if (supportNullInput || Engine.AlwaysNullable)
                        sb.AppendLine($"var {argname} = {typeHandler}Obj.InputNullableValue({value}, isnull[{i}]);");
                    else
                        sb.AppendLine($"var {argname} = {typeHandler}Obj.InputValue({value});");
                }
            }
            sb.Append($"// END create arguments for {funcName}");
            return sb.ToString();
        }

        /// <summary>
        /// This function creates code to call the user function.
        /// </summary> 
        public static string BuildFunctionCall(string funcName, List<Tuple<string, string>> sqlParams, bool supportNullInput)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"{funcName}(");
            int argc = sqlParams.Count;

            for (int i = 0; i < argc; i++)
            {
                if (supportNullInput || Engine.AlwaysNullable)
                    sb.Append($"({sqlParams[i].Item1}?) argument_{i}");
                else
                    sb.Append($"({sqlParams[i].Item1}) argument_{i}");
                if (i < argc - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(");");
            return sb.ToString();
        }

        /// <summary>
        /// This function returns the code to create the Datum result according
        /// to the OID of the result function. It also adds the code to set the
        /// Datum object to the function output.
        /// </summary> 
        public static string BuildCallSetResult(int id)
        {
            if ((OID)id == OID.VOIDOID)
                return ("var resultDatum = pldotnet_createDatumVoid();\n"
                + "pldotnet_SetDatumResult(resultDatum, false, output);");

            string setResult;
            if (HandleArray.ContainsKey((OID)id))
                setResult = $"var resultDatum = {GetTypeHandler(id)}Obj.OutputNullableArray(result);";
            else
                setResult = $"var resultDatum = {GetTypeHandler(id)}Obj.OutputNullableValue(result);";

            setResult += "pldotnet_SetDatumResult(resultDatum, result == null, output);";
            return setResult;
        }

        /// <summary>
        /// Returns the code to create the handler object that will be used.
        /// </summary> 
        public static string BuildHandlerObjects(List<int> inputTypes, int outputType)
        {
            List<string> allHandlers = new List<string>();

            if ((OID)outputType != OID.VOIDOID)
                allHandlers.Add(GetTypeHandler(outputType));

            for (int i = 0; i < inputTypes.Count; i++)
                allHandlers.Add(GetTypeHandler(inputTypes[i]));

            List<string> filteredHandlers = allHandlers.Distinct().ToList();

            string handlerObjects = "";

            for (int i = 0; i < filteredHandlers.Count; i++)
            {
                string handler = filteredHandlers[i];
                handlerObjects += $"public static {handler} {handler}Obj = new {handler}();\n";
            }

            return handlerObjects;
        }

        /// <summary>
        /// It builds the source codes from the template files and the user
        /// function information received from the C code.
        /// </summary>
        /// <remarks>
        /// If the user provides an assembly file, "userFunction" will be the provided
        /// SQL function body, i.e., 'UserAssembly.dll:UserNamespace.UserClass!FunctionName'.
        /// </remarks>
        public static unsafe (string userHandler, string userFunction) BuildSourceCodes(IntPtr name, int returnTypeID, IntPtr paramNames, int* paramTypes, IntPtr body, bool supportNullInput)
        {
            string funcName = Marshal.PtrToStringAuto(name);
            string returnType = HandleArray.ContainsKey((OID)returnTypeID) ? "Array" : OidTypes[(OID)returnTypeID];
            string parameters = Marshal.PtrToStringAuto(paramNames);
            List<string> paramNameList = new List<string>();
            List<int> paramTypeList = new List<int>();
            if (parameters != null)
            {
                paramNameList.AddRange(parameters.Split(" "));
                for (int i = 0; i < paramNameList.Count(); i++)
                {
                    paramTypeList.Add(paramTypes[i]);
                }
            }
            var sqlParams = GetSqlParams(paramNameList, paramTypeList);
            string paramsStr = GetParamsString(sqlParams, supportNullInput);
            string bodyStr = Marshal.PtrToStringAuto(body);

            bool providedAssembly = bodyStr.Contains(".dll");
            string nampespace = "UserSpace";
            string className = "UserFunction";
            if (providedAssembly)
            {
                string[] bodySplit = bodyStr.Split(':');
                bodySplit = bodySplit[1].Split('.');
                nampespace = bodySplit[0]; // UserNamespace
                bodySplit = bodySplit[1].Split('!');
                className = bodySplit[0]; // UserClass
                funcName = bodySplit[1]; // FunctionName
            }

            if (!File.Exists(CSharpTemplateUserHandler))
            {
                string msg = $"Csharp template file '{CSharpTemplateUserHandler}' not found";
                throw new SystemException(msg);
            }

            string userHandlerCode = File.ReadAllText(CSharpTemplateUserHandler);
            string nullAbleOutput = returnType == "void" ? "" : "var result =";
            userHandlerCode = userHandlerCode.Replace("// $create_arguments", BuildCreateArguments(funcName, paramTypeList, supportNullInput));
            userHandlerCode = userHandlerCode.Replace("// $user_function_call$", $"{nullAbleOutput} {className}." + BuildFunctionCall(funcName, sqlParams, supportNullInput));
            userHandlerCode = userHandlerCode.Replace("// $call_set_result$", BuildCallSetResult(returnTypeID));
            userHandlerCode = userHandlerCode.Replace("// $handler_objects$", BuildHandlerObjects(paramTypeList, returnTypeID));

            if (providedAssembly)
            {
                userHandlerCode = userHandlerCode.Replace("// $user_namespace$", $"using {nampespace};\n");
                return (userHandlerCode, bodyStr);
            }

            if (!File.Exists(CSharpTemplateUserFunction))
            {
                string msg = $"Csharp template file '{CSharpTemplateUserFunction}' not found";
                throw new SystemException(msg);
            }

            nullAbleOutput = returnType == "void" ? "" : "?";
            string rawFunctionDecl = $"public static {returnType}{nullAbleOutput} {funcName}({paramsStr}) {{\n#line 1\n{bodyStr}\n}}";
            string userFunctionCode = File.ReadAllText(CSharpTemplateUserFunction);
            userFunctionCode = userFunctionCode.Replace("// $user_function_declaration$", rawFunctionDecl);

            return (userHandlerCode, userFunctionCode);
        }

        /// <summary>
        /// This function returns the compilation errors reported during the
        /// compilation of the dynamic code using Roslyn.
        /// </summary>  
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

        /// <summary>
        /// This function compiles the dynamic code using Roslyn.
        /// </summary>   
        public static Microsoft.CodeAnalysis.Emit.EmitResult CompileSourceCode(string sourceCode, MemoryStream memStream, string assemblyName, MemoryStream memStreamUserFunction = null)
        {
            SyntaxTree userTree = SyntaxFactory.ParseSyntaxTree(sourceCode);
            SyntaxNode node = userTree.GetRoot().NormalizeWhitespace();
            sourceCode = node.ToFullString();

            pldotnet_Info("===========================");
            pldotnet_Info("Compiling source code");
            pldotnet_Info($"Source code:\n{sourceCode}");
            pldotnet_Info("===========================");

            var trustedAssembliesPathsArray = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator);
            List<string> trustedAssembliesPaths = new();
            trustedAssembliesPaths.AddRange(trustedAssembliesPathsArray);
            trustedAssembliesPaths.Add(typeof(NpgsqlPoint).Assembly.Location);
            trustedAssembliesPaths.Add(typeof(Engine).Assembly.Location);

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
                "Npgsql",
                "System.Text",
                "System.Buffers",
                "System.Text.Unicode",
                "System.Linq",
                "System.Text",
                "System.Buffers",
                "System.Text.Unicode",
                "System.Diagnostics",
                "System.Net.NetworkInformation",
                "System.Net.Primitives",
                "PlDotNET",
                "System.Core",
                "System.Linq.Expressions",
                "Microsoft.CSharp",
                "System.Collections"
            };

            List<PortableExecutableReference> references = trustedAssembliesPaths
                .Where(p => neededAssemblies.Contains(Path.GetFileNameWithoutExtension(p)))
                .Select(p => MetadataReference.CreateFromFile(p))
                .ToList();

            if (memStreamUserFunction != null)
                references.Add(MetadataReference.CreateFromStream(new MemoryStream(memStreamUserFunction.GetBuffer())));

            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Release)
                .WithConcurrentBuild(true).WithAllowUnsafe(true);

            CSharpCompilation compilation = CSharpCompilation.Create(
                $"{assemblyName}.dll",
                options: compilationOptions,
                syntaxTrees: new[] { userTree },
                references: references);

            Microsoft.CodeAnalysis.Emit.EmitResult compileResult = compilation.Emit(memStream);

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

        public unsafe delegate int DelCompileUserFunction(uint functionId, IntPtr name, int returnType, IntPtr paramNames, int* paramTypes, IntPtr body, [MarshalAs(UnmanagedType.I1)] bool supportNullInput);

        // <summary>
        /// This function is called called from C code and tries to create and
        /// compile the dynamic code using Roslyn. It also saves the
        /// CachedFunction in FuncBuiltCodeDict so that any compiled code can
        /// be called by the user function ID.
        /// </summary>
        public static unsafe int CompileUserFunction(uint functionId, IntPtr name, int returnType, IntPtr paramNames, int* paramTypes, IntPtr body, [MarshalAs(UnmanagedType.I1)] bool supportNullInput)
        {
            (string userHandler, string userFunction) sourceCode = BuildSourceCodes(name, returnType, paramNames, paramTypes, body, supportNullInput);

            bool useUserAssembly = sourceCode.userFunction.Contains(".dll");

            if (Engine.FuncBuiltCodeDict.TryGetValue(functionId, out CachedFunction cached))
            {
                if (cached.UserHandlerSourceCode == sourceCode.userHandler && cached.UserFunctionSourceCode == sourceCode.userFunction && !useUserAssembly)
                {
                    pldotnet_Info("User function hasn't changed, so it doesn't need to be recompiled!");
                    return 0;
                }
                else
                {
                    pldotnet_Info("User function has changed.");
                    FuncBuiltCodeDict[functionId].UserAssemblyLoadContext.Unload();
                    FuncBuiltCodeDict.Remove(functionId);
                }
            }

            MemoryStream memUserFunction = new MemoryStream();
            if (!useUserAssembly)
            {
                var compileResultUserFunction = CompileSourceCode(sourceCode.userFunction, memUserFunction, $"UserFunction_{functionId}");
            }
            else
            {
                string userAssemblyPath = sourceCode.userFunction.Split(":")[0];
                using (var fs = File.Open(userAssemblyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fs.CopyTo(memUserFunction);
                }
            }

            MemoryStream memUserHandler = new MemoryStream();
            var compileResultUserHandler = Engine.CompileSourceCode(sourceCode.userHandler, memUserHandler, $"UserHandler_{functionId}", memUserFunction);

            AssemblyLoadContext userAlc = new AssemblyLoadContext($"UserFunction_{functionId}", true);
            userAlc.LoadFromAssemblyPath(typeof(NpgsqlPoint).Assembly.Location);
            userAlc.LoadFromAssemblyPath(typeof(Engine).Assembly.Location);
            userAlc.LoadFromStream(new MemoryStream(memUserFunction.GetBuffer()));

            CachedFunction newCachedFunction = new CachedFunction()
            {
                UserFunctionSourceCode = sourceCode.userFunction,
                UserHandlerSourceCode = sourceCode.userHandler,
                SupportNullInput = supportNullInput,
                UserAssemblyLoadContext = userAlc,
                UserProcedure = GetDirectDelegate(memUserHandler, userAlc)
            };
            Engine.FuncBuiltCodeDict.Add(functionId, newCachedFunction);

            memUserFunction.Close();
            memUserHandler.Close();

            return 0;
        }

        /// <summary>
        /// It creates the Delegate function for the CallUserFunction function,
        /// which was compiled by Roslyn.
        /// </summary>
        public static Action<List<IntPtr>, IntPtr, bool[]> GetDirectDelegate(MemoryStream memoryStream, AssemblyLoadContext userAlc)
        {
            var compiledAssembly = userAlc.LoadFromStream(new MemoryStream(memoryStream.GetBuffer()));

            Type procClassType = compiledAssembly.GetType("UserSpace.UserHandler");

            if (null == procClassType)
            {
                pldotnet_Warning($"Failed to get type UserSpace.UserHandler");
                return null;
            }

            MethodInfo procMethod = procClassType.GetMethod("CallUserFunction");

            return (Action<List<IntPtr>, IntPtr, bool[]>)Delegate.CreateDelegate(
                typeof(Action<List<IntPtr>, IntPtr, bool[]>),
                null,
                procMethod
            );
        }

        /// <summary>
        /// This function is called called from C code and tries to get the
        /// cached function by the function id. If the cached functions is not
        /// found, an error message is reported. Otherwise, it calls the 
        /// function compiled by Roslyn.
        /// </summary>
        public static unsafe int RunUserFunction(uint functionId, IntPtr arguments, byte* nullmap, IntPtr output)
        {
            if (Engine.FuncBuiltCodeDict.TryGetValue(functionId, out CachedFunction cached))
            {
                GCHandle gchList = GCHandle.FromIntPtr(arguments);
                var argumentList = (List<IntPtr>)gchList.Target;
                bool[] isnull = new bool[argumentList.Count];
                if (cached.SupportNullInput || Engine.AlwaysNullable)
                    for (int i = 0, nargs = isnull.Length; i < nargs; i++)
                        isnull[i] = nullmap[i] == 0 ? false : true;

                cached.UserProcedure(argumentList, output, isnull);
            }
            else
            {
                pldotnet_Elog(21, $"[pldotnet]: could not find the generated function (ID: {functionId})");
            }

            return 0;
        }
        public unsafe delegate int DelRunUserFunction(uint functionId, IntPtr arguments, byte* nullmap, IntPtr output);

        /// <summary>
        /// Free memmory pointed by a IntPtr.
        /// </summary>
        public static unsafe void FreeGenericGCHandle(IntPtr p)
        {
            GCHandle gch = GCHandle.FromIntPtr(p);
            gch.Free();
        }
        public delegate void DelFreeGenericGCHandle(IntPtr p);

        /// <summary>
        /// This functions is called from C and creates a new list of IntPtr,
        /// which pldotnet adds the Datums and passes to RunUserFunction.
        /// </summary>
        public static unsafe System.IntPtr BuildDatumList()
        {
            var l = new List<IntPtr>();
            GCHandle handle = GCHandle.Alloc(l, GCHandleType.Normal);
            return GCHandle.ToIntPtr(handle);
        }
        public delegate System.IntPtr DelBuildDatumList();

        /// <summary>
        /// This functions is called from C and adds an IntPtr(Datum) to a list,
        /// of IntPtr. Pldotnet passes the final list to RunUserFunction.
        /// </summary>
        public static unsafe void AddDatumToList(System.IntPtr list, System.IntPtr datum)
        {
            GCHandle gchList = GCHandle.FromIntPtr(list);
            List<IntPtr> listObj = (List<IntPtr>)gchList.Target;
            listObj.Add(datum);
        }
        public delegate void DelAddDatumToList(System.IntPtr list, System.IntPtr datum);

        /// <summary>
        /// Unloads the assemblies of a specific function.
        /// </summary>
        public static void UnloadAssemblies(uint functionId)
        {
            if (FuncBuiltCodeDict.ContainsKey(functionId))
            {
                FuncBuiltCodeDict[functionId].UserAssemblyLoadContext.Unload();
                FuncBuiltCodeDict.Remove(functionId);
            }
            else
            {
                pldotnet_Elog(21, $"[pldotnet]: could not find the generated function (ID: {functionId})");
            }
        }
        public delegate void DelUnloadAssemblies(uint functionId);
    }
}
