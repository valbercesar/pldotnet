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

        enum OID : int
        {
            BOOLOID = 16,
            BYTEAOID = 17,
            CHAROID = 18,
            NAMEOID = 19,
            INT8OID = 20,
            INT2OID = 21,
            INT2VECTOROID = 22,
            INT4OID = 23,
            REGPROCOID = 24,
            TEXTOID = 25,
            OIDOID = 26,
            TIDOID = 27,
            XIDOID = 28,
            CIDOID = 29,
            OIDVECTOROID = 30,
            JSONOID = 114,
            XMLOID = 142,
            PG_NODE_TREEOID = 194,
            PG_NDISTINCTOID = 3361,
            PG_DEPENDENCIESOID = 3402,
            PG_MCV_LISTOID = 5017,
            PG_DDL_COMMANDOID = 32,
            XID8OID = 5069,
            POINTOID = 600,
            LSEGOID = 601,
            PATHOID = 602,
            BOXOID = 603,
            POLYGONOID = 604,
            LINEOID = 628,
            FLOAT4OID = 700,
            FLOAT8OID = 701,
            UNKNOWNOID = 705,
            CIRCLEOID = 718,
            MONEYOID = 790,
            MACADDROID = 829,
            INETOID = 869,
            CIDROID = 650,
            MACADDR8OID = 774,
            ACLITEMOID = 1033,
            BPCHAROID = 1042,
            VARCHAROID = 1043,
            DATEOID = 1082,
            TIMEOID = 1083,
            TIMESTAMPOID = 1114,
            TIMESTAMPTZOID = 1184,
            INTERVALOID = 1186,
            TIMETZOID = 1266,
            BITOID = 1560,
            VARBITOID = 1562,
            NUMERICOID = 1700,
            REFCURSOROID = 1790,
            REGPROCEDUREOID = 2202,
            REGOPEROID = 2203,
            REGOPERATOROID = 2204,
            REGCLASSOID = 2205,
            REGCOLLATIONOID = 4191,
            REGTYPEOID = 2206,
            REGROLEOID = 4096,
            REGNAMESPACEOID = 4089,
            UUIDOID = 2950,
            PG_LSNOID = 3220,
            TSVECTOROID = 3614,
            GTSVECTOROID = 3642,
            TSQUERYOID = 3615,
            REGCONFIGOID = 3734,
            REGDICTIONARYOID = 3769,
            JSONBOID = 3802,
            JSONPATHOID = 4072,
            TXID_SNAPSHOTOID = 2970,
            PG_SNAPSHOTOID = 5038,
            INT4RANGEOID = 3904,
            NUMRANGEOID = 3906,
            TSRANGEOID = 3908,
            TSTZRANGEOID = 3910,
            DATERANGEOID = 3912,
            INT8RANGEOID = 3926,
            INT4MULTIRANGEOID = 4451,
            NUMMULTIRANGEOID = 4532,
            TSMULTIRANGEOID = 4533,
            TSTZMULTIRANGEOID = 4534,
            DATEMULTIRANGEOID = 4535,
            INT8MULTIRANGEOID = 4536,
            RECORDOID = 2249,
            RECORDARRAYOID = 2287,
            CSTRINGOID = 2275,
            ANYOID = 2276,
            ANYARRAYOID = 2277,
            VOIDOID = 2278,
            TRIGGEROID = 2279,
            EVENT_TRIGGEROID = 3838,
            LANGUAGE_HANDLEROID = 2280,
            INTERNALOID = 2281,
            ANYELEMENTOID = 2283,
            ANYNONARRAYOID = 2776,
            ANYENUMOID = 3500,
            FDW_HANDLEROID = 3115,
            INDEX_AM_HANDLEROID = 325,
            TSM_HANDLEROID = 3310,
            TABLE_AM_HANDLEROID = 269,
            ANYRANGEOID = 3831,
            ANYCOMPATIBLEOID = 5077,
            ANYCOMPATIBLEARRAYOID = 5078,
            ANYCOMPATIBLENONARRAYOID = 5079,
            ANYCOMPATIBLERANGEOID = 5080,
            ANYMULTIRANGEOID = 4537,
            ANYCOMPATIBLEMULTIRANGEOID = 4538,
            PG_BRIN_BLOOM_SUMMARYOID = 4600,
            PG_BRIN_MINMAX_MULTI_SUMMARYOID = 4601,
            BOOLARRAYOID = 1000,
            BYTEAARRAYOID = 1001,
            CHARARRAYOID = 1002,
            NAMEARRAYOID = 1003,
            INT8ARRAYOID = 1016,
            INT2ARRAYOID = 1005,
            INT2VECTORARRAYOID = 1006,
            INT4ARRAYOID = 1007,
            REGPROCARRAYOID = 1008,
            TEXTARRAYOID = 1009,
            OIDARRAYOID = 1028,
            TIDARRAYOID = 1010,
            XIDARRAYOID = 1011,
            CIDARRAYOID = 1012,
            OIDVECTORARRAYOID = 1013,
            PG_TYPEARRAYOID = 210,
            PG_ATTRIBUTEARRAYOID = 270,
            PG_PROCARRAYOID = 272,
            PG_CLASSARRAYOID = 273,
            JSONARRAYOID = 199,
            XMLARRAYOID = 143,
            XID8ARRAYOID = 271,
            POINTARRAYOID = 1017,
            LSEGARRAYOID = 1018,
            PATHARRAYOID = 1019,
            BOXARRAYOID = 1020,
            POLYGONARRAYOID = 1027,
            LINEARRAYOID = 629,
            FLOAT4ARRAYOID = 1021,
            FLOAT8ARRAYOID = 1022,
            CIRCLEARRAYOID = 719,
            MONEYARRAYOID = 791,
            MACADDRARRAYOID = 1040,
            INETARRAYOID = 1041,
            CIDRARRAYOID = 651,
            MACADDR8ARRAYOID = 775,
            ACLITEMARRAYOID = 1034,
            BPCHARARRAYOID = 1014,
            VARCHARARRAYOID = 1015,
            DATEARRAYOID = 1182,
            TIMEARRAYOID = 1183,
            TIMESTAMPARRAYOID = 1115,
            TIMESTAMPTZARRAYOID = 1185,
            INTERVALARRAYOID = 1187,
            TIMETZARRAYOID = 1270,
            BITARRAYOID = 1561,
            VARBITARRAYOID = 1563,
            NUMERICARRAYOID = 1231,
            REFCURSORARRAYOID = 2201,
            REGPROCEDUREARRAYOID = 2207,
            REGOPERARRAYOID = 2208,
            REGOPERATORARRAYOID = 2209,
            REGCLASSARRAYOID = 2210,
            REGCOLLATIONARRAYOID = 4192,
            REGTYPEARRAYOID = 2211,
            REGROLEARRAYOID = 4097,
            REGNAMESPACEARRAYOID = 4090,
            UUIDARRAYOID = 2951,
            PG_LSNARRAYOID = 3221,
            TSVECTORARRAYOID = 3643,
            GTSVECTORARRAYOID = 3644,
            TSQUERYARRAYOID = 3645,
            REGCONFIGARRAYOID = 3735,
            REGDICTIONARYARRAYOID = 3770,
            JSONBARRAYOID = 3807,
            JSONPATHARRAYOID = 4073,
            TXID_SNAPSHOTARRAYOID = 2949,
            PG_SNAPSHOTARRAYOID = 5039,
            INT4RANGEARRAYOID = 3905,
            NUMRANGEARRAYOID = 3907,
            TSRANGEARRAYOID = 3909,
            TSTZRANGEARRAYOID = 3911,
            DATERANGEARRAYOID = 3913,
            INT8RANGEARRAYOID = 3927,
            INT4MULTIRANGEARRAYOID = 6150,
            NUMMULTIRANGEARRAYOID = 6151,
            TSMULTIRANGEARRAYOID = 6152,
            TSTZMULTIRANGEARRAYOID = 6153,
            DATEMULTIRANGEARRAYOID = 6155,
            INT8MULTIRANGEARRAYOID = 6157,
            CSTRINGARRAYOID = 1263
        }

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
            {OID.CIRCLEOID, "NpgsqlCircle"}
        };

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
                arguments.Add(new Tuple<string, string>(OID_TYPES[(OID)paramTypes[i]], paramNames[i]));
            }
            return arguments;
        }

        public static string GetParamsString(List<Tuple<string, string>> sqlParams)
        {
            return string.Join(", ", sqlParams.Select((p) => $"{p.Item1} {p.Item2}").ToList());
        }

        public static string GetDatumConversionFunction(int id)
        {
            switch (id)
            {
                case (int)OID.INT2OID:
                    return "pldotnet_getInt16";
                case (int)OID.INT4OID:
                    return "pldotnet_getInt32";
                case (int)OID.INT8OID:
                    return "pldotnet_getInt64";
                case (int)OID.FLOAT4OID:
                    return "pldotnet_getFloat";
                case (int)OID.FLOAT8OID:
                    return "pldotnet_getDouble";
                case (int)OID.BOOLOID:
                    return "pldotnet_getBoolean";
                case (int)OID.POINTOID:
                    return "pldotnet_BuildNpgsqlPoint";
                case (int)OID.LINEOID:
                    return "pldotnet_BuildNpgsqlLine";
                case (int)OID.LSEGOID:
                    return "pldotnet_BuildNpgsqlLSeg";
                case (int)OID.BOXOID:
                    return "pldotnet_BuildNpgsqlBox";
                case (int)OID.TEXTOID:
                    return "pldotnet_BuildString";
                case (int)OID.PATHOID:
                    return "pldotnet_BuildNpgsqlPath";
                case (int)OID.POLYGONOID:
                    return "pldotnet_BuildNpgsqlPolygon";
                case (int)OID.CIRCLEOID:
                    return "pldotnet_BuildNpgsqlCircle";
                default:
                    throw new NotImplementedException($"Datum to {(OID)id} is not supported! Check GetDatumConversionFunction");
            }
        }

        // DONUT
        public static string BuildCreateArguments(string funcName, List<int> paramTypes)
        {
            // WARNING: this is completely wrong.
            // - the individual IntPtr are not GCHandles, only Datum
            // - creating a GCHandle from it is wrong
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"// BEGIN create arguments for {funcName}");
            int argc = paramTypes.Count;

            for (int i = 0; i < argc; i++)
            {
                var argname = $"argument_{i}";
                var value = $"arguments[{i}]";
                var dotnet_type = paramTypes[i];
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

        public static string BuildCallSetResult(int id, string returnType)
        {
            string setResult = "var result_datum = ";
            switch (id)
            {
                case (int)OID.INT2OID:
                    setResult +=
                    $"pldotnet_createDatumInt16(({returnType})result);\n";
                    break;
                case (int)OID.INT4OID:
                    setResult +=
                    $"pldotnet_createDatumInt32(({returnType})result);\n";
                    break;
                case (int)OID.INT8OID:
                    setResult +=
                    $"pldotnet_createDatumInt64(({returnType})result);\n";
                    break;
                case (int)OID.FLOAT4OID:
                    setResult +=
                    $"pldotnet_createDatumFloat(({returnType})result);\n";
                    break;
                case (int)OID.FLOAT8OID:
                    setResult +=
                    $"pldotnet_createDatumDouble(({returnType})result);\n";
                    break;
                case (int)OID.BOOLOID:
                    setResult +=
                    $"pldotnet_createDatumBoolean(({returnType})result);\n";
                    break;
                case (int)OID.POINTOID:
                    setResult +=
                    $"pldotnet_createDatumPoint((double)result.X, "
                    + "(double)result.Y);\n";
                    break;
                case (int)OID.LINEOID:
                    setResult +=
                    $"pldotnet_createDatumLine((double)result.A, "
                    + "(double)result.B,(double)result.C);\n";
                    break;
                case (int)OID.LSEGOID:
                    setResult +=
                    $"pldotnet_createDatumLineSegment((double)result.Start.X,"
                    + "(double)result.Start.Y, (double)result.End.X, "
                    + "(double)result.End.Y);\n";
                    break;
                case (int)OID.BOXOID:
                    setResult +=
                    $"pldotnet_createDatumBox((double)result.UpperRight.X, "
                    + "(double)result.UpperRight.Y, (double)result.LowerLeft.X, "
                    + "(double)result.LowerLeft.Y);\n";
                    break;
                case (int)OID.TEXTOID:
                    setResult += "pldotnet_createDatumTextInternal(result);";
                    break;
                case (int)OID.PATHOID:
                    setResult += "pldotnet_createDatumPath(result);\n";
                    break;
                case (int)OID.POLYGONOID:
                    setResult += "pldotnet_createDatumPolygon(result);\n";
                    break;
                case (int)OID.CIRCLEOID:
                    setResult += "pldotnet_createDatumCircle(result.Center.X, "
                    + "result.Center.Y, result.Radius);\n";
                    break;
                default:
                    throw new NotImplementedException($"It is not possible to return a {returnType} type! Check BuildCallSetResult.");
            }
            setResult += "pldotnet_SetDatumResult(result_datum, result_datum == null, output);";
            return setResult;
        }

        public static unsafe string BuildSourceCode(IntPtr Name, int returnTypeID, IntPtr ParamNames, int* ParamTypes, IntPtr Body)
        {
            string funcName = Marshal.PtrToStringAuto(Name);
            string returnType = OID_TYPES[(OID)returnTypeID];
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
            string rawFunctionDecl = $"public static {returnType} {funcName}({paramsStr}) {{\n{body}\n}}";

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

            return compileResult;
        }

        public unsafe delegate int DelCompileUserFunction(uint FunctionId, IntPtr Name, int ReturnType, IntPtr ParamNames, int* ParamTypes, IntPtr Body);

        public static unsafe int CompileUserFunction(uint FunctionId, IntPtr Name, int ReturnType, IntPtr ParamNames, int* ParamTypes, IntPtr Body)
        {
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
