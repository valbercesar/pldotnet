using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using PlDotNET.Handler;

namespace PlDotNET
{
    public abstract class CodeGenerator
    {
        public string UserHandlerTemplatePath;

        public string UserFunctionTemplatePath;

        public DotNETLanguage Language;

        public string BuildUserHandlerSourceCode(string funcName, uint returnTypeId, string[] paramNames, uint[] paramTypes, string funcBody, bool supportNullInput)
        {
            // Check if the file exists
            if (!File.Exists(UserHandlerTemplatePath))
            {
                throw new SystemException($"Template file '{UserHandlerTemplatePath}' not found");
            }

            bool providedAssembly = funcBody.Contains(".dll");
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

            string[] dotnetTypes = GetDotNetTypes(paramTypes);

            string sourceCode = File.ReadAllText(UserHandlerTemplatePath);
            sourceCode = sourceCode.Replace("// $handler_objects$", BuildHandlerObjects(paramTypes, returnTypeId));
            sourceCode = sourceCode.Replace("// $create_arguments", BuildCreateArguments(funcName, paramTypes, supportNullInput));
            sourceCode = sourceCode.Replace("// $user_function_call$", BuildFunctionCall(funcName, returnTypeId, dotnetTypes, supportNullInput, className));
            sourceCode = sourceCode.Replace("// $call_set_result$", BuildCallSetResult(returnTypeId));

            if (Language == DotNETLanguage.FSharp)
                sourceCode = sourceCode.Replace("// $user_function_declaration$", BuildUserFunction(funcName, funcBody, returnTypeId, paramNames, dotnetTypes, supportNullInput));

            if (providedAssembly)
                sourceCode = sourceCode.Replace("// $user_namespace$", $"using {nampespace};\n");

            return sourceCode;
        }

        public string BuildUserFunctionSourceCode(string funcName, uint returnTypeId, string[] paramNames, uint[] paramTypes, string funcBody, bool supportNullInput)
        {
            if (funcBody.Contains(".dll"))
                return funcBody;

            if (Language == DotNETLanguage.FSharp)
                return "";

            if (!File.Exists(UserFunctionTemplatePath))
            {
                string msg = $"Template file '{UserFunctionTemplatePath}' not found";
                throw new SystemException(msg);
            }
            string[] dotnetTypes = GetDotNetTypes(paramTypes);
            string userFunctionCode = File.ReadAllText(UserFunctionTemplatePath);
            userFunctionCode = userFunctionCode.Replace("// $user_function_declaration$", BuildUserFunction(funcName, funcBody, returnTypeId, paramNames, dotnetTypes, supportNullInput));
            return userFunctionCode;
        }

        public List<string> FilterHandlers(uint[] inputTypes, uint outputType)
        {
            List<string> allHandlers = new List<string>();

            if ((OID)outputType != OID.VOIDOID)
                allHandlers.Add(Engine.GetTypeHandler(outputType));

            for (int i = 0; i < inputTypes.Length; i++)
                allHandlers.Add(Engine.GetTypeHandler(inputTypes[i]));

            return allHandlers.Distinct().ToList();
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

        public abstract string[] GetDotNetTypes(uint[] paramTypes);

        /// <summary>
        /// This function creates user function.
        /// </summary>
        public abstract string BuildUserFunction(string funcName, string funcBody, uint returnTypeId, string[] paramNames, string[] dotnetTypes, bool supportNullInput);
    }

    public class CSharpCodeGenerator : CodeGenerator
    {
        public CSharpCodeGenerator()
        {
            this.UserHandlerTemplatePath = "@CSHARP_TEMPLATE_DIR/UserHandler.tcs";
            this.UserFunctionTemplatePath = "@CSHARP_TEMPLATE_DIR/UserFunction.tcs";
            this.Language = DotNETLanguage.CSharp;
        }

        /// <inheritdoc />
        public override string BuildHandlerObjects(uint[] inputTypes, uint outputType)
        {
            var sb = new System.Text.StringBuilder();
            foreach (string handler in FilterHandlers(inputTypes, outputType))
                sb.AppendLine($"public static {handler} {handler}Obj = new {handler}();");
            return sb.ToString();
        }

