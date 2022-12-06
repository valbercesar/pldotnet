// <copyright file="CodeGenerator.cs" company="Brick Abode">
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
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using PlDotNET.Handler;

namespace PlDotNET
{
    public abstract class CodeGenerator
    {
        public string UserHandlerTemplatePath;

        public string UserFunctionTemplatePath;

        public string PathToGeneratedCode;

        public DotNETLanguage Language;

        /// <summary>
        /// Filter the necessary handlers that need to be created in the generated code.
        /// </summary>
        public static List<string> FilterHandlers(uint[] inputTypes, uint outputType)
        {
            List<string> allHandlers = new ();

            if ((OID)outputType != OID.VOIDOID)
            {
                allHandlers.Add(Engine.GetTypeHandler(outputType));
            }

            for (int i = 0; i < inputTypes.Length; i++)
            {
                allHandlers.Add(Engine.GetTypeHandler(inputTypes[i]));
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
                Elog.pldotnet_Info("===========================");
                Elog.pldotnet_Info($"Source code:\n{sourceCode}");
                Elog.pldotnet_Info("===========================");
            }
        }

        /// <summary>
        /// Returns the Nullable message to insert it into the dynamic code.
        /// </summary>
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
        /// Returns the source code for the UserHandler according to the programming language.
        /// </summary>
        public string BuildUserHandlerSourceCode(string funcName, uint returnTypeId, string[] paramNames, uint[] paramTypes, string funcBody, bool supportNullInput)
        {
            // Check if the file exists
            if (!File.Exists(this.UserHandlerTemplatePath))
            {
                throw new SystemException($"Template file '{this.UserHandlerTemplatePath}' not found");
            }

            bool providedAssembly = funcBody.Contains(".dll:");
            string nampespace = "PlDotNET.UserSpace";
            string className = "UserFunction";
            if (providedAssembly)
            {
                string[] bodySplit = funcBody.Split(':');
                bodySplit = bodySplit[1].Split('.');
                nampespace = bodySplit[0]; // UserNamespace
                bodySplit = bodySplit[1].Split('!');
                className = bodySplit[0]; // UserClass
                funcName = bodySplit[1]; // FunctionName
            }

            string[] dotnetTypes = this.GetDotNetTypes(paramTypes);

            string sourceCode = File.ReadAllText(this.UserHandlerTemplatePath);
            sourceCode = sourceCode.Replace("// $handler_objects$", this.BuildHandlerObjects(paramTypes, returnTypeId));
            sourceCode = sourceCode.Replace("// $create_arguments", this.BuildCreateArguments(funcName, paramTypes, supportNullInput));
            sourceCode = sourceCode.Replace("// $user_function_call$", this.BuildFunctionCall(funcName, returnTypeId, dotnetTypes, supportNullInput, className));
            sourceCode = sourceCode.Replace("// $call_set_result$", this.BuildCallSetResult(returnTypeId));

            if (providedAssembly)
            {
                sourceCode = sourceCode.Replace("// $user_namespace$", $"using {nampespace};\n");
            }
            else if (this.Language == DotNETLanguage.FSharp)
            {
                // Creates the UserFunction type with the SQL user function
                sourceCode = sourceCode.Replace("// $user_function_declaration$", this.BuildUserFunction(funcName, funcBody, returnTypeId, paramNames, dotnetTypes, supportNullInput));
            }

            return this.FormatAndSaveGeneratedCode(sourceCode, $"UserHandler_{funcName}");
        }

        /// <summary>
        /// Returns the source code for the UserFunction.
        /// </summary>
        /// If the user provides an assembly file, this function returns the SQL user function body,
        /// i.e., 'UserAssembly.dll:UserNamespace.UserClass!FunctionName'.
        /// If the user function uses F#, this function returns an empty string.
        /// </remarks>
        public string BuildUserFunctionSourceCode(string funcName, uint returnTypeId, string[] paramNames, uint[] paramTypes, string funcBody, bool supportNullInput)
        {
            if (funcBody.Contains(".dll:"))
            {
                return funcBody;
            }

            if (this.Language == DotNETLanguage.FSharp)
            {
                return string.Empty;
            }

            if (!File.Exists(this.UserFunctionTemplatePath))
            {
                string msg = $"Template file '{this.UserFunctionTemplatePath}' not found";
                throw new SystemException(msg);
            }

            string[] dotnetTypes = this.GetDotNetTypes(paramTypes);
            string sourceCode = File.ReadAllText(this.UserFunctionTemplatePath);
            sourceCode = sourceCode.Replace("// $user_function_declaration$", this.BuildUserFunction(funcName, funcBody, returnTypeId, paramNames, dotnetTypes, supportNullInput));
            return this.FormatAndSaveGeneratedCode(sourceCode, $"UserFunction_{funcName}");
        }

