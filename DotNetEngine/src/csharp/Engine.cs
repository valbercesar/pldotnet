// <copyright file="Engine.cs" company="Brick Abode">
//
// PL/.NET (pldotnet) - PostgreSQL support for .NET C# and F# as
//                      procedural languages (PL)
//
//
// Copyright 2019-2020 Brick Abode
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// DotNetEngine/src/csharp/Engine.cs - pldotnet assembly compiler and runner
// </copyright>

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Net.Client;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NpgsqlTypes;
using PlDotNET.FSharp;
using PlDotNET.Handler;

namespace PlDotNET
{
    public enum DotNETLanguage : ushort
    {
        CSharp,
        FSharp,
        VisualBasic,
    }

    public struct CachedFunction
    {
        public string UserHandlerSourceCode;
        public string UserFunctionSourceCode;
        public bool SupportNullInput;
        public Action<List<IntPtr>, IntPtr, bool[]> UserProcedure;
        public AssemblyLoadContext UserAssemblyLoadContext;
        public DotNETLanguage Language;
    }

    public static class Engine
    {
        public static bool AlwaysNullable = false;

        public static Dictionary<OID, OID> HandleArray =
                       new ()
        {
            { OID.BOOLARRAYOID, OID.BOOLOID },
            { OID.INT2ARRAYOID, OID.INT2OID },
            { OID.INT4ARRAYOID, OID.INT4OID },
            { OID.INT8ARRAYOID, OID.INT8OID },
            { OID.FLOAT4ARRAYOID, OID.FLOAT4OID },
            { OID.FLOAT8ARRAYOID, OID.FLOAT8OID },
            { OID.POINTARRAYOID, OID.POINTOID },
            { OID.LINEARRAYOID, OID.LINEOID },
            { OID.LSEGARRAYOID, OID.LSEGOID },
            { OID.BOXARRAYOID, OID.BOXOID },
            { OID.POLYGONARRAYOID, OID.POLYGONOID },
            { OID.TEXTARRAYOID, OID.TEXTOID },
            { OID.PATHARRAYOID, OID.PATHOID },
            { OID.CIRCLEARRAYOID, OID.CIRCLEOID },
            { OID.DATEARRAYOID, OID.DATEOID },
            { OID.TIMEARRAYOID, OID.TIMEOID },
            { OID.TIMETZARRAYOID, OID.TIMETZOID },
            { OID.TIMESTAMPARRAYOID, OID.TIMESTAMPOID },
            { OID.TIMESTAMPTZARRAYOID, OID.TIMESTAMPTZOID },
            { OID.INTERVALARRAYOID, OID.INTERVALOID },
            { OID.MACADDRARRAYOID, OID.MACADDROID },
            { OID.MACADDR8ARRAYOID, OID.MACADDR8OID },
            { OID.INETARRAYOID, OID.INETOID },
            { OID.CIDRARRAYOID, OID.CIDROID },
            { OID.MONEYARRAYOID, OID.MONEYOID },
            { OID.VARBITARRAYOID, OID.VARBITOID },
            { OID.BITARRAYOID, OID.BITOID },
            { OID.BYTEAARRAYOID, OID.BYTEAOID },
            { OID.BPCHARARRAYOID, OID.BPCHAROID },
            { OID.VARCHARARRAYOID, OID.VARCHAROID },
            { OID.XMLARRAYOID, OID.XMLOID },
            { OID.JSONARRAYOID, OID.JSONOID },
            { OID.UUIDARRAYOID, OID.UUIDOID },
            { OID.INT4RANGEARRAYOID, OID.INT4RANGEOID },
            { OID.NUMRANGEARRAYOID, OID.NUMRANGEOID },
            { OID.TSRANGEARRAYOID, OID.TSRANGEOID },
            { OID.TSTZRANGEARRAYOID, OID.TSTZRANGEOID },
            { OID.DATERANGEARRAYOID, OID.DATERANGEOID },
            { OID.INT8RANGEARRAYOID, OID.INT8RANGEOID },
            { OID.INT4MULTIRANGEARRAYOID, OID.INT4MULTIRANGEOID },
            { OID.NUMMULTIRANGEARRAYOID, OID.NUMMULTIRANGEOID },
            { OID.TSMULTIRANGEARRAYOID, OID.TSMULTIRANGEOID },
            { OID.TSTZMULTIRANGEARRAYOID, OID.TSTZMULTIRANGEOID },
            { OID.DATEMULTIRANGEARRAYOID, OID.DATEMULTIRANGEOID },
            { OID.INT8MULTIRANGEARRAYOID, OID.INT8MULTIRANGEOID },
        };