        /// <inheritdoc />
        public override string BuildCreateArguments(string funcName, uint[] paramTypes, bool supportNullInput)
        {
            var sb = new System.Text.StringBuilder();
            if (!supportNullInput)
            {
                sb.AppendLine($"// As the SQL function named {funcName} is `STRICT` or `RETURNS NULL ON NULL INPUT`,");
                sb.AppendLine("// `PL.NET` doesn't check whether any argument datum is null.");
                sb.AppendLine("// You can also set true for the `Engine.AlwaysNullable` variable");
                sb.AppendLine("// to always check whether the datum is null.\n");
            }
            sb.AppendLine($"// BEGIN create arguments for {funcName}");
            int argc = paramTypes.Length;
            for (int i = 0; i < argc; i++)
            {
                string handler = Engine.GetTypeHandler(paramTypes[i]);
                if (Engine.HandleArray.ContainsKey((OID)paramTypes[i]))
                {
                    if (supportNullInput || Engine.AlwaysNullable)
                        sb.AppendLine($"var argument_{i} = {handler}Obj.InputNullableArray(arguments[{i}], isnull[{i}]);");
                    else
                        sb.AppendLine($"var argument_{i} = {handler}Obj.InputArray(arguments[{i}]);");
                }
                else
                {
                    if (supportNullInput || Engine.AlwaysNullable)
                        sb.AppendLine($"var argument_{i} = {handler}Obj.InputNullableValue(arguments[{i}], isnull[{i}]);");
                    else
                        sb.AppendLine($"var argument_{i} = {handler}Obj.InputValue(arguments[{i}]);");
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
                sb.AppendLine("var result = ");
            sb.Append($"{className}.{funcName}(");
            string aux = (supportNullInput || Engine.AlwaysNullable) ? "?" : "";
            for (int i = 0, argc = dotnetTypes.Length; i < argc; i++)
            {
                sb.Append($"({dotnetTypes[i]}{aux}) argument_{i}");
                if (i < argc - 1)
                    sb.Append(", ");
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
                    sb.AppendLine($"var resultDatum = {Engine.GetTypeHandler(returnTypeId)}Obj.OutputNullableArray(result);");
                else
                    sb.AppendLine($"var resultDatum = {Engine.GetTypeHandler(returnTypeId)}Obj.OutputNullableValue(result);");

                sb.AppendLine("Engine.pldotnet_SetDatumResult(resultDatum, result == null, output);");
            }
            return sb.ToString();
        }

        /// <inheritdoc />
        public override string BuildUserFunction(string funcName, string funcBody, uint returnTypeId, string[] paramNames, string[] dotnetTypes, bool supportNullInput)
        {
            var sb = new System.Text.StringBuilder();
            string returnType = Engine.HandleArray.ContainsKey((OID)returnTypeId) ? "Array" : Engine.OidTypes[(OID)returnTypeId];
            string nullAbleOutput = returnType == "void" ? "" : "?";
            string aux = (supportNullInput || Engine.AlwaysNullable) ? "?" : "";

            sb.Append($"public static {returnType}{nullAbleOutput} {funcName}(");
            for (int i = 0, length = paramNames.Length; i < length; i++)
            {
                sb.Append($"{dotnetTypes[i]}{aux} {paramNames[i]}");
                if (i < length - 1)
                    sb.Append(", ");
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

    }

    public class FSharpCodeGenerator : CodeGenerator
    {
        public FSharpCodeGenerator()
        {
            this.Language = DotNETLanguage.FSharp;
            this.UserHandlerTemplatePath = "@CSHARP_TEMPLATE_DIR/UserHandler.tfs";
            this.UserFunctionTemplatePath = "@CSHARP_TEMPLATE_DIR/UserFunction.tfs";
        }

        public static Dictionary<string, string> FSharpTypes =
               new Dictionary<string, string>()
        {
            {"float", "float32"},
            {"short", "int16"},
            {"long", "int64"}
        };

        public static List<string> ClassTypes =
               new List<string>()
        {
            "Array",
            "byte[]",
            "BitArray",
            "string",
            "PhysicalAddress",
        };

        /// <inheritdoc />
        public override string BuildHandlerObjects(uint[] inputTypes, uint outputType)
        {
            var sb = new System.Text.StringBuilder();
            foreach (string handler in FilterHandlers(inputTypes, outputType))
                sb.AppendLine($"let {handler}Obj = new {handler}()");
            return "// handler objects\n" + IndentCode(sb.ToString(), 8);
        }

        /// <inheritdoc />
        public override string BuildCreateArguments(string funcName, uint[] paramTypes, bool supportNullInput)
        {
            var sb = new System.Text.StringBuilder();
            if (!supportNullInput)
            {
                sb.AppendLine($"// As the SQL function named {funcName} is `STRICT` or `RETURNS NULL ON NULL INPUT`,");
                sb.AppendLine("// `PL.NET` doesn't check whether any argument datum is null.");
                sb.AppendLine("// You can also set true for the `Engine.AlwaysNullable` variable");
                sb.AppendLine("// to always check whether the datum is null.\n");
            }
            sb.AppendLine($"// BEGIN create arguments for {funcName}");
            int argc = paramTypes.Length;
            for (int i = 0; i < argc; i++)
            {
                string handlerName = Engine.GetTypeHandler(paramTypes[i]);
                if (Engine.HandleArray.ContainsKey((OID)paramTypes[i]))
                {
                    if (supportNullInput || Engine.AlwaysNullable)
                        sb.AppendLine($"let argument_{i} = {handlerName}Obj.InputNullableArray(arguments.[{i}], isnull.[{i}])");
                    else
                        sb.AppendLine($"let argument_{i} = {handlerName}Obj.InputArray(arguments.[{i}])");
                }
                else
                {
                    if (supportNullInput || Engine.AlwaysNullable)
                        sb.AppendLine($"let argument_{i} = {handlerName}Obj.InputNullableValue(arguments.[{i}], isnull.[{i}])");
                    else
                        sb.AppendLine($"let argument_{i} = {handlerName}Obj.InputValue(arguments.[{i}])");
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
                sb.Append("let result = ");
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
            string type = Engine.HandleArray.ContainsKey((OID)returnTypeId) ? "Array" : Engine.OidTypes[(OID)returnTypeId];
            string returnType = FSharpTypes.ContainsKey(type) ? FSharpTypes[type] : type;

            if ((OID)returnTypeId == OID.VOIDOID)
            {
                sb.AppendLine("Engine.pldotnet_SetDatumResult(new IntPtr(0), false, output);");
            }
            else
            {
                if (Engine.HandleArray.ContainsKey((OID)returnTypeId))
                    sb.AppendLine($"let resultDatum = {Engine.GetTypeHandler(returnTypeId)}Obj.OutputNullableArray(result)");
                else
                    sb.AppendLine($"let resultDatum = {Engine.GetTypeHandler(returnTypeId)}Obj.OutputNullableValue(result)");

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
                    sb.Append($" ({paramNames[i]}: Nullable<{dotnetTypes[i]}>)");
                else
                    sb.Append($" ({paramNames[i]}: {dotnetTypes[i]})");
            }

            if (ClassTypes.Contains(returnType))
                sb.Append($" : {returnType} = {IndentCode(funcBody, 8)}");
            else if (returnType == "void")
                sb.Append($" = {IndentCode(funcBody, 8)}");
            else
                sb.Append($" : Nullable<{returnType}> = {IndentCode(funcBody, 8)}");

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

        public string IndentCode(string code, uint spaceNumber)
        {
            string[] codeLines = code.Split("\n");
            string indentation = new String(' ', (int)spaceNumber);
            string indentCode = "";
            for (int i = 0; i < codeLines.Length; i++)
                indentCode += (indentation + codeLines[i] + '\n');
            return indentCode;
        }
    }
}