        /// <summary>
        /// Returns the code to create the handler object that will be used.
        /// </summary>
        public abstract string BuildHandlerObjects(uint[] inputTypes, uint outputType);

        /// <summary>
        /// This function creates the code to call the handler objects, which
        /// do the process of converting a Postgres type to an equivalente .NET
        /// type.
        /// </summary>
        public abstract string BuildCreateArguments(string funcName, uint[] paramTypes, bool supportNullInput);

        /// <summary>
        /// This function creates code to call the user function.
        /// </summary>
        public abstract string BuildFunctionCall(string funcName, uint returnTypeId, string[] dotnetTypes, bool supportNullInput, string className = "UserFunction");

        /// <summary>
        /// This function returns the code to create the Datum result according
        /// to the OID of the result function. It also adds the code to set the
        /// Datum object to the function output.
        /// </summary>
        public abstract string BuildCallSetResult(uint id);

        /// <summary>
        /// This function creates user function.
        /// </summary>
        public abstract string BuildUserFunction(string funcName, string funcBody, uint returnTypeId, string[] paramNames, string[] dotnetTypes, bool supportNullInput);

        /// <summary>
        /// Returns the .NET types of the SQL user function according to the language.
        /// </summary>
        public abstract string[] GetDotNetTypes(uint[] paramTypes);

        /// <summary>
        /// This function formats and saves the generated code.
        /// </summary>
        /// <returns>
        /// Returns the formatted code.
        /// </returns>
        public abstract string FormatAndSaveGeneratedCode(string sourceCode, string fileName);
    }

