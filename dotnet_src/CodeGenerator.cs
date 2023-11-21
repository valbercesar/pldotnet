// <copyright file="CodeGenerator.cs" company="Brick Abode">
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
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using PlDotNET.Common;
using PlDotNET.Handler;

// # BuildUserFunctionSourceCode
//    - add parameter: bool retset
//    - UserFunction needs its type changed to be IEnumerable<ret_type> UserFunction(args)
//
// # BuildUserHandlerSourceCode
//    - add parameter: bool retset
//    - SRF UserHandler needs to handle SRF:
//        + CALL_SRF_FIRST:
//            + call UserFunction to get IEnumerator<ret_type>
//            + put in cache
//            + has_more = .EnumerableNext()
//        + CALL_SRF_NEXT:
//            + if (has_more == false) return {value = 0, ReturnMode = SrfDone}
//            + get IEnumerator<ret_type> from cache, return {value = .GetEnumerator(), ReturnMode = SrfNext}
//            + has_more = .EnumerableNext()
//    - Normal (non-SRF) UserHandler needs to require CALL_NORMAL
//    - Argument handling and return values are handled the same for both cases
//        + For SRF, argument handling is only for CALL_SRF_FIRST case
namespace PlDotNET
{
    public abstract class CodeGenerator
    {
        public string UserHandlerTemplatePath;

        public string UserFunctionTemplatePath;

        public DotNETLanguage Language;

        public byte[] InputModes = new byte[] { (byte)ProArgMode.In, (byte)ProArgMode.InOut };
        public byte[] OutputModes = new byte[] { (byte)ProArgMode.Out, (byte)ProArgMode.InOut };

        // taken from catalog/pg_proc.h
        public enum ProArgMode : byte
        {
            [Description("PROARGMODE_IN")]
            In = (byte)'i',
            [Description("PROARGMODE_OUT")]
            Out = (byte)'o',
            [Description("PROARGMODE_INOUT")]
            InOut = (byte)'b',
            [Description("PROARGMODE_VARIADIC")]
            Variadic = (byte)'v',
            [Description("PROARGMODE_TABLE")]
            Table = (byte)'t',
        }

        /// <summary>
        /// Filter the necessary handlers that need to be created in the generated code.
        /// </summary>
        /// <returns>
        /// Returns the type handlers that need to be added in the dynamic code.
        /// </returns>
        public static List<string> FilterHandlers(uint[] inputTypes, uint outputType)
        {
            // We need an NPGSQL object for all arguments: IN/OUT/INOUT.  Thus, we
            // do not filter here by paramMode.
            List<string> allHandlers = new ();

            if ((OID)outputType != OID.VOIDOID)
            {
                allHandlers.Add(DatumConversion.GetTypeHandlerName(outputType));
            }

            for (int i = 0; i < inputTypes.Length; i++)
            {
                allHandlers.Add(DatumConversion.GetTypeHandlerName(inputTypes[i]));
            }

            return allHandlers.Distinct().ToList();
        }

        /// <summary>
        /// Prints the source code if Engine.PrintSourceCode is true.
        /// </summary>
        public static void PrintSourceCode(string sourceCode)
        {
            if (Engine.PrintSourceCode)
            {
                Elog.Info("===========================");
                Elog.Info($"Source code:\n{sourceCode}");
                Elog.Info("===========================");
            }
        }

