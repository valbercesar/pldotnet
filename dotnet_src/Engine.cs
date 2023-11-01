// <copyright file="Engine.cs" company="Brick Abode">
//
// PL/.NET (pldotnet) - PostgreSQL support for .NET C# and F# as
//                      procedural languages (PL)
//
//
// Copyright (c) 2023 Brick Abode
//
// This code is subject to the terms of the PostgreSQL License.
// The full text of the license can be found in the LICENSE file
// at the top level of the pldotnet repository.
//
// Engine.cs - pldotnet assembly compiler and runner
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
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

    public enum CallMode : int
    {
        [Description("CALL_NORMAL")] // Normal, non-SRF function
        Normal = 1,
        [Description("CALL_SRF_FIRST")] // First call to an SRF; create and cache
        SrfFirst = 2,
        [Description("CALL_SRF_NEXT")] // Next call to an SRF
        SrfNext = 3,
        [Description("CALL_SRF_CLEANUP")] // SRF is done; you may remove it from the cache
        SrfCleanup = 4,
    }

    public enum ReturnMode : int
    {
        [Description("RETURN_ERROR")] // We encountered an error
        Error = 0,
        [Description("RETURN_NORMAL")] // Normal return to CALL_NORMAL
        Normal = 1,
        [Description("RETURN_SRF_NEXT")] // SRF return to CALL_SRF_NEXT
        SrfNext = 2,
        [Description("RETURN_SRF_DONE")] // We have no more values
        SrfDone = 3,
    }

    public struct CachedFunction
    {
        public string UserHandlerSourceCode;
        public string UserFunctionSourceCode;
        public string FunctionName;
        public bool SupportNullInput;
        public Func<List<IntPtr>, IntPtr, ulong, int, bool[], int> UserProcedure;
        public AssemblyLoadContext UserAssemblyLoadContext;
        public DotNETLanguage Language;
    }

    public static class Engine
    {
        public static bool AlwaysNullable = false;

        public static bool PrintSourceCode = false;

        public static bool SaveSourceCode = true;

        public static string PathToSaveSourceCode = "/tmp/PlDotNET/GeneratedCodes";

        public static string PathToTemporaryFiles = "/tmp/PlDotNET/";

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
            { OID.RECORDOID, "void" },
        };

        public static IDictionary<uint, CachedFunction> FuncBuiltCodeDict = new Dictionary<uint, CachedFunction>();

        public static FSharpCodeGenerator FSharpGenerator = new ();

        public static CSharpCodeGenerator CSharpGenerator = new ();

        public unsafe delegate int DelCompileUserFunction(
                uint functionId,
                IntPtr name,
                uint returnType,
                [MarshalAs(UnmanagedType.I1)] bool retset,
                IntPtr paramNames,
                uint* paramTypes,
                byte* paramModes,
                int num_output_values,
                IntPtr body,
                [MarshalAs(UnmanagedType.I1)] bool supportNullInput,
                IntPtr dotnetLanguage);

        public unsafe delegate int DelRunUserFunction(uint functionId, ulong call_id, int call_mode, void* arguments, int num_arguments, byte* nullmap, IntPtr output);

        public delegate void DelFreeGenericGCHandle(IntPtr p);

        public delegate System.IntPtr DelBuildDatumList();

        public delegate void DelAddDatumToList(System.IntPtr list, System.IntPtr datum);

        public delegate void DelUnloadAssemblies(uint functionId);

        /// <summary>
        /// Returns the handler object NAME for the specified OID.
        /// </summary>
        /// <returns>
        /// Returns The TypeHandler name.
        /// </returns>
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
                case (uint)OID.RECORDOID: // This is a hack, but I think it's ok for now
                    return "IntHandler";
                default:
                    if (HandleArray.ContainsKey((OID)id))
                    {
                        return GetTypeHandler((uint)HandleArray[(OID)id]);
                    }

                    throw new NotImplementedException($"Datum to {(OID)id} is not supported! Check GetTypeHandler");
            }
        }

        /// <summary>
        /// This function compiles the dynamic code using Roslyn.
        /// </summary>
        /// <returns>
        /// Returns The response of the dynamic code compiled with Roslyn.
        /// </returns>
        public static Microsoft.CodeAnalysis.Emit.EmitResult CompileSourceCode(string sourceCode, MemoryStream memStream, string assemblyName, MemoryStream memStreamUserFunction = null)
        {
            SyntaxTree userTree = SyntaxFactory.ParseSyntaxTree(sourceCode);

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
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"PL.NET could not compile the following C# generated code:");
                sb.AppendLine($"**********");
                sb.AppendLine($"{sourceCode}");
                sb.AppendLine($"**********");
                sb.AppendLine($"Here are the compilation results:");
                foreach (var diagnostic in compileResult.Diagnostics)
                {
                    sb.AppendLine(diagnostic.ToString());
                }

                Elog.Warning(sb.ToString());
            }

            return compileResult;
        }

        /// <summary>
        /// This function is called called from C code and tries to create and
        /// compile the dynamic code using Roslyn. It also saves the
        /// CachedFunction in FuncBuiltCodeDict so that any compiled code can
        /// be called by the user function ID.
        /// This function returns 0 if all the codes were compiled correctly.
        /// </summary>
        /// <returns>
        /// Returns 0 when the proccess succeeded, otherwise returns 1.
        /// </returns>
        public static unsafe int CompileUserFunction(uint functionId, IntPtr name, uint returnTypeId, [MarshalAs(UnmanagedType.I1)] bool retset, IntPtr paramNames, uint* paramTypes, byte* paramModes, int num_output_values, IntPtr body, [MarshalAs(UnmanagedType.I1)] bool supportNullInput, IntPtr language)
        {
            // User function Data
            string funcName = Marshal.PtrToStringAuto(name);
            string auxParameters = Marshal.PtrToStringAuto(paramNames);
            string[] paramNameArray = auxParameters == null ? Array.Empty<string>() : auxParameters.Split(" ");
            uint[] paramTypeArray = auxParameters == null ? Array.Empty<uint>() : new ReadOnlySpan<uint>(paramTypes, paramNameArray.Length).ToArray();
            string funcBody = Marshal.PtrToStringAuto(body);
            byte[] paramModeArray = Array.Empty<byte>();

            paramModeArray = (paramModes != null) ? new ReadOnlySpan<byte>(paramModes, paramNameArray.Length).ToArray() : paramModeArray;

            Elog.Info($"START CompileUserFunction, retset is {retset} <foo>");

            // Check if PL.NET supports all the PostgreSQL types of the user function
            if (!CheckSupportedTypes(returnTypeId, paramTypeArray))
            {
                Elog.Warning($"Unsupported return type: {returnTypeId}");
                return 1;
            }

            // Check the directiores access. They need to be 0700.
            try
            {
                CheckDirectoriesAccess();
            }
            catch (Exception e)
            {
                Elog.Info($"{e.GetType().Name}: {e.Message}");
                return 1;
            }

            // Check if the user provides an assembly with the user function
            // The syntax for that is 'UserAssembly.dll:UserNamespace.UserClass!FunctionName'
            bool useUserAssembly = ValidateUserAssembly(funcBody);

            // The language name (just csharp or fsharp for now)
            string plLanguage = Marshal.PtrToStringAuto(language);

            // Generates the source codes
            // PL.NET generates and compiles one or two dynamic codes, according to the language type and wheter the user
            // provided his own assembly.
            // If the user provides his own assembly regardless of the procedural language, PL.NET generates a dynamic code
            // using the `UserHandler.tcs` template, in which the user assembly is included by reference in Roslyn.
            // On the other hand, if the user defines the function body and uses C#, PL.NET generates two dynamic codes using
            // the `UserHandler.tcs` and `UserFunction.tcs` template, so the user function is defined and called in different
            // dynamic codes.
            // Finally, if the procedural language is F# and the user defines the function, PL.NET creates just a dynamic F#
            // code using `UserHandler.tfs`, which is compiled with FSharp.Compiler.Service and has the user function defined
            // at the same code  where it is called.
            // For future improvements, we plan to create two dynamic codes to handle plfsharp, so the UserFunction would be
            // defined in a dynamic F# code and then the generated assembly would be included in the C# code created using the
            // `UserHandler.tcs` template. Thus, the UserHandler codes would be the same for plfsharp and plcsharp.
            // We are not currently doing it that way, because Roslyn fails when we include the reference of the assembly
            // generated by FSharp.Compiler.Service.
            string userFunctionCode, userHandlerCode;
            try
            {
                // The CodeGenerator object that creates the dynamic codes according to the language (C# or F#)
                CodeGenerator dynamicCodeGenerator = (plLanguage == "csharp" || useUserAssembly) ? CSharpGenerator : FSharpGenerator;

                Elog.Info("BUILDING source code");

                // Generate the UserFunction code
                // If the user provides his own assembly, this variable receives the assembly information.
                // If the user function uses F#, this variable receives an empty string, since PL.NET creates the UserFunction
                // together with the UserHandler.
                userFunctionCode = dynamicCodeGenerator.BuildUserFunctionSourceCode(funcName, returnTypeId, retset, paramNameArray, paramTypeArray, paramModeArray, num_output_values, funcBody, supportNullInput || Engine.AlwaysNullable);

                // Generate the UserHandler code
                userHandlerCode = dynamicCodeGenerator.BuildUserHandlerSourceCode(funcName, returnTypeId, retset, paramNameArray, paramTypeArray, paramModeArray, num_output_values, funcBody, supportNullInput || Engine.AlwaysNullable);
            }
            catch (Exception e)
            {
                Elog.Warning($"{e.GetType().Name}: {e.Message}");
                return 1;
            }

            // Check if the user function exists in .NET context
            if (Engine.FuncBuiltCodeDict.TryGetValue(functionId, out CachedFunction cached))
            {
                // check PL.NET needs to recompile the source codes
                if (cached.UserHandlerSourceCode == userHandlerCode && cached.UserFunctionSourceCode == userFunctionCode && !useUserAssembly)
                {
                    return 0;
                }

                FuncBuiltCodeDict[functionId].UserAssemblyLoadContext.Unload();
                FuncBuiltCodeDict.Remove(functionId);
            }

            // The DotNETLanguage of the dynamic codes
            DotNETLanguage dotnetLanguage = (plLanguage == "csharp" || useUserAssembly) ? DotNETLanguage.CSharp : DotNETLanguage.FSharp;

            MemoryStream memUserFunction, memUserHandler;
            try
            {
                // Compile the UserFunction source code, if necessary, and copy the assembly to a MemoryStream object
                memUserFunction = CreateMemoryStreamForUserFunctionCode(dotnetLanguage, functionId, useUserAssembly, userFunctionCode);

                // Compile the UserHandler source code and then copy the assembly to a MemoryStream object
                memUserHandler = CreateMemoryStreamForUserHandlerCode(dotnetLanguage, functionId, funcName, userHandlerCode, memUserFunction);
            }
            catch (Exception e)
            {
                Elog.Info($"{e.GetType().Name}: {e.Message}");
                return 1;
            }

            // Load the assemblies into AssemblyLoadContext
            AssemblyLoadContext userAlc = new ($"UserFunction_{functionId}", true);
            _ = userAlc.LoadFromAssemblyPath(typeof(NpgsqlPoint).Assembly.Location); // Npgsql Assembly
            _ = userAlc.LoadFromAssemblyPath(typeof(Engine).Assembly.Location); // PlDotNET Assembly
            _ = dotnetLanguage == DotNETLanguage.FSharp ? userAlc.LoadFromAssemblyPath(typeof(NpgsqlPoint).Assembly.Location.Replace("Npgsql", "FSharp.Core")) : null; // FSharp.Core
            _ = dotnetLanguage != DotNETLanguage.FSharp ? userAlc.LoadFromStream(new MemoryStream(memUserFunction.GetBuffer())) : null; // UserFunction Assembly
            Assembly userHandlerAssembly = userAlc.LoadFromStream(new MemoryStream(memUserHandler.GetBuffer())); // UserHandler Assembly

            // Create the CachedFunction to keep the function information
            CachedFunction newCachedFunction = new ()
            {
                UserFunctionSourceCode = userFunctionCode,
                UserHandlerSourceCode = userHandlerCode,
                FunctionName = funcName,
                SupportNullInput = supportNullInput,
                UserAssemblyLoadContext = userAlc,
                UserProcedure = GetDirectDelegate(userHandlerAssembly),
                Language = dotnetLanguage,
            };

            // Add the CachedFunction in the dictionary where the key is the function Id
            Engine.FuncBuiltCodeDict.Add(functionId, newCachedFunction);

            memUserFunction.Close();
            memUserHandler.Close();

            return 0;
        }

        /// <summary>
        /// This function returns a MemoryStream object which contains the Assembly for the UserFunction code.
        /// </summary>
        /// <returns>
        /// Returns a memory stream object with the compiled UserFunction code.
        /// </returns>
        public static MemoryStream CreateMemoryStreamForUserFunctionCode(DotNETLanguage language, uint functionId, bool useUserAssembly, string userFunctionCode)
        {
            MemoryStream memUserFunction = new ();
            if (!useUserAssembly)
            {
                if (language == DotNETLanguage.CSharp)
                {
                    var compileResultUserFunction = CompileSourceCode(userFunctionCode, memUserFunction, $"UserFunction_{functionId}");

                    // Verify that the C# code for UserFunction compiled correctly
                    if (!compileResultUserFunction.Success)
                    {
                        throw new SystemException("PL.NET could not compile the generated C# code.");
                    }
                }
            }
            else
            {
                string userAssemblyPath = userFunctionCode.Split(":")[0];
                using var fs = File.Open(userAssemblyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                fs.CopyTo(memUserFunction);
            }

            return memUserFunction;
        }

        /// <summary>
        /// This function returns a MemoryStream object which contains the Assembly for the UserHandler code.
        /// </summary>
        /// <returns>
        /// Returns a memory stream object with the compiled UserHandler code.
        /// </returns>
        public static MemoryStream CreateMemoryStreamForUserHandlerCode(DotNETLanguage language, uint functionId, string functionName, string userHandlerCode, MemoryStream assemblyToInclude)
        {
            MemoryStream memUserHandler = new ();
            if (language == DotNETLanguage.FSharp)
            {
                List<string> extraAssemblies = new ()
                {
                    typeof(NpgsqlPoint).Assembly.Location,
                    typeof(Engine).Assembly.Location,
                    typeof(NpgsqlPoint).Assembly.Location.Replace("Npgsql", "FSharp.Core"),
                };

                return FSharpCompiler.CompileFSharpSourceCode(functionId, Engine.PathToTemporaryFiles, userHandlerCode, extraAssemblies.ToArray());
            }

            var compileResultUserHandler = Engine.CompileSourceCode(userHandlerCode, memUserHandler, $"UserHandler_{functionId}", assemblyToInclude);

            // Verify that the C# code for UserHandler compiled correctly
            if (compileResultUserHandler.Success)
            {
                return memUserHandler;
            }

            throw new SystemException("PL.NET could not compile the generated C# code.");
        }

        /// <summary>
        /// It creates the Delegate function for the CallUserFunction function,
        /// which was compiled by Roslyn.
        /// </summary>
        /// <returns>
        /// Returns the Function object of the delegated CallUserFunction or Null for a failed proccess.
        /// </returns>
        public static Func<List<IntPtr>, IntPtr, ulong, int, bool[], int> GetDirectDelegate(Assembly compiledAssembly)
        {
            Type procClassType = compiledAssembly.GetType("PlDotNET.UserSpace.UserHandler");

            if (procClassType == null)
            {
                Elog.Warning($"Failed to get type PlDotNET.UserSpace.UserHandler");
                return null;
            }

            return (Func<List<IntPtr>, IntPtr, ulong, int, bool[], int>)Delegate.CreateDelegate(
                typeof(Func<List<IntPtr>, IntPtr, ulong, int, bool[], int>),
                null,
                procClassType.GetMethod("CallUserFunction"));
        }

        /// <summary>
        /// This function is called called from C code and tries to get the
        /// cached function by the function id. If the cached functions is not
        /// found, an error message is reported. Otherwise, it calls the
        /// function compiled by Roslyn.
        /// </summary>
        /// <returns>
        /// Returns ReturnMode
        /// </returns>
        public static unsafe int RunUserFunction(uint functionId, ulong call_id, int call_mode, void* arguments, int num_arguments, byte* nullmap, IntPtr output)
        {
            string argaddr = ((IntPtr)arguments).ToString("X");

            IntPtr[] argumentArray = new ReadOnlySpan<IntPtr>(arguments, num_arguments).ToArray();
            List<IntPtr> argumentList = new List<IntPtr>(argumentArray);

            if (Engine.FuncBuiltCodeDict.TryGetValue(functionId, out CachedFunction cached))
            {
                try
                {
                    bool[] isnull = new bool[argumentList.Count];
                    if (cached.SupportNullInput || Engine.AlwaysNullable)
                    {
                        for (int i = 0, nargs = isnull.Length; i < nargs; i++)
                        {
                            isnull[i] = nullmap[i] != 0;
                        }
                    }

                    var retval = cached.UserProcedure(argumentList, output, call_id, call_mode, isnull);
                    return retval;
                }
                catch (Exception e)
                {
                    Elog.Warning($"{e.GetType().Name}: {e.Message}");
                    return (int)ReturnMode.Error;
                }
            }

            Elog.Warning($"PL.NET could not find the user function (ID: {functionId})");
            return (int)ReturnMode.Error;
        }

        /// <summary>
        /// Free memmory pointed by a IntPtr.
        /// </summary>
        public static unsafe void FreeGenericGCHandle(IntPtr p)
        {
            GCHandle.FromIntPtr(p).Free();
        }

        /// <summary>
        /// This functions is called from C and creates a new list of IntPtr,
        /// which pldotnet adds the Datums and passes to RunUserFunction.
        /// </summary>
        /// <returns>
        /// Returns an empty list of IntPtr.
        /// </returns>
        public static unsafe System.IntPtr BuildDatumList()
        {
            GCHandle handle = GCHandle.Alloc(new List<IntPtr>(), GCHandleType.Normal);
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
            if (!FuncBuiltCodeDict.ContainsKey(functionId))
            {
                Elog.Warning($"PL.NET could not find the generated function to unload its assemblies (ID: {functionId})");
                return;
            }

            FuncBuiltCodeDict[functionId].UserAssemblyLoadContext.Unload();
            FuncBuiltCodeDict.Remove(functionId);
        }

        /// <summary>
        /// Checks if PL.NET supports all the PostgreSQL types of the SQL user function.
        /// </summary>
        /// <returns>
        /// Returns true if all types are supported.
        /// </returns>
        public static bool CheckSupportedTypes(uint returnTypeId, uint[] paramTypes)
        {
            List<string> unsupportedTypes = new ();

            if (!(HandleArray.ContainsKey((OID)returnTypeId) || OidTypes.ContainsKey((OID)returnTypeId)))
            {
                unsupportedTypes.Add($"{(OID)returnTypeId}");
            }

            foreach (var paramType in paramTypes)
            {
                if (!(HandleArray.ContainsKey((OID)paramType) || OidTypes.ContainsKey((OID)paramType)))
                {
                    unsupportedTypes.Add($"{(OID)paramType}");
                }
            }

            if (unsupportedTypes.Count == 0)
            {
                return true;
            }

            // Give a helpful error message
            unsupportedTypes = unsupportedTypes.Distinct().ToList();

            var sb = new System.Text.StringBuilder();

            for (int i = 0, length = unsupportedTypes.Count; i < length; i++)
            {
                sb.AppendLine($"PL.NET does not support the PostgreSQL {unsupportedTypes[i]} type.");
            }

            sb.AppendLine("Please contact Brick Abode to inquire about adding support. <winning@brickabode.com>");
            Elog.Warning("\n" + sb.ToString());

            return false;
        }

        /// <summary>
        /// Checks if the user provides a valid Assembly.
        /// </summary>
        /// <returns>
        /// Returns true if the user provides a valid assembly, that is, 'UserAssembly.dll:UserNamespace.UserClass!FunctionName'.
        /// </returns>
        public static bool ValidateUserAssembly(string code)
        {
            return Regex.IsMatch(code, @"^([-/.a-zA-Z0-9]+.dll):([a-zA-Z0-9.]+)!([a-zA-Z0-9]+)$");
        }

        /// <summary>
        /// Checks if the user provides a valid Assembly. If so, modify the arguments with the assembly path, namespace and class names, and the method name.
        /// </summary>
        /// <returns>
        /// Returns true if the user provides a valid assembly, that is, 'UserAssembly.dll:UserNamespace.UserClass!FunctionName'.
        /// </returns>
        public static bool GetInformationFromUserAssembly(string code, ref string assemblyPath, ref string namespaceAndClass, ref string methodName)
        {
            Regex regex = new ("^([-/.a-zA-Z0-9]+.dll):([a-zA-Z0-9.]+)!([a-zA-Z0-9]+)$");
            if (!regex.IsMatch(code))
            {
                return false;
            }

            string[] matches = regex.Split(code);
            assemblyPath = matches[1];
            namespaceAndClass = matches[2];
            methodName = matches[3];
            return true;
        }

        /// <summary>
        /// Check the access of the specified directories.
        /// </summary>
        /// <param name="language">The language of the source code.</param>
        /// <exception cref="SystemException">Thrown if the source code directory or the temporary files directory does not have a mode of 0700.</exception>
        public static void CheckDirectoriesAccess()
        {
            // Check the access of the directory to save the source codes.
            if (Engine.SaveSourceCode)
            {
                if (!Directory.Exists(Engine.PathToSaveSourceCode))
                {
                    // Create the directory if it doesn't exist
                    Directory.CreateDirectory(Engine.PathToSaveSourceCode);
                }

                if (!CheckDirectoryMode(Engine.PathToSaveSourceCode))
                {
                    // Throw an exception if the directory doesn't have the correct mode
                    throw new SystemException($"Please specify a directory where the source codes can be saved and the directory must have a mode of 0700; current directory, '{Engine.PathToSaveSourceCode}', is no good.");
                }
            }

            // Check the access of the temporary files directory
            if (!Directory.Exists(Engine.PathToTemporaryFiles))
            {
                // Create the directory if it doesn't exist
                Directory.CreateDirectory(Engine.PathToTemporaryFiles);
            }

            if (!CheckDirectoryMode(Engine.PathToTemporaryFiles))
            {
                // Throw an exception if the directory doesn't have the correct mode
                throw new SystemException($"Please specify a directory where the temporary files can be saved and the directory must have a mode of 0700; current directory, '{Engine.PathToTemporaryFiles}', is no good.");
            }
        }

        /// <summary>
        /// Check the mode of the specified directory.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <returns>True if the mode of the directory is 0700, false otherwise.</returns>
        public static bool CheckDirectoryMode(string path)
        {
            // Get the mode of the specified directory
            string mode = string.Empty;

            // Execute the "stat" command to get information about the directory
            Process p = new ();
            p.StartInfo.FileName = "/usr/bin/stat";
            p.StartInfo.Arguments = path;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();

            // Read the output of the "stat" command
            string output = p.StandardOutput.ReadToEnd();

            // Use a regular expression to parse the output and extract the mode
            Match m = Regex.Match(output, @"Access:\s+\(([0-9]+)/");
            mode = m.Success ? mode = m.Groups[1].Value : mode;

            // If Linux mode didn't work, then we do Mac mode
            return (mode == "0700") ? true : output.Contains("drwx------");
        }
    }
}