        public static Dictionary<OID, string> OidTypes =
                       new ()
        {
            { OID.BOOLOID, "bool" },
            { OID.INT2OID, "short" },
            { OID.INT4OID, "int" },
            { OID.INT8OID, "long" },
            { OID.FLOAT4OID, "float" },
            { OID.FLOAT8OID, "double" },
            { OID.POINTOID, "NpgsqlPoint" },
            { OID.LINEOID, "NpgsqlLine" },
            { OID.LSEGOID, "NpgsqlLSeg" },
            { OID.BOXOID, "NpgsqlBox" },
            { OID.POLYGONOID, "NpgsqlPolygon" },
            { OID.TEXTOID, "string" },
            { OID.PATHOID, "NpgsqlPath" },
            { OID.CIRCLEOID, "NpgsqlCircle" },
            { OID.DATEOID, "DateOnly" },
            { OID.TIMEOID, "TimeOnly" },
            { OID.TIMETZOID, "DateTimeOffset" },
            { OID.TIMESTAMPOID, "DateTime" },
            { OID.TIMESTAMPTZOID, "DateTime" },
            { OID.INTERVALOID, "NpgsqlInterval" },
            { OID.MACADDROID, "PhysicalAddress" },
            { OID.MACADDR8OID, "PhysicalAddress" },
            { OID.INETOID, "(IPAddress Address, int Netmask)" },
            { OID.CIDROID, "(IPAddress Address, int Netmask)" },
            { OID.MONEYOID, "decimal" },
            { OID.VARBITOID, "BitArray" },
            { OID.BITOID, "BitArray" },
            { OID.BYTEAOID, "byte[]" },
            { OID.BPCHAROID, "string" },
            { OID.VARCHAROID, "string" },
            { OID.XMLOID, "string" },
            { OID.JSONOID, "string" },
            { OID.UUIDOID, "Guid" },
            { OID.INT4RANGEOID, "NpgsqlRange<int>" },
            { OID.INT8RANGEOID, "NpgsqlRange<long>" },
            { OID.TSRANGEOID, "NpgsqlRange<DateTime>" },
            { OID.TSTZRANGEOID, "NpgsqlRange<DateTime>" },
            { OID.DATERANGEOID, "NpgsqlRange<DateOnly>" },
            { OID.VOIDOID, "void" },
        };

        public static IDictionary<uint, CachedFunction> FuncBuiltCodeDict = new Dictionary<uint, CachedFunction>();

        public static FSharpCodeGenerator FSharpGenerator = new ();

        public static CSharpCodeGenerator CSharpGenerator = new ();

        public unsafe delegate int DelCompileUserFunction(uint functionId, IntPtr name, uint returnType, IntPtr paramNames, uint* paramTypes, IntPtr body, [MarshalAs(UnmanagedType.I1)] bool supportNullInput, IntPtr dotnetLanguage);

        public unsafe delegate int DelRunUserFunction(uint functionId, IntPtr arguments, byte* nullmap, IntPtr output);

        public delegate void DelFreeGenericGCHandle(IntPtr p);

        public delegate System.IntPtr DelBuildDatumList();

        public delegate void DelAddDatumToList(System.IntPtr list, System.IntPtr datum);

        public delegate void DelUnloadAssemblies(uint functionId);

        /// <summary>
        /// C function declared in pldotnet_conversions.h to return a void datum.
        /// See ::pldotnet_createDatumVoid().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumVoid();

        /// <summary>
        /// C function declared in pldotnet_common.h to set the datum result.
        /// See ::pldotnet_SetDatumResult().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_SetDatumResult(IntPtr value, [MarshalAs(UnmanagedType.I1)] bool isnull, IntPtr nativeResult);