        /// <summary>
        /// A Nullable message to insert it into the dynamic code.
        /// </summary>
        /// <returns>
        /// Returns the Nullable message.
        /// </returns>
        public static string GetNullableMessage(string funcName)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"// As the SQL function named {funcName} is `STRICT` or `RETURNS NULL ON NULL INPUT`,");
            sb.AppendLine("// `PL.NET` doesn't check whether any argument datum is null.");
            sb.AppendLine("// You can also set true for the `Engine.AlwaysNullable` variable");
            sb.AppendLine("// to always check whether the datum is null.");
            return sb.ToString();
        }

        /// <summary>
        /// Saves the source code if Engine.SaveSourceCode is true.
        /// </summary>
        public void SaveSourceCode(string sourceCode, string fileName)
        {
            if (Engine.SaveSourceCode)
            {
                string path = Path.Combine(Engine.PathToSaveSourceCode, fileName);
                path = Path.ChangeExtension(path, this.Language == DotNETLanguage.CSharp ? ".cs" : ".fs");
                File.WriteAllText(path, sourceCode, Encoding.UTF8);
            }
        }

        /// <summary>
        /// Creates the source code for the UserHandler according to the programming language.
        /// </summary>
        /// <returns>
        /// Returns the generated UserHandler source code.
        /// </returns>
        public string BuildUserHandlerSourceCode(
                string funcName,
                uint returnTypeId,
                bool retset,
                string[] paramNames,
                uint[] paramTypes,
                byte[] paramModes,
                int num_output_values,
                string funcBody,
                bool supportNullInput)
        {
            // Check if the file exists
            if (!File.Exists(this.UserHandlerTemplatePath))
            {
                throw new SystemException($"Template file '{this.UserHandlerTemplatePath}' not found");
            }

            string userFunctionPrefix = "PlDotNET.UserSpace.UserFunction";
            string assemblyPath = string.Empty;
            _ = Engine.GetInformationFromUserAssembly(funcBody, ref assemblyPath, ref userFunctionPrefix, ref funcName);
            string simpleReturnType = this.GetReturnType(returnTypeId, false, paramModes, supportNullInput); // retset=False, so without `IEnumerable<>`

            string[] dotnetTypes = this.GetDotNetTypes(paramTypes, paramModes);

            string sourceCode = File.ReadAllText(this.UserHandlerTemplatePath);
            sourceCode = sourceCode.Replace("// $srf_cache$", this.BuildSRFCache(retset, simpleReturnType));
            sourceCode = sourceCode.Replace("// $handler_objects$", this.BuildHandlerObjects(paramTypes, returnTypeId));
            sourceCode = sourceCode.Replace("// $srf_begin$", this.BuildSRFBegin(retset));
            sourceCode = sourceCode.Replace("// $create_arguments$", this.BuildCreateArguments(funcName, paramTypes, paramModes, supportNullInput));
            sourceCode = sourceCode.Replace("// $user_function_call$", this.BuildFunctionCall(funcName, returnTypeId, dotnetTypes, paramModes, supportNullInput, userFunctionPrefix));
            sourceCode = sourceCode.Replace("// $srf_middle$", this.BuildSRFMiddle(retset));
            sourceCode = sourceCode.Replace("// $call_set_result$", this.BuildCallSetResult(returnTypeId, paramNames, paramTypes, paramModes, num_output_values));
            sourceCode = sourceCode.Replace("// $srf_end$", this.BuildSRFEnd(retset));

            if (this.Language == DotNETLanguage.FSharp)
            {
                // Creates the UserFunction type with the SQL user function
                sourceCode = sourceCode.Replace("// $user_function_declaration$", this.BuildUserFunction(funcName, funcBody, returnTypeId, retset, paramNames, paramModes, dotnetTypes, supportNullInput));
            }

            sourceCode = this.FormatGeneratedCode(sourceCode);

            PrintSourceCode(sourceCode);
            this.SaveSourceCode(sourceCode, $"UserHandler_{funcName}");

            return sourceCode;
        }

        /// <summary>
        /// Creates the SRF cache, or else a small comment if not SRF
        /// </summary>
        /// <remarks>
        /// We need the cache so that the IEnumerator is not garbage
        /// collected.  We store it here and then free it (delete from
        /// the cache) when we get CALL_SRF_CLEANUP.
        /// </remarks>
        /// <returns>
        /// Returns the generated source code.
        /// </returns>
        public string BuildSRFCache(bool retset, string returnType)
        {
            return retset ?
                $"public static Dictionary<ulong, IEnumerator<{returnType}>> EnumeratorCache = new Dictionary<ulong, IEnumerator<{returnType}>>();\n" :
                    "// skipping SRF cache; not a set-returning function";
        }

        /// <summary>
        /// Creates the opening for SRF handling, or else a small comment if not SRF.
        /// </summary>
        /// <remarks>
        /// Generated SRF-handling code:
        ///     1. If CALL_SRF_FIRST, then
        /// that is the end, because the outer code cascades into input
        /// value setup and calling the function.
        /// </remarks>
        /// <returns>
        /// Returns the generated source code.
        /// </returns>
        public string BuildSRFBegin(bool retset)
        {
            if (retset)
            {
                return @"if(call_mode == (int)CallMode.SrfFirst){
                            // Elog.Info($""Got SRF_FIRST on call_id {call_id}"");
                            ";
            }

            return "// skipping SRF setup; not a set-returning function";
        }

        /// <summary>
        /// Creates the middle of the SRF handling, or else a small comment if not SRF.
        /// </summary>
        /// <remarks>
        /// 1. we end the SRF_FIRST handling
        /// 2. we open the SRF_NEXT handling.
        /// </remarks>
        /// <returns>
        /// Returns the generated source code.
        /// </returns>
        public string BuildSRFMiddle(bool retset)
        {
            if (retset)
            {
                return @"
                        // Elog.Info($""Got SRF result ({result.GetType().Name}) {result}"");
                        var enumerator = result.GetEnumerator();
                        // Elog.Info($""Got SRF enumerator ({enumerator.GetType().Name}) {enumerator}; adding to cache under [{call_id}]"");
                        EnumeratorCache.Add(call_id, enumerator);
                        // Elog.Info($""Returning SRF_NEXT"");
                        return (int)ReturnMode.SrfNext;
                    } else if(call_mode == (int)CallMode.SrfNext){
                        // Elog.Info($""Getting SRF enumerator (call_id {call_id})"");
                        var enumerator = EnumeratorCache[call_id];
                        // Elog.Info($""Got SRF enumerator ({enumerator.GetType().Name}) {enumerator}"");
                        if (enumerator.MoveNext() == false) {
                            // Elog.Info($""Enumerator is done; returning SRF_DONE"");
                            return (int)ReturnMode.SrfDone;
                        }
                        // Elog.Info($""Getting SRF current value"");
                        var result = enumerator.Current;
                        // Elog.Info($""Got next result {result}"");
                        // we now cascade to returning the value";
            }

            return "// skipping SRF middle; not a set-returning function";
        }

        /// <summary>
        /// Creates the end of the SRF handling, or else a small comment if not SRF.
        /// </summary>
        /// <remarks>
        /// 1. we return mode SrfNext
        /// 2. we close the SRF_NEXT handling
        /// 3. we handle CALL_SRF_CLEANUP
        /// 4. we error on all other cases.
        /// </remarks>
        /// <returns>
        /// Returns the generated source code.
        /// </returns>
        public string BuildSRFEnd(bool retset)
        {
            if (retset)
            {
                return @"
                                    return (int)ReturnMode.SrfNext;
                                } else if(call_mode == (int)CallMode.SrfCleanup){
                                    Elog.Info($""Removing call_id {call_id} from cache"");
                                    EnumeratorCache.Remove(call_id);
                                    return (int)ReturnMode.SrfDone;
                                }
                                    Elog.Warning($""Unrecognized call mode: {call_mode}"");
                                return (int)ReturnMode.Error;
                                ";
            }

            return "return (int)ReturnMode.Normal;";
        }

        /// <summary>
        /// Creates the source code for the UserFunction.
        /// </summary>
        /// <remarks>
        /// If the user provides an assembly file, this function returns the SQL user function body,
        /// i.e., 'UserAssembly.dll:UserNamespace.UserClass!FunctionName'.
        /// If the user function uses F#, this function returns an empty string.
        /// </remarks>
        /// <returns>
        /// Returns the generated UserFunction source code.
        /// </returns>
        public string BuildUserFunctionSourceCode(
                string funcName,
                uint returnTypeId,
                bool retset,
                string[] paramNames,
                uint[] paramTypes,
                byte[] paramModes,
                int num_output_values,
                string funcBody,
                bool supportNullInput)
        {
            if (Engine.ValidateUserAssembly(funcBody))
            {
                return funcBody;
            }

            if (this.Language == DotNETLanguage.FSharp)
            {
                // Returns an empty string because the UserFunction code is being created along with the UserHandler code
                return string.Empty;
            }

            if (!File.Exists(this.UserFunctionTemplatePath))
            {
                string msg = $"Template file '{this.UserFunctionTemplatePath}' not found";
                throw new SystemException(msg);
            }

            string[] dotnetTypes = this.GetDotNetTypes(paramTypes, paramModes);
            string sourceCode = File.ReadAllText(this.UserFunctionTemplatePath);
            sourceCode = sourceCode.Replace("// $user_function_declaration$", this.BuildUserFunction(funcName, funcBody, returnTypeId, retset, paramNames, paramModes, dotnetTypes, supportNullInput));

            sourceCode = this.FormatGeneratedCode(sourceCode);

            PrintSourceCode(sourceCode);
            this.SaveSourceCode(sourceCode, $"UserFunction_{funcName}");

            return sourceCode;
        }

        public string GetReturnType(uint returnTypeId, bool retset, byte[] paramModes, bool supportNullInput)
        {
            string returnType = DatumConversion.ArrayTypes.ContainsKey((OID)returnTypeId) ? "Array" : DatumConversion.SupportedTypesStr[(OID)returnTypeId];
            bool has_output_var = paramModes.Intersect(this.OutputModes).Any();
            if (has_output_var)
            {
                returnType = "void";
            }

            string nullAbleOutput = (returnType == "void") ? string.Empty : "?";
            returnType = retset ? $"IEnumerable<{returnType}{nullAbleOutput}>" : $"{returnType}{nullAbleOutput}";
            return returnType;
        }

        /// <summary>
        /// Creates the code to create the handler objects that will be used to make the conversions.
        /// </summary>
        /// <returns>
        /// Returns the code to create the handler objects.
        /// </returns>
        public abstract string BuildHandlerObjects(uint[] inputTypes, uint outputType);

        /// <summary>
        /// This function creates the code to call the handler objects, which
        /// do the process of converting a Postgres type to an equivalente .NET
        /// type.
        /// </summary>
        /// <returns>
        /// Returns the user function arguments.
        /// </returns>
        public abstract string BuildCreateArguments(string funcName, uint[] paramTypes, byte[] paramModes, bool supportNullInput);

        /// <summary>
        /// This function creates code to call the user function.
        /// </summary>
        /// <returns>
        /// Returns the code to call the user function.
        /// </returns>
        public abstract string BuildFunctionCall(string funcName, uint returnTypeId, string[] dotnetTypes, byte[] paramModes, bool supportNullInput, string prefix);

        /// <summary>
        /// This function creates the code to create the Datum result according
        /// to the OID of the result function. It also adds the code to set the
        /// Datum object to the function output.
        /// </summary>
        /// <returns>
        /// Returns the created code as string.
        /// </returns>
        public abstract string BuildCallSetResult(uint returnTypeId, string[] paramNames, uint[] paramTypes, byte[] paramModes, int num_output_values);

        /// <summary>
        /// This function creates user function.
        /// </summary>
        /// <returns>
        /// Returns user function as string.
        /// </returns>
        public abstract string BuildUserFunction(string funcName, string funcBody, uint returnTypeId, bool retset, string[] paramNames, byte[] paramModes, string[] dotnetTypes, bool supportNullInput);

        /// <summary>
        /// Get the .NET types of the SQL user function according to the language.
        /// </summary>
        /// <returns>
        /// Returns the types of each function argument.
        /// </returns>
        public abstract string[] GetDotNetTypes(uint[] paramTypes, byte[] paramModes);

        /// <summary>
        /// This function formats the generated code.
        /// </summary>
        /// <returns>
        /// Returns the formatted code.
        /// </returns>
        public abstract string FormatGeneratedCode(string sourceCode);
    }

    public class CSharpCodeGenerator : CodeGenerator
    {
        public CSharpCodeGenerator()
        {
            this.UserHandlerTemplatePath = "@PLDOTNET_TEMPLATE_DIR/UserHandler.tcs";
            this.UserFunctionTemplatePath = "@PLDOTNET_TEMPLATE_DIR/UserFunction.tcs";
            this.Language = DotNETLanguage.CSharp;
        }

        /// <inheritdoc />
        public override string BuildHandlerObjects(uint[] inputTypes, uint outputType)
        {
            var sb = new System.Text.StringBuilder();
            foreach (string handler in FilterHandlers(inputTypes, outputType))
            {
                sb.AppendLine($"public static {handler} {handler}Obj = new {handler}();");
            }

            return sb.ToString();
        }

        /// <inheritdoc />
        public override string BuildCreateArguments(string funcName, uint[] paramTypes, byte[] paramModes, bool supportNullInput)
        {
            var sb = new System.Text.StringBuilder();
            string[] dotNetTypes = this.GetDotNetTypes(paramTypes, paramModes);
            int argc = paramTypes.Length;
            int skips = 0;

            sb.AppendLine(supportNullInput ? string.Empty : GetNullableMessage(funcName));
            sb.AppendLine($"// BEGIN create arguments for {funcName}");

            for (int i = 0; i < argc; i++)
            {
                string argType = (this.OutputModes.Contains(paramModes[i]) || supportNullInput) ? $"{dotNetTypes[i]}?" : dotNetTypes[i];

                if (this.OutputModes.Contains(paramModes[i]))
                {
                    sb.AppendLine($"// Argument argument_{i} is {argType} because it's an output ('{((char)paramModes[i]).ToString()}') variable");
                }

                if (this.InputModes.Contains(paramModes[i]))
                {
                    string handler = DatumConversion.GetTypeHandlerName(paramTypes[i]);
                    string null_input = supportNullInput ? $", isnull[{i - skips}]" : string.Empty;

                    string inputMethod = DatumConversion.ArrayTypes.ContainsKey((OID)paramTypes[i]) ?
                        (supportNullInput ? "InputNullableArray" : "InputArray") :
                        (supportNullInput ? "InputNullableValue" : "InputValue");
                    sb.AppendLine($"{argType} argument_{i} = {handler}Obj.{inputMethod}(arguments[{i - skips}]{null_input});");
                }
                else if (paramModes[i] == (byte)ProArgMode.Out)
                {
                    sb.AppendLine($"{argType} argument_{i};");
                    skips += 1;
                }
                else
                {
                    throw new SystemException($"Unsupported mode {paramModes[i]} on parameter number {i} of {funcName}: all modes are {string.Join(", ", paramModes)}.  [1]");
                }
            }

            sb.Append($"// END create arguments for {funcName}");
            return sb.ToString();
        }

        /// <inheritdoc />
        public override string BuildFunctionCall(string funcName, uint returnTypeId, string[] dotnetTypes, byte[] paramModes, bool supportNullInput, string prefix)
        {
            var sb = new System.Text.StringBuilder();
            bool has_output_var = paramModes.Intersect(this.OutputModes).Any();
            returnTypeId = has_output_var ? (uint)OID.VOIDOID : returnTypeId;

            sb.AppendLine(string.Empty);
            if ((OID)returnTypeId != OID.VOIDOID)
            {
                sb.AppendLine("var result = ");
            }

            sb.Append($"{prefix}.{funcName}(");

            string aux = supportNullInput ? "?" : string.Empty;
            for (int i = 0, argc = dotnetTypes.Length; i < argc; i++)
            {
                switch (paramModes[i])
                {
                    case (byte)ProArgMode.In:
                        sb.Append($"({dotnetTypes[i]}{aux}) argument_{i}");
                        break;
                    case (byte)ProArgMode.InOut:
                        sb.Append($"ref argument_{i}");
                        break;
                    case (byte)ProArgMode.Out:
                        sb.Append($"out argument_{i}");
                        break;
                    default:
                        throw new SystemException($"Unrecognized parameter mode: {paramModes[i]}, slot {i}");
                }

                sb.Append((i < argc - 1) ? ", " : string.Empty);
            }

            sb.Append(");");
            return sb.ToString();
        }

        /// <inheritdoc />
        public override string BuildCallSetResult(uint returnTypeId, string[] paramNames, uint[] paramTypes, byte[] paramModes, int num_output_values)
        {
            var sb = new System.Text.StringBuilder();

            if ((OID)returnTypeId == OID.VOIDOID)
            {
                return string.Empty;
            }

            // This just assigns the Datum in the case where we want the return value.
            // That is, not using INOUT or OUT arguments.
            sb.AppendLine(string.Empty);
            sb.AppendLine($"// Handling {num_output_values} output values");
            if (num_output_values == 0)
            {
                sb.AppendLine($"// Handling normal function return (no INOUT/OUT arguments)");
                string output_handler = DatumConversion.ArrayTypes.ContainsKey((OID)returnTypeId) ? "OutputNullableArray" : "OutputNullableValue";
                sb.AppendLine($"IntPtr resultDatum = {DatumConversion.GetTypeHandlerName(returnTypeId)}Obj.{output_handler}(result);");
                sb.AppendLine($"OutputResult.SetDatumResult(resultDatum, result == null, output, 0, {returnTypeId});");
            }
            else if (num_output_values == 1)
            {
                // find the 1 output value and return it
                int output_parameter_offset = Enumerable.Range(0, paramModes.Length).FirstOrDefault(i => this.OutputModes.Contains(paramModes[i]), -1);
                var outResultName = $"argument_{output_parameter_offset}";
                string oututHandler = DatumConversion.ArrayTypes.ContainsKey((OID)returnTypeId) ? "OutputNullableArray" : "OutputNullableValue";

                if (output_parameter_offset == -1)
                {
                    throw new SystemException($"Could not find output parameter from modes: {string.Join(", ", paramModes)}");
                }

                sb.AppendLine($"// Handling single OUT return value `{outResultName}`, in slot {output_parameter_offset}");
                sb.AppendLine($"IntPtr resultDatum = {DatumConversion.GetTypeHandlerName(returnTypeId)}Obj.{oututHandler}({outResultName});");
                sb.AppendLine($"OutputResult.SetDatumResult(resultDatum, {outResultName} == null, output, 0, {returnTypeId});");
            }
            else if (num_output_values > 1)
            {
                int i;
                int skips = 0;

                for (i = 0; i < paramNames.Length; i++)
                {
                    if (!this.OutputModes.Contains(paramModes[i]))
                    {
                        sb.AppendLine($"// Skipping non-output-mode ({((char)paramModes[i]).ToString()}) argument {i}");
                        skips += 1;
                        continue;
                    }

                    string handler = DatumConversion.GetTypeHandlerName(paramTypes[i]);
                    var outResultName = $"argument_{i}";
                    var outputHandler = DatumConversion.ArrayTypes.ContainsKey((OID)paramTypes[i]) ? "OutputNullableArray" : "OutputNullableValue";
                    sb.AppendLine($"// Adding output-mode ({((char)paramModes[i]).ToString()}) argument {i} for oid {returnTypeId}");
                    sb.AppendLine($"IntPtr resultDatum_{i} = {handler}Obj.{outputHandler}({outResultName});");
                    sb.AppendLine($"OutputResult.SetDatumResult(resultDatum_{i}, argument_{i} == null, output, {i - skips}, {(int)paramTypes[i]});");
                }
            }
            else
            {
                throw new SystemException($"Unsupported number of arguments: {num_output_values}");
            }

            return sb.ToString();
        }

        /// <inheritdoc />
        public override string BuildUserFunction(string funcName, string funcBody, uint returnTypeId, bool retset, string[] paramNames, byte[] paramModes, string[] dotnetTypes, bool supportNullInput)
        {
            var sb = new System.Text.StringBuilder();
            string returnType = this.GetReturnType(returnTypeId, retset, paramModes, supportNullInput);
            string aux = supportNullInput ? "?" : string.Empty;

            // for Set-Returning Functions, we create a C# generator
            Elog.Info($"BuildUserFunction: retset is {retset}, returnType is {returnType}");

            sb.Append($"public static {returnType} {funcName}(");

            for (int i = 0, argc = dotnetTypes.Length; i < argc; i++)
            {
                switch (paramModes[i])
                {
                    case (byte)ProArgMode.In:
                        sb.Append($"{dotnetTypes[i]}{aux} {paramNames[i]}");
                        break;
                    case (byte)ProArgMode.InOut:
                        // output variables are always nullable
                        sb.Append($"ref {dotnetTypes[i]}? {paramNames[i]}");
                        break;
                    case (byte)ProArgMode.Out:
                        // output variables are always nullable
                        sb.Append($"out {dotnetTypes[i]}? {paramNames[i]}");
                        break;
                    default:
                        throw new SystemException($"Unsupported mode {paramModes[i]} on parameter number {i} of {funcName}: all modes are {string.Join(", ", paramModes)}.  [2]");
                }

                sb.Append((i < argc - 1) ? ", " : string.Empty);
            }

            sb.Append($") {{\n#line 1\n{funcBody}\n}}");
            return sb.ToString();
        }

        /// <inheritdoc />
        public override string[] GetDotNetTypes(uint[] paramTypes, byte[] paramModes)
        {
            return paramTypes.Select(t => DatumConversion.ArrayTypes.ContainsKey((OID)t) ? "Array" : DatumConversion.SupportedTypesStr[(OID)t]).ToArray();
        }

        /// <inheritdoc />
        public override string FormatGeneratedCode(string sourceCode)
        {
            SyntaxTree userTree = SyntaxFactory.ParseSyntaxTree(sourceCode);
            SyntaxNode node = userTree.GetRoot().NormalizeWhitespace();
            sourceCode = node.ToFullString();
            return sourceCode;
        }
    }

    public class FSharpCodeGenerator : CodeGenerator
    {
        /// <summary>
        /// This Dictionary contains the C# types that differs from F# type names.
        /// </summary>
        private static readonly Dictionary<string, string> FSharpTypes =
               new ()
        {
            { "float", "float32" },
            { "short", "int16" },
            { "long", "int64" },
            { "NpgsqlRange<long>", "NpgsqlRange<int64>" },
            { "(IPAddress Address, int Netmask)", "struct(IPAddress*int)" },
        };

        /// <summary>
        /// This List contains object types which are inherently Nullable.
        /// </summary>
        private static readonly List<string> ClassTypes =
               new ()
        {
            "Array",
            "byte[]",
            "BitArray",
            "string",
            "PhysicalAddress",
        };

        public FSharpCodeGenerator()
        {
            this.Language = DotNETLanguage.FSharp;
            this.UserHandlerTemplatePath = "@PLDOTNET_TEMPLATE_DIR/UserHandler.tfs";
            this.UserFunctionTemplatePath = "@PLDOTNET_TEMPLATE_DIR/UserFunction.tfs";
        }

        /// <summary>
        /// Indents code according to the provided number of space.
        /// </summary>
        /// <returns>
        /// Returns the indented code.
        /// </returns>
        public static string IndentCode(string code, uint spaceNumber)
        {
            string indentation = new (' ', (int)spaceNumber);
            string newline = string.Empty;
            var sb = new System.Text.StringBuilder();

            foreach (string line in code.Split("\n"))
            {
                sb.Append(string.IsNullOrWhiteSpace(line) ? newline : $"{newline}{indentation}{line}");
                newline = "\n";
            }

            return sb.ToString();
        }

        /// <inheritdoc />
        public override string BuildHandlerObjects(uint[] inputTypes, uint outputType)
        {
            var sb = new System.Text.StringBuilder();
            FilterHandlers(inputTypes, outputType).ToList().ForEach(handler => sb.AppendLine($"let {handler}Obj = new {handler}()"));
            return "// handler objects\n" + IndentCode(sb.ToString(), 8);
        }

        /// <inheritdoc />
        public override string BuildCreateArguments(string funcName, uint[] paramTypes, byte[] paramModes, bool supportNullInput)
        {
            int skips = 0, argc = paramTypes.Length;
            string[] dotNetTypes = this.GetDotNetTypes(paramTypes, paramModes);
            var sb = new System.Text.StringBuilder();

            sb.AppendLine($"// BEGIN create arguments for {funcName}");

            for (int i = 0; i < argc; i++)
            {
                // Because F# is a functional language, it does not support INOUT or OUT arguments like C# does.
                // Instead, IN and INOUT are treated as normal arguments, and INOUT and OUT get `output_{i}` variables
                // to receive their return values.
                string handler = DatumConversion.GetTypeHandlerName(paramTypes[i]);
                string argType = (this.OutputModes.Contains(paramModes[i]) || supportNullInput) ? $"{dotNetTypes[i]}?" : dotNetTypes[i];

                if (this.InputModes.Contains(paramModes[i]))
                {
                    string handlerName = DatumConversion.GetTypeHandlerName(paramTypes[i]);
                    string null_input = supportNullInput ? $", isnull[{i - skips}]" : string.Empty;
                    string inputMethod = DatumConversion.ArrayTypes.ContainsKey((OID)paramTypes[i]) ?
                        (supportNullInput ? "InputNullableArray" : "InputArray") :
                        (supportNullInput ? "InputNullableValue" : "InputValue");
                    sb.AppendLine($"let argument_{i} = {handler}Obj.{inputMethod}(arguments[{i - skips}]{null_input});");
                }
                else if (paramModes[i] == (byte)ProArgMode.Out)
                {
                    skips += 1;
                }
                else
                {
                    throw new SystemException($"Unsupported mode {paramModes[i]} on parameter number {i} of {funcName}: all modes are {string.Join(", ", paramModes)}.  [1]");
                }
            }

            sb.AppendLine($"// END create arguments for {funcName}");
            return "\n" + IndentCode(sb.ToString(), 8);
        }

        /// <inheritdoc />
        public override string BuildFunctionCall(string funcName, uint returnTypeId, string[] dotnetTypes, byte[] paramModes, bool supportNullInput, string prefix)
        {
            // FIXME: we need the oid, not the type names, so we can check f# type conversions
            List<string> retvals = new List<string>();
            List<string> arguments = new List<string>();
            List<string> variables = new List<string>();
            string let_result;
            int i, output_num = 0;

            for (i = 0; i < dotnetTypes.Length; i++)
            {
                if (this.InputModes.Contains(paramModes[i]))
                {
                    arguments.Add($"argument_{i}");
                }

                if (this.OutputModes.Contains(paramModes[i]))
                {
                    variables.Add($"{dotnetTypes[i]} output_{i};");
                    retvals.Add($"output_{i}");
                    output_num++;
                }
            }

            // set the `let_result` string to get the return values
            if (retvals.Count > 1)
            {
                let_result = $"let {string.Join(", ", retvals)} = ";
            }
            else if (retvals.Count == 1)
            {
                let_result = $"let {retvals[0]} = ";
            }
            else if (retvals.Count == 0)
            {
                let_result = ((OID)returnTypeId != OID.VOIDOID) ? "let result = " : string.Empty;
            }
            else
            {
                throw new SystemException($"Bizarre retvals.Count {retvals.Count}");
            }

            string argstring = string.Join(" ", arguments);

            return "// Calling user function\n" + IndentCode($"{let_result}UserFunction.{funcName} {argstring}\n", 8);
        }

        /// <inheritdoc />
        public override string BuildCallSetResult(uint returnTypeId, string[] paramNames, uint[] paramTypes, byte[] paramModes, int num_output_values)
        {
            int i, output_num = 0;
            var sb = new System.Text.StringBuilder();

            if ((OID)returnTypeId == OID.VOIDOID)
            {
                return string.Empty;
            }

            if (num_output_values < 0)
            {
                throw new SystemException($"Unrecognized num_output_values: {num_output_values}");
            }

            if (num_output_values == 0)
            {
                // use "result"
                string type = DatumConversion.ArrayTypes.ContainsKey((OID)returnTypeId) ? "Array" : DatumConversion.SupportedTypesStr[(OID)returnTypeId];
                string returnType = FSharpTypes.ContainsKey(type) ? FSharpTypes[type] : type;
                string outputHandler = DatumConversion.ArrayTypes.ContainsKey((OID)returnTypeId) ? "OutputNullableArray" : "OutputNullableValue";
                string isnull = ClassTypes.Contains(returnType) ? "Object.ReferenceEquals(result, null)" : "not result.HasValue";
                sb.AppendLine($"// Handling normal function return (no INOUT/OUT arguments)");

                string makeDatum = $"let resultDatum = {DatumConversion.GetTypeHandlerName(returnTypeId)}Obj.{outputHandler}(result)";
                sb.AppendLine(makeDatum);
                string setDatum = $"OutputResult.SetDatumResult(resultDatum, {isnull}, output, 0, uint32 {returnTypeId})";
                sb.AppendLine(setDatum);
                return "// Create PostgreSQL datum\n" + IndentCode(sb.ToString(), 8);
            }

            // num_output_values > 1, so use "output_0", "output_1", etc
            for (i = 0; i < paramTypes.Length; i++)
            {
                if (this.OutputModes.Contains(paramModes[i]))
                {
                    string outputTypeHandler = DatumConversion.GetTypeHandlerName(paramTypes[i]);
                    string type = DatumConversion.ArrayTypes.ContainsKey((OID)paramTypes[i]) ? "Array" : DatumConversion.SupportedTypesStr[(OID)paramTypes[i]];
                    string returnType = FSharpTypes.ContainsKey(type) ? FSharpTypes[type] : type;
                    string outputHandlerMethod = DatumConversion.ArrayTypes.ContainsKey((OID)paramTypes[i]) ? "OutputNullableArray" : "OutputNullableValue";
                    string isnull = ClassTypes.Contains(returnType) ? $"Object.ReferenceEquals(output_{i}, null)" : $"not output_{i}.HasValue";

                    sb.AppendLine($"let resultDatum_{output_num} = {outputTypeHandler}Obj.{outputHandlerMethod}(output_{i})");
                    sb.AppendLine($"OutputResult.SetDatumResult(resultDatum_{output_num}, {isnull}, output, {output_num}, uint32 {(int)paramTypes[i]})");

                    output_num++;
                }
            }

            return "// Create PostgreSQL datums for INOUT/OUT parameters\n" + IndentCode(sb.ToString(), 8);
        }

        /// <inheritdoc />
        public override string BuildUserFunction(string funcName, string funcBody, uint returnTypeId, bool retset, string[] paramNames, byte[] paramModes, string[] dotnetTypes, bool supportNullInput)
        {
            var sb = new System.Text.StringBuilder();
            string return_type = DatumConversion.ArrayTypes.ContainsKey((OID)returnTypeId) ? "Array" : DatumConversion.SupportedTypesStr[(OID)returnTypeId];
            return_type = FSharpTypes.ContainsKey(return_type) ? FSharpTypes[return_type] : return_type;
            List<string> outputTypes = new ();

            // returns are always nullable in PostgreSQL
            if (return_type != "void" && !ClassTypes.Contains(return_type))
            {
                return_type = $"Nullable<{return_type}>";
            }

            sb.Append($"static member {funcName}");

            for (int i = 0, length = paramNames.Length; i < length; i++)
            {
                if (this.OutputModes.Contains(paramModes[i]))
                {
                    string outputParamType = FSharpTypes.ContainsKey(dotnetTypes[i]) ? FSharpTypes[dotnetTypes[i]] : dotnetTypes[i];
                    outputParamType = ClassTypes.Contains(dotnetTypes[i]) ? dotnetTypes[i] : $"Nullable<{dotnetTypes[i]}>";
                    outputTypes.Add(outputParamType);
                }

                if (this.InputModes.Contains(paramModes[i]))
                {
                    string inputParamType = dotnetTypes[i];
                    if (supportNullInput && (!ClassTypes.Contains(inputParamType)))
                    {
                       inputParamType = $"Nullable<{inputParamType}>";
                    }

                    sb.Append($" ({paramNames[i]}: {inputParamType})");
                }
            }

            if (outputTypes.Count > 1)
            {
                return_type = string.Join(" * ", outputTypes);
            }
            else if (outputTypes.Count == 1)
            {
                return_type = outputTypes[0];
            }

            if (return_type == "void")
            {
                sb.Append($" = {IndentCode(funcBody, 8)}");
            }
            else
            {
                sb.Append($" : {return_type} = {IndentCode(funcBody, 8)}");
            }

            return sb.ToString();
        }

        /// <inheritdoc />
        public override string[] GetDotNetTypes(uint[] paramTypes, byte[] paramModes)
        {
            string[] dotnetTypes = new string[paramTypes.Length];
            for (int i = 0, length = paramTypes.Length; i < length; i++)
            {
                string type = DatumConversion.ArrayTypes.ContainsKey((OID)paramTypes[i]) ? "Array" : DatumConversion.SupportedTypesStr[(OID)paramTypes[i]];
                dotnetTypes[i] = FSharpTypes.ContainsKey(type) ? FSharpTypes[type] : type;
            }

            return dotnetTypes;
        }

        /// <inheritdoc />
        public override string FormatGeneratedCode(string sourceCode)
        {
            return sourceCode;
        }
    }
}