    public class CSharpCodeGenerator : CodeGenerator
    {
        public CSharpCodeGenerator()
        {
            this.UserHandlerTemplatePath = "@CSHARP_TEMPLATE_DIR/UserHandler.tcs";
            this.UserFunctionTemplatePath = "@CSHARP_TEMPLATE_DIR/UserFunction.tcs";
            this.Language = DotNETLanguage.CSharp;
            this.PathToGeneratedCode = $"{Engine.PathToGeneratedCode}csharp";
            if (!Directory.Exists(this.PathToGeneratedCode))
            {
                Directory.CreateDirectory(this.PathToGeneratedCode);
            }
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
        public override string BuildCreateArguments(string funcName, uint[] paramTypes, bool supportNullInput)
        {
            var sb = new System.Text.StringBuilder();
            if (!supportNullInput)
            {
                sb.AppendLine(GetNullableMessage(funcName));
            }

            sb.AppendLine($"// BEGIN create arguments for {funcName}");
            int argc = paramTypes.Length;
            for (int i = 0; i < argc; i++)
            {
                string handler = Engine.GetTypeHandler(paramTypes[i]);
                if (Engine.HandleArray.ContainsKey((OID)paramTypes[i]))
                {
                    if (supportNullInput || Engine.AlwaysNullable)
                    {
                        sb.AppendLine($"var argument_{i} = {handler}Obj.InputNullableArray(arguments[{i}], isnull[{i}]);");
                    }
                    else
                    {
                        sb.AppendLine($"var argument_{i} = {handler}Obj.InputArray(arguments[{i}]);");
                    }
                }
                else
                {
                    if (supportNullInput || Engine.AlwaysNullable)
                    {
                        sb.AppendLine($"var argument_{i} = {handler}Obj.InputNullableValue(arguments[{i}], isnull[{i}]);");
                    }
                    else
                    {
                        sb.AppendLine($"var argument_{i} = {handler}Obj.InputValue(arguments[{i}]);");
                    }
                }
            }

            sb.Append($"// END create arguments for {funcName}");
            return sb.ToString();
        }

        /// <inheritdoc />
        public override string BuildFunctionCall(string funcName, uint returnTypeId, string[] dotnetTypes, bool supportNullInput, string className = "UserFunction")
        {
            var sb = new System.Text.StringBuilder();
            if ((OID)returnTypeId != OID.VOIDOID)
            {
                sb.AppendLine("var result = ");
            }

            sb.Append($"{className}.{funcName}(");
            string aux = (supportNullInput || Engine.AlwaysNullable) ? "?" : string.Empty;
            for (int i = 0, argc = dotnetTypes.Length; i < argc; i++)
            {
                sb.Append($"({dotnetTypes[i]}{aux}) argument_{i}");
                if (i < argc - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append(");");
            return sb.ToString();
        }

        /// <inheritdoc />
        public override string BuildCallSetResult(uint returnTypeId)
        {
            var sb = new System.Text.StringBuilder();
            if ((OID)returnTypeId == OID.VOIDOID)
            {
                sb.AppendLine("Engine.pldotnet_SetDatumResult(new IntPtr(0), false, output);");
            }
            else
            {
                if (Engine.HandleArray.ContainsKey((OID)returnTypeId))
                {
                    sb.AppendLine($"var resultDatum = {Engine.GetTypeHandler(returnTypeId)}Obj.OutputNullableArray(result);");
                }
                else
                {
                    sb.AppendLine($"var resultDatum = {Engine.GetTypeHandler(returnTypeId)}Obj.OutputNullableValue(result);");
                }

                sb.AppendLine("Engine.pldotnet_SetDatumResult(resultDatum, result == null, output);");
            }

            return sb.ToString();
        }

        /// <inheritdoc />
        public override string BuildUserFunction(string funcName, string funcBody, uint returnTypeId, string[] paramNames, string[] dotnetTypes, bool supportNullInput)
        {
            var sb = new System.Text.StringBuilder();
            string returnType = Engine.HandleArray.ContainsKey((OID)returnTypeId) ? "Array" : Engine.OidTypes[(OID)returnTypeId];
            string nullAbleOutput = returnType == "void" ? string.Empty : "?";
            string aux = (supportNullInput || Engine.AlwaysNullable) ? "?" : string.Empty;

            sb.Append($"public static {returnType}{nullAbleOutput} {funcName}(");
            for (int i = 0, length = paramNames.Length; i < length; i++)
            {
                sb.Append($"{dotnetTypes[i]}{aux} {paramNames[i]}");
                if (i < length - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append($") {{\n#line 1\n{funcBody}\n}}");
            return sb.ToString();
        }

        /// <inheritdoc />
        public override string[] GetDotNetTypes(uint[] paramTypes)
        {
            string[] dotnetTypes = new string[paramTypes.Length];
            for (int i = 0, length = paramTypes.Length; i < length; i++)
            {
                dotnetTypes[i] = Engine.HandleArray.ContainsKey((OID)paramTypes[i]) ? "Array" : Engine.OidTypes[(OID)paramTypes[i]];
            }

            return dotnetTypes;
        }

        /// <inheritdoc />
        public override string FormatAndSaveGeneratedCode(string sourceCode, string fileName)
        {
            SyntaxTree userTree = SyntaxFactory.ParseSyntaxTree(sourceCode);
            SyntaxNode node = userTree.GetRoot().NormalizeWhitespace();
            sourceCode = node.ToFullString();
            File.WriteAllText($"{this.PathToGeneratedCode}/{fileName}.cs", sourceCode, Encoding.UTF8);
            PrintSourceCode(sourceCode);
            return sourceCode;
        }
    }

    public class FSharpCodeGenerator : CodeGenerator
    {
        private static readonly Dictionary<string, string> FSharpTypes =
               new ()
        {
            { "float", "float32" },
            { "short", "int16" },
            { "long", "int64" },
        };

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
            this.UserHandlerTemplatePath = "@CSHARP_TEMPLATE_DIR/UserHandler.tfs";
            this.UserFunctionTemplatePath = "@CSHARP_TEMPLATE_DIR/UserFunction.tfs";
            this.PathToGeneratedCode = $"{Engine.PathToGeneratedCode}fsharp";
            if (!Directory.Exists(this.PathToGeneratedCode))
            {
                Directory.CreateDirectory(this.PathToGeneratedCode);
            }
        }

        /// <summary>
        /// Returns the indented code according to the provided number of space.
        /// </summary>
        public static string IndentCode(string code, uint spaceNumber)
        {
            string[] codeLines = code.Split("\n");
            string indentation = new (' ', (int)spaceNumber);
            var sb = new System.Text.StringBuilder();

            for (int i = 0, length = codeLines.Length; i < length; i++)
            {
                if (i < length - 1)
                {
                    sb.AppendLine($"{indentation}{codeLines[i]}");
                }
                else
                {
                    sb.Append($"{indentation}{codeLines[i]}");
                }
            }

            return sb.ToString();
        }

        /// <inheritdoc />
        public override string BuildHandlerObjects(uint[] inputTypes, uint outputType)
        {
            var sb = new System.Text.StringBuilder();
            foreach (string handler in FilterHandlers(inputTypes, outputType))
            {
                sb.AppendLine($"let {handler}Obj = new {handler}()");
            }

            return "// handler objects\n" + IndentCode(sb.ToString(), 8);
        }

        /// <inheritdoc />
        public override string BuildCreateArguments(string funcName, uint[] paramTypes, bool supportNullInput)
        {
            var sb = new System.Text.StringBuilder();
            if (!supportNullInput)
            {
                sb.AppendLine(GetNullableMessage(funcName));
            }

            sb.AppendLine($"// BEGIN create arguments for {funcName}");
            int argc = paramTypes.Length;
            for (int i = 0; i < argc; i++)
            {
                string handlerName = Engine.GetTypeHandler(paramTypes[i]);
                if (Engine.HandleArray.ContainsKey((OID)paramTypes[i]))
                {
                    if (supportNullInput || Engine.AlwaysNullable)
                    {
                        sb.AppendLine($"let argument_{i} = {handlerName}Obj.InputNullableArray(arguments.[{i}], isnull.[{i}])");
                    }
                    else
                    {
                        sb.AppendLine($"let argument_{i} = {handlerName}Obj.InputArray(arguments.[{i}])");
                    }
                }
                else
                {
                    if (supportNullInput || Engine.AlwaysNullable)
                    {
                        sb.AppendLine($"let argument_{i} = {handlerName}Obj.InputNullableValue(arguments.[{i}], isnull.[{i}])");
                    }
                    else
                    {
                        sb.AppendLine($"let argument_{i} = {handlerName}Obj.InputValue(arguments.[{i}])");
                    }
                }
            }

            sb.Append($"// END create arguments for {funcName}");
            return "\n" + IndentCode(sb.ToString(), 8);
        }

        /// <inheritdoc />
        public override string BuildFunctionCall(string funcName, uint returnTypeId, string[] dotnetTypes, bool supportNullInput, string className = "UserFunction")
        {
            var sb = new System.Text.StringBuilder();
            if ((OID)returnTypeId != OID.VOIDOID)
            {
                sb.Append("let result = ");
            }

            sb.Append($"{className}.{funcName}");
            for (int i = 0, argc = dotnetTypes.Length; i < argc; i++)
            {
                sb.Append($" argument_{i}");
            }

            return "//Calling user function\n" + IndentCode(sb.ToString(), 8);
        }

        /// <inheritdoc />
        public override string BuildCallSetResult(uint returnTypeId)
        {
            var sb = new System.Text.StringBuilder();
            _ = Engine.HandleArray.ContainsKey((OID)returnTypeId) ? "Array" : Engine.OidTypes[(OID)returnTypeId];

            if ((OID)returnTypeId == OID.VOIDOID)
            {
                sb.AppendLine("Engine.pldotnet_SetDatumResult(new IntPtr(0), false, output);");
            }
            else
            {
                if (Engine.HandleArray.ContainsKey((OID)returnTypeId))
                {
                    sb.AppendLine($"let resultDatum = {Engine.GetTypeHandler(returnTypeId)}Obj.OutputNullableArray(result)");
                }
                else
                {
                    sb.AppendLine($"let resultDatum = {Engine.GetTypeHandler(returnTypeId)}Obj.OutputNullableValue(result)");
                }

                sb.AppendLine("Engine.pldotnet_SetDatumResult(resultDatum, false, output);");
            }

            return "// Create PostgreSQL datum\n" + IndentCode(sb.ToString(), 8);
        }

        /// <inheritdoc />
        public override string BuildUserFunction(string funcName, string funcBody, uint returnTypeId, string[] paramNames, string[] dotnetTypes, bool supportNullInput)
        {
            var sb = new System.Text.StringBuilder();
            string type = Engine.HandleArray.ContainsKey((OID)returnTypeId) ? "Array" : Engine.OidTypes[(OID)returnTypeId];
            string returnType = FSharpTypes.ContainsKey(type) ? FSharpTypes[type] : type;

            sb.Append($"static member {funcName}");

            for (int i = 0, length = paramNames.Length; i < length; i++)
            {
                if (supportNullInput && (!ClassTypes.Contains(dotnetTypes[i])))
                {
                    sb.Append($" ({paramNames[i]}: Nullable<{dotnetTypes[i]}>)");
                }
                else
                {
                    sb.Append($" ({paramNames[i]}: {dotnetTypes[i]})");
                }
            }

            if (ClassTypes.Contains(returnType))
            {
                sb.Append($" : {returnType} = {IndentCode(funcBody, 8)}");
            }
            else if (returnType == "void")
            {
                sb.Append($" = {IndentCode(funcBody, 8)}");
            }
            else
            {
                sb.Append($" : Nullable<{returnType}> = {IndentCode(funcBody, 8)}");
            }

            return sb.ToString();
        }

        /// <inheritdoc />
        public override string[] GetDotNetTypes(uint[] paramTypes)
        {
            string[] dotnetTypes = new string[paramTypes.Length];
            for (int i = 0, length = paramTypes.Length; i < length; i++)
            {
                string type = Engine.HandleArray.ContainsKey((OID)paramTypes[i]) ? "Array" : Engine.OidTypes[(OID)paramTypes[i]];
                dotnetTypes[i] = FSharpTypes.ContainsKey(type) ? FSharpTypes[type] : type;
            }

            return dotnetTypes;
        }

        /// <inheritdoc />
        public override string FormatAndSaveGeneratedCode(string sourceCode, string fileName)
        {
            File.WriteAllText($"{this.PathToGeneratedCode}/{fileName}.fs", sourceCode, Encoding.UTF8);
            PrintSourceCode(sourceCode);
            return sourceCode;
        }
    }
}