        /// <summary>
        /// Returns the handler object NAME for the specified OID.
        /// </summary>
        public static string GetTypeHandler(uint id)
        {
            switch (id)
            {
                case (uint)OID.BOOLOID:
                    return "BoolHandler";
                case (uint)OID.INT2OID:
                    return "ShortHandler";
                case (uint)OID.INT4OID:
                    return "IntHandler";
                case (uint)OID.INT8OID:
                    return "LongHandler";
                case (uint)OID.FLOAT4OID:
                    return "FloatHandler";
                case (uint)OID.FLOAT8OID:
                    return "DoubleHandler";
                case (uint)OID.POINTOID:
                    return "PointHandler";
                case (uint)OID.LINEOID:
                    return "LineHandler";
                case (uint)OID.LSEGOID:
                    return "LineSegmentHandler";
                case (uint)OID.BOXOID:
                    return "BoxHandler";
                case (uint)OID.PATHOID:
                    return "PathHandler";
                case (uint)OID.POLYGONOID:
                    return "PolygonHandler";
                case (uint)OID.CIRCLEOID:
                    return "CircleHandler";
                case (uint)OID.DATEOID:
                    return "DateHandler";
                case (uint)OID.TIMEOID:
                    return "TimeHandler";
                case (uint)OID.TIMETZOID:
                    return "TimeTzHandler";
                case (uint)OID.TIMESTAMPOID:
                    return "TimestampHandler";
                case (uint)OID.TIMESTAMPTZOID:
                    return "TimestampTzHandler";
                case (uint)OID.INTERVALOID:
                    return "IntervalHandler";
                case (uint)OID.MACADDROID:
                    return "MacaddrHandler";
                case (uint)OID.MACADDR8OID:
                    return "Macaddr8Handler";
                case (uint)OID.INETOID:
                    return "InetHandler";
                case (uint)OID.CIDROID:
                    return "CidrHandler";
                case (uint)OID.TEXTOID:
                    return "TextHandler";
                case (uint)OID.MONEYOID:
                    return "MoneyHandler";
                case (uint)OID.VARBITOID:
                    return "VarBitStringHandler";
                case (uint)OID.BITOID:
                    return "BitStringHandler";
                case (uint)OID.BYTEAOID:
                    return "ByteaHandler";
                case (uint)OID.BPCHAROID:
                    return "CharHandler";
                case (uint)OID.VARCHAROID:
                    return "CharVaryingHandler";
                case (uint)OID.XMLOID:
                    return "XmlHandler";
                case (uint)OID.JSONOID:
                    return "JsonHandler";
                case (uint)OID.UUIDOID:
                    return "UuidHandler";
                case (uint)OID.INT4RANGEOID:
                    return "IntRangeHandler";
                case (uint)OID.INT8RANGEOID:
                    return "LongRangeHandler";
                case (uint)OID.TSRANGEOID:
                    return "TimestampRangeHandler";
                case (uint)OID.TSTZRANGEOID:
                    return "TimestampTzRangeHandler";
                case (uint)OID.DATERANGEOID:
                    return "DateRangeHandler";
                default:
                    if (HandleArray.ContainsKey((OID)id))
                    {
                        return GetTypeHandler((uint)HandleArray[(OID)id]);
                    }

                    throw new NotImplementedException($"Datum to {(OID)id} is not supported! Check GetTypeHandler");
            }
        }

        /// <summary>
        /// This function returns the compilation errors reported during the
        /// compilation of the dynamic code using Roslyn.
        /// </summary>
        public static string GetCompilationError(Diagnostic diagnostic, string[] lines)
        {
            string pattern = @"\d+,\d+";
            string message = diagnostic.ToString();
            _ = Regex.Match(message, pattern, RegexOptions.IgnoreCase);

            // TODO(rosicley) - I commented the code below because it was failing.
            // if (m.Success)
            // {
            //     var sb = new System.Text.StringBuilder();
            //     var split = m.Value.Split(',');
            //     if (split.Length > 0)
            //     {
            //         var l0 = Int32.Parse(split[0]);
            //         var line = lines[l0 - 1].TrimEnd();
            //         sb.AppendLine($" > {line}");
            //         sb.AppendLine($" ^ {message}");
            //         return sb.ToString();
            //     }
            // }
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

            Elog.pldotnet_Info("===========================");
            Elog.pldotnet_Info("Compiling source code");
            Elog.pldotnet_Info($"Source code:\n{sourceCode}");
            Elog.pldotnet_Info("===========================");

            var trustedAssembliesPathsArray = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator);
            List<string> trustedAssembliesPaths = new ();
            trustedAssembliesPaths.AddRange(trustedAssembliesPathsArray);
            trustedAssembliesPaths.Add(typeof(NpgsqlPoint).Assembly.Location);
            trustedAssembliesPaths.Add(typeof(Engine).Assembly.Location);

