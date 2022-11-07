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
        static bool AlwaysNullable = false;

        public struct CachedFunction
        {
            public string SourceCode;
            public Action<List<IntPtr>, IntPtr, bool[]> UserProcedure;
            public bool SupportNullInput;
        }

        static Dictionary<OID, OID> HANDLE_ARRAY =
                       new Dictionary<OID, OID>()
        {
            {OID.BOOLARRAYOID,        OID.BOOLOID},
            {OID.INT2ARRAYOID,        OID.INT2OID},
            {OID.INT4ARRAYOID,        OID.INT4OID},
            {OID.INT8ARRAYOID,        OID.INT8OID},
            {OID.FLOAT4ARRAYOID,      OID.FLOAT4OID},
            {OID.FLOAT8ARRAYOID,      OID.FLOAT8OID},
            {OID.POINTARRAYOID,       OID.POINTOID},
            {OID.LINEARRAYOID,        OID.LINEOID},
            {OID.LSEGARRAYOID,        OID.LSEGOID},
            {OID.BOXARRAYOID,         OID.BOXOID},
            {OID.POLYGONARRAYOID,     OID.POLYGONOID},
            {OID.TEXTARRAYOID,        OID.TEXTOID},
            {OID.PATHARRAYOID,        OID.PATHOID},
            {OID.CIRCLEARRAYOID,      OID.CIRCLEOID},
            {OID.DATEARRAYOID,        OID.DATEOID},
            {OID.TIMEARRAYOID,        OID.TIMEOID},
            {OID.TIMETZARRAYOID,      OID.TIMETZOID},
            {OID.TIMESTAMPARRAYOID,   OID.TIMESTAMPOID},
            {OID.TIMESTAMPTZARRAYOID, OID.TIMESTAMPTZOID},
            {OID.INTERVALARRAYOID,    OID.INTERVALOID},
            {OID.MACADDRARRAYOID,     OID.MACADDROID},
            {OID.MACADDR8ARRAYOID,    OID.MACADDR8OID},
            {OID.INETARRAYOID,        OID.INETOID},
            {OID.CIDRARRAYOID,        OID.CIDROID},
            {OID.MONEYARRAYOID,       OID.MONEYOID},
            {OID.VARBITARRAYOID,      OID.VARBITOID},
            {OID.BITARRAYOID,         OID.BITOID},
            {OID.BYTEAARRAYOID,       OID.BYTEAOID},
            {OID.BPCHARARRAYOID,      OID.BPCHAROID},
            {OID.VARCHARARRAYOID,     OID.VARCHAROID},
            {OID.XMLARRAYOID,         OID.XMLOID},
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

        static Dictionary<OID, string> OID_TYPES =
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
            {OID.INT4RANGEOID, "NpgsqlRange<int>"},
            {OID.INT8RANGEOID, "NpgsqlRange<long>"},
            {OID.TSRANGEOID, "NpgsqlRange<DateTime>"},
            {OID.TSTZRANGEOID, "NpgsqlRange<DateTime>"},
            {OID.DATERANGEOID, "NpgsqlRange<DateOnly>"},
            // {OID.NUMRANGEOID, "NpgsqlRange<Numeric>"}, // currently unimplemented
        };

        static uint FunctionId;
        static MemoryStream MemStream;
        static IDictionary<uint, CachedFunction> FuncBuiltCodeDict;
        static CachedFunction Cached;
        static Action<List<IntPtr>, IntPtr, bool[]> UserProcedure;
        static bool SupportNullInput;

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

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_typlenbyvalalign(int oid, ref short typlen, ref bool typbyval, ref byte typalign);

        public static Dictionary<int, short> typlens = new Dictionary<int, short>();
        public static Dictionary<int, bool> typbyvals = new Dictionary<int, bool>();
        public static Dictionary<int, byte> typaligns = new Dictionary<int, byte>();

        public static unsafe void add_typlenbyvalalign(int oid)
        {
            short typlen = 0;
            bool typbyval = false;
            byte typalign = 0;

            pldotnet_typlenbyvalalign(oid, ref typlen, ref typbyval, ref typalign);
            typlens[oid] = typlen;
            typbyvals[oid] = typbyval;
            typaligns[oid] = typalign;
        }

        // TODO - delete
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

        public static List<Tuple<string, string>> GetSqlParams(List<string> paramNames, List<int> paramTypes)
        {
            List<Tuple<string, string>> arguments = new List<Tuple<string, string>>();
            for (int i = 0; i < paramNames.Count(); i++)
            {
                pldotnet_Info($"THE TYPE : {paramTypes[i]}");
                string type = HANDLE_ARRAY.ContainsKey((OID)paramTypes[i]) ? "Array" : OID_TYPES[(OID)paramTypes[i]];
                arguments.Add(new Tuple<string, string>(type, paramNames[i]));
            }
            return arguments;
        }

        public static string GetParamsString(List<Tuple<string, string>> sqlParams)
        {
            if (Engine.SupportNullInput || Engine.AlwaysNullable)
                return string.Join(", ", sqlParams.Select((p) => $"{p.Item1}? {p.Item2}").ToList());
            return string.Join(", ", sqlParams.Select((p) => $"{p.Item1} {p.Item2}").ToList());
        }

        public static string GetTypeHandler(int id)
        {
            switch (id)
            {
                case (int)OID.BOOLOID:
                    return "bool_handler";
                case (int)OID.INT2OID:
                    return "short_handler";
                case (int)OID.INT4OID:
                    return "int_handler";
                case (int)OID.INT8OID:
                    return "long_handler";
                case (int)OID.FLOAT4OID:
                    return "float_handler";
                case (int)OID.FLOAT8OID:
                    return "double_handler";
                case (int)OID.POINTOID:
                    return "point_handler";
                case (int)OID.LINEOID:
                    return "line_handler";
                case (int)OID.LSEGOID:
                    return "lseg_handler";
                case (int)OID.BOXOID:
                    return "box_handler";
                case (int)OID.PATHOID:
                    return "path_handler";
                case (int)OID.POLYGONOID:
                    return "polygon_handler";
                case (int)OID.CIRCLEOID:
                    return "circle_handler";
                case (int)OID.DATEOID:
                    return "date_handler";
                case (int)OID.TIMEOID:
                    return "time_handler";
                case (int)OID.TIMETZOID:
                    return "timetz_handler";
                case (int)OID.TIMESTAMPOID:
                    return "timestamp_handler";
                case (int)OID.TIMESTAMPTZOID:
                    return "timestamptz_handler";
                case (int)OID.INTERVALOID:
                    return "interval_handler";
                case (int)OID.MACADDROID:
                    return "macaddr_handler";
                case (int)OID.MACADDR8OID:
                    return "macaddr8_handler";
                case (int)OID.INETOID:
                    return "inet_handler";
                case (int)OID.CIDROID:
                    return "cidr_handler";
                case (int)OID.TEXTOID:
                    return "text_handler";
                case (int)OID.MONEYOID:
                    return "money_handler";
                case (int)OID.VARBITOID:
                    return "varbit_handler";
                case (int)OID.BITOID:
                    return "varbit_handler";
                case (int)OID.BYTEAOID:
                    return "bytea_handler";
                case (int)OID.BPCHAROID:
                    return "bpchar_handler";
                case (int)OID.VARCHAROID:
                    return "varchar_handler";
                case (int)OID.XMLOID:
                    return "xml_handler";
                case (int)OID.INT4RANGEOID:
                    return "int_range_handler";
                case (int)OID.INT8RANGEOID:
                    return "long_range_handler";
                case (int)OID.TSRANGEOID:
                    return "time_range_handler";
                case (int)OID.TSTZRANGEOID:
                    return "timetz_range_handler";
                case (int)OID.DATERANGEOID:
                    return "date_range_handler";
                // case (int)OID.NUMRANGEOID: // currently unimplemented
                    // return "numeric_range_handler";
                default:
                    if (HANDLE_ARRAY.ContainsKey((OID)id))
                        return GetTypeHandler((int)HANDLE_ARRAY[(OID)id]);
                    throw new NotImplementedException($"Datum to {(OID)id} is not supported! Check GetTypeHandler");
            }
        }

        public static string BuildCreateArguments(string funcName, List<int> paramTypes)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"// BEGIN create arguments for {funcName}");
            int argc = paramTypes.Count;

            for (int i = 0; i < argc; i++)
            {
                var argname = $"argument_{i}";
                var value = $"arguments[{i}]";
                var type = paramTypes[i];
                var type_handler = GetTypeHandler(type);
                if (HANDLE_ARRAY.ContainsKey((OID)type))
                {
                    if (Engine.SupportNullInput || Engine.AlwaysNullable)
                        sb.AppendLine($"var {argname} = {type_handler}_obj.input_nullable_array({value}, isnull[{i}]);");
                    else
                        sb.AppendLine($"var {argname} = {type_handler}_obj.input_array({value});");
                }
                else
                {
                    if (Engine.SupportNullInput || Engine.AlwaysNullable)
                        sb.AppendLine($"var {argname} = {type_handler}_obj.input_nullable_value({value}, isnull[{i}]);");
                    else
                        sb.AppendLine($"var {argname} = {type_handler}_obj.input_value({value});");
                }
            }
            sb.Append($"// END create arguments for {funcName}");
            return sb.ToString();
        }

        // DONUT - TODO - remove thins function
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
                if (Engine.SupportNullInput || Engine.AlwaysNullable)
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

        public static string BuildCallSetResult(int id, string returnType)
        {
            string setResult;
            if (HANDLE_ARRAY.ContainsKey((OID)id))
                setResult = $"var result_datum = {GetTypeHandler(id)}_obj.output_nullable_array(result, OID.{HANDLE_ARRAY[(OID)id]});";
            else
                setResult = $"var result_datum = {GetTypeHandler(id)}_obj.output_nullable_value(result);";

            setResult += "pldotnet_SetDatumResult(result_datum, result == null, output);";
            return setResult;
        }

        public static unsafe string BuildSourceCode(IntPtr Name, int returnTypeID, IntPtr ParamNames, int* ParamTypes, IntPtr Body)
        {
            string funcName = Marshal.PtrToStringAuto(Name);
            string returnType = HANDLE_ARRAY.ContainsKey((OID)returnTypeID) ? "Array" : OID_TYPES[(OID)returnTypeID];
            string parameters = Marshal.PtrToStringAuto(ParamNames);
            List<string> paramNameList = new List<string>();
            List<int> paramTypeList = new List<int>();
            if (parameters != null)
            {
                paramNameList.AddRange(parameters.Split(" "));
                for (int i = 0; i < paramNameList.Count(); i++)
                {
                    paramTypeList.Add(ParamTypes[i]);
                }
            }
            var sqlParams = GetSqlParams(paramNameList, paramTypeList);
            string paramsStr = GetParamsString(sqlParams);
            string body = Marshal.PtrToStringAuto(Body);

            pldotnet_Info($"Compiling function {funcName}");
            pldotnet_Info($"Return type: {returnType}");
            pldotnet_Info($"Params: {paramsStr}");
            pldotnet_Info($"Body: {body}");

            // dummy template, we need to use a real template later
            // including the boilerplate code for the user function
            string rawFunctionDecl = $"public static {returnType}? {funcName}({paramsStr}) {{\n#line 1\n{body}\n}}";

            if (!File.Exists(CSharpTemplatePath))
            {
                string msg = $"Csharp template file '{CSharpTemplatePath}' not found";
                pldotnet_Info(msg);
                throw new SystemException(msg);
            }

            pldotnet_Info($"Loading template from {CSharpTemplatePath}");
            var template = File.ReadAllText(CSharpTemplatePath);
            var withArgumentsCreated = template.Replace("// $create_arguments", BuildCreateArguments(funcName, paramTypeList));
            var withFunctionDecl = withArgumentsCreated.Replace("// $user_function_declaration$", rawFunctionDecl);
            var withFunctionCall = withFunctionDecl.Replace("// $user_function_call$", BuildFunctionCall(funcName, sqlParams));
            var withArgumentsDeleted = withFunctionCall.Replace("// $free_arguments", BuildFreeArguments(funcName, sqlParams));
            var withResultsSet = withArgumentsDeleted.Replace("// $call_set_result$", BuildCallSetResult(returnTypeID, returnType));
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
                .WithConcurrentBuild(true).WithAllowUnsafe(true);

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
            Assembly myAssembly2 = AssemblyLoadContext.Default.LoadFromAssemblyPath(typeof(Engine).Assembly.Location);

            return compileResult;
        }

        public unsafe delegate int DelCompileUserFunction(uint FunctionId, IntPtr Name, int ReturnType, IntPtr ParamNames, int* ParamTypes, IntPtr Body, [MarshalAs(UnmanagedType.I1)] bool SupportNullInput);

        public static unsafe int CompileUserFunction(uint FunctionId, IntPtr Name, int ReturnType, IntPtr ParamNames, int* ParamTypes, IntPtr Body, [MarshalAs(UnmanagedType.I1)] bool SupportNullInput)
        {
            Engine.SupportNullInput = SupportNullInput;

            string sourceCode = BuildSourceCode(Name, ReturnType, ParamNames, ParamTypes, Body);

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
                UserProcedure = GetDirectDelegate(Engine.MemStream),
                SupportNullInput = Engine.SupportNullInput
            };

            Engine.FunctionId = FunctionId;
            Engine.FuncBuiltCodeDict[FunctionId] = Engine.Cached;
            Engine.UserProcedure = Engine.Cached.UserProcedure;

            pldotnet_Info("================================");
            pldotnet_Info($"\nNormalized source code: \n{Engine.Cached.SourceCode}");
            pldotnet_Info("================================");

            return 0;
        }

        public static Action<List<IntPtr>, IntPtr, bool[]> GetDirectDelegate(MemoryStream memoryStream)
        {
            var compiledAssembly = Assembly.Load(memoryStream.GetBuffer());

            Type procClassType = compiledAssembly.GetType("PlDotNETUserSpace.UserClass");

            if (null == procClassType)
            {
                pldotnet_Warning($"Failed to get type PlDotNETUserSpace.UserClass");
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
        /// This function should be called from C code
        /// It tries to get the cached function by its id.
        /// If it is not found, it returns a number different from zero
        /// Otherwise, it calls the user function compiled by Roslyn
        /// </summary>

        public static unsafe int RunUserFunction(uint functionId, IntPtr arguments, byte* nullmap, IntPtr output)
        {
            if (Engine.FuncBuiltCodeDict.TryGetValue(functionId, out CachedFunction cached))
            {
                GCHandle gch_list = GCHandle.FromIntPtr(arguments);
                var argument_list = (List<IntPtr>)gch_list.Target;
                bool[] isnull = new bool[argument_list.Count];
                if (cached.SupportNullInput || Engine.AlwaysNullable)
                    for (int i = 0, nargs = isnull.Length; i < nargs; i++)
                        isnull[i] = nullmap[i] == 0 ? false : true;

                for (int i = 0; i < isnull.Length; i++)
                    pldotnet_Info($"C# - DEBUG - is the argument[{i}] nulll? {isnull[i]}");

                cached.UserProcedure(argument_list, output, isnull);
            }
            else
            {
                pldotnet_Elog(21, $"[pldotnet]: could not find the generated function (ID: {functionId})");
            }

            return 0;
        }

        public unsafe delegate int DelRunUserFunction(uint functionId, IntPtr arguments, byte* nullmap, IntPtr output);

        /// <summary>
        /// Free memmory pointed by a IntPtr
        /// </summary>
        public static unsafe void FreeGenericGCHandle(IntPtr p)
        {
            GCHandle gch = GCHandle.FromIntPtr(p);
            gch.Free();
        }

        public delegate void DelFreeGenericGCHandle(IntPtr p);

        // Create a new list of IntPtr
        // Intended for pldotnet to pass an array of Datum's to Engine.cs
        public static unsafe System.IntPtr BuildDatumList()
        {
            var l = new List<IntPtr>();
            GCHandle handle = GCHandle.Alloc(l, GCHandleType.Normal);
            return GCHandle.ToIntPtr(handle);
        }
        public delegate System.IntPtr DelBuildDatumList();

        // Add an IntPtr(Datum) to a list of IntPtr
        // Intended for pldotnet to pass an array of Datum's to Engine.cs
        public static unsafe void AddDatumToList(System.IntPtr _list, System.IntPtr _datum)
        {
            GCHandle gch_list = GCHandle.FromIntPtr(_list);
            var list = (List<IntPtr>)gch_list.Target;
            list.Add(_datum);
        }
        public delegate void DelAddDatumToList(System.IntPtr _list, System.IntPtr _datum);
    }
}