            var neededAssemblies = new[]
            {
                "System.Buffers",
                "System.Collections",
                "System.Collections.Generic",
                "System.Console",
                "System.Core",
                "System.Data",
                "System.Data.Common",
                "System.Data.SqlClient",
                "System.Diagnostics",
                "System.Diagnostics.CodeAnalysis",
                "System.Globalization",
                "System.Linq",
                "System.Linq.Expressions",
                "System.Net.NetworkInformation",
                "System.Net.Primitives",
                "System.Private.CoreLib",
                "System.Runtime",
                "System.Text",
                "System.Text.Unicode",
                "Npgsql",
                "PlDotNET",
            };

            List<PortableExecutableReference> references = trustedAssembliesPaths
                .Where(p => neededAssemblies.Contains(Path.GetFileNameWithoutExtension(p)))
                .Select(p => MetadataReference.CreateFromFile(p))
                .ToList();

            if (memStreamUserFunction != null)
            {
                references.Add(MetadataReference.CreateFromStream(new MemoryStream(memStreamUserFunction.GetBuffer())));
            }

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
                {
                    sb.AppendLine(GetCompilationError(diagnostic, lines));
                }

                sb.AppendLine("\n********ERROR************\n");
                Elog.pldotnet_Warning(sb.ToString());
            }

            return compileResult;
        }

        // <summary>
        // This function is called called from C code and tries to create and
        // compile the dynamic code using Roslyn. It also saves the
        // CachedFunction in FuncBuiltCodeDict so that any compiled code can
        // be called by the user function ID.
        // This function returns 0 if all the codes were compiled correctly.
        // </summary>
        public static unsafe int CompileUserFunction(uint functionId, IntPtr name, uint returnTypeId, IntPtr paramNames, uint* paramTypes, IntPtr body, [MarshalAs(UnmanagedType.I1)] bool supportNullInput, IntPtr language)
        {
            // User function Data
            string funcName = Marshal.PtrToStringAuto(name);
            string returnType = HandleArray.ContainsKey((OID)returnTypeId) ? "Array" : OidTypes[(OID)returnTypeId];
            string auxParameters = Marshal.PtrToStringAuto(paramNames);
            string[] paramNameArray = auxParameters == null ? Array.Empty<string>() : auxParameters.Split(" ");
            uint[] paramTypeArray = auxParameters == null ? Array.Empty<uint>() : new ReadOnlySpan<uint>(paramTypes, paramNameArray.Length).ToArray();
            string funcBody = Marshal.PtrToStringAuto(body);

            CodeGenerator dynamicCodeGenerator;
            DotNETLanguage dotnetLanguage;
            switch (Marshal.PtrToStringAuto(language))
            {
                case "csharp":
                    dynamicCodeGenerator = CSharpGenerator;
                    dotnetLanguage = DotNETLanguage.CSharp;
                    break;
                case "fsharp":
                    dynamicCodeGenerator = FSharpGenerator;
                    dotnetLanguage = DotNETLanguage.FSharp;
                    break;
                default:
                    Elog.pldotnet_Warning($"PL.NET doesn't have support for {Marshal.PtrToStringAuto(language)}. Please contact Brick Abode! <tlewis@brickabode.com>");
                    return 1;
            }

            // Generate the UserFunction code
            // If the user function uses F#, this variable receives an empty string
            string userFunctionCode = dynamicCodeGenerator.BuildUserFunctionSourceCode(funcName, returnTypeId, paramNameArray, paramTypeArray, funcBody, supportNullInput);

            // Generate the UserHandler code
            string userHandlerCode = dynamicCodeGenerator.BuildUserHandlerSourceCode(funcName, returnTypeId, paramNameArray, paramTypeArray, funcBody, supportNullInput);

            // Check if the user provided an assembly
            bool useUserAssembly = userFunctionCode.Contains(".dll");

            // Check if the user function exists in .NET context
            if (Engine.FuncBuiltCodeDict.TryGetValue(functionId, out CachedFunction cached))
            {
                // check PL.NET needs to recompile the source codes
                if (cached.UserHandlerSourceCode == userHandlerCode && cached.UserFunctionSourceCode == userFunctionCode && !useUserAssembly)
                {
                    Elog.pldotnet_Info("User function hasn't changed, so it doesn't need to be recompiled!");
                    return 0;
                }
                else
                {
                    FuncBuiltCodeDict[functionId].UserAssemblyLoadContext.Unload();
                    FuncBuiltCodeDict.Remove(functionId);
                }
            }

            MemoryStream memUserFunction = new ();
            if (!useUserAssembly)
            {
                if (dotnetLanguage == DotNETLanguage.CSharp)
                {
                    var compileResultUserFunction = CompileSourceCode(userFunctionCode, memUserFunction, $"UserFunction_{functionId}");

                    // Verify that the C# code for UserFunction compiled correctly
                    if (!compileResultUserFunction.Success)
                    {
                        return 1;
                    }
                }
            }
            else
            {
                string userAssemblyPath = userFunctionCode.Split(":")[0];
                using var fs = File.Open(userAssemblyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                fs.CopyTo(memUserFunction);
            }

            MemoryStream memUserHandler = new ();
            if (dotnetLanguage == DotNETLanguage.FSharp)
            {
                string generatedAssembly = FSharpCompiler.CompileFSharpSourceCode(functionId, userHandlerCode);

                // Verify that the F# code compiled correctly
                if (generatedAssembly == string.Empty)
                {
                    return 1;
                }

                using var fs = File.Open(generatedAssembly, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                fs.CopyTo(memUserHandler);
            }
            else
            {
                var compileResultUserHandler = Engine.CompileSourceCode(userHandlerCode, memUserHandler, $"UserHandler_{functionId}", memUserFunction);

                // Verify that the C# code for UserHandler compiled correctly
                if (!compileResultUserHandler.Success)
                {
                    return 1;
                }
            }

            // Load the assemblies into AssemblyLoadContext
            AssemblyLoadContext userAlc = new ($"UserFunction_{functionId}", true);
            Assembly npgsqlAssembly = userAlc.LoadFromAssemblyPath(typeof(NpgsqlPoint).Assembly.Location);
            Assembly pldotnetAssembly = userAlc.LoadFromAssemblyPath(typeof(Engine).Assembly.Location);
            Assembly userFunctionAssembly = dotnetLanguage != DotNETLanguage.FSharp ? userAlc.LoadFromStream(new MemoryStream(memUserFunction.GetBuffer())) : null;
            Assembly userHandlerAssembly = userAlc.LoadFromStream(new MemoryStream(memUserHandler.GetBuffer()));

            // Create the CachedFunction to keep the function information
            CachedFunction newCachedFunction = new ()
            {
                UserFunctionSourceCode = userFunctionCode,
                UserHandlerSourceCode = userHandlerCode,
                SupportNullInput = supportNullInput,
                UserAssemblyLoadContext = userAlc,
                UserProcedure = GetDirectDelegate(userHandlerAssembly),
                Language = dotnetLanguage,
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
        public static Action<List<IntPtr>, IntPtr, bool[]> GetDirectDelegate(Assembly compiledAssembly)
        {
            Type procClassType = compiledAssembly.GetType("PlDotNET.UserSpace.UserHandler");

            if (procClassType == null)
            {
                Elog.pldotnet_Warning($"Failed to get type PlDotNET.UserSpace.UserHandler");
                return null;
            }

            MethodInfo procMethod = procClassType.GetMethod("CallUserFunction");

            return (Action<List<IntPtr>, IntPtr, bool[]>)Delegate.CreateDelegate(
                typeof(Action<List<IntPtr>, IntPtr, bool[]>),
                null,
                procMethod);
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
                {
                    for (int i = 0, nargs = isnull.Length; i < nargs; i++)
                    {
                        isnull[i] = nullmap[i] != 0;
                    }
                }

                cached.UserProcedure(argumentList, output, isnull);
            }
            else
            {
                Elog.pldotnet_Elog(21, $"[pldotnet]: could not find the generated function (ID: {functionId})");
            }

            return 0;
        }

        /// <summary>
        /// Free memmory pointed by a IntPtr.
        /// </summary>
        public static unsafe void FreeGenericGCHandle(IntPtr p)
        {
            GCHandle gch = GCHandle.FromIntPtr(p);
            gch.Free();
        }

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
                Elog.pldotnet_Elog(21, $"[pldotnet]: could not find the generated function (ID: {functionId})");
            }
        }
    }
}
