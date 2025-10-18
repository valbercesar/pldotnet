using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlDotNET
{
    // Static class responsible for generating SQL test case files from C# test methods
    public static class TestCaseGenerator
    {
        // Path where the generated SQL files will be saved, with a placeholder for the file name
        private static readonly string PathToSaveGeneratedFile = "tests/npgsql/sql/file-name.sql";

        // List of strings to identify methods or code snippets to skip during test generation
        private static readonly List<string> KeyToIgnore = new()
        {
            "PgPostmasterMock.Start",
            "ManualResetEvent(",
            "TimeSpan.From",
            "Thread.Sleep(",
        };

        // List of file/class names to exclude from test case generation
        private static readonly List<string> FilesToIgnore = new()
        {
            "PoolTests",
            "TestUtil",
        };

        // Dictionary mapping constructor parameter types and names to their default values
        private static readonly Dictionary<string, string> ConstructorArguments = new()
        {
            { "MultiplexingMode multiplexingMode", "MultiplexingMode.NonMultiplexing" },
            { "CommandBehavior behavior", "CommandBehavior.Default" },
            { "CompatMode compatMode", "CompatMode.OnePass" },
            { "SyncOrAsync syncOrAsync", "SyncOrAsync.Sync" },
            { "bool disableDateTimeInfinityConversions", "true" },
            { "NpgsqlDbType npgsqlDbType", "NpgsqlDbType.Json" }
        };

        // Dictionary mapping method parameter types and names to their default values
        private static readonly Dictionary<string, string> FunctionArguments = new()
        {
            { "PrepareOrNot prepare", "PrepareOrNot.NotPrepared" },
            { "bool async", "true" },
            { "bool withErrorBarriers", "false" },
            { "bool dispose", "true" },
            { "bool enabled", "true" },
            { "PooledOrNot pooled", "PooledOrNot.Unpooled" },
            { "bool pooling", "false" },
            { "bool openFromClose", "false" },
            { "NpgsqlRange<DateTime> input", "new NpgsqlRange<DateTime>(new DateTime(2022, 1, 1, 12, 30, 30), true, false, new DateTime(2022, 12, 25, 17, 30, 30), false, false)" },
            { "DateTimeKind kind", "DateTimeKind.Unspecified" },
            { "int count", "5" }, // Specific to NpgsqlParameterCollectionTests, requires 5 or 3
            { "bool isAsync", "false" },
        };

        // Formats C# code by parsing it into a syntax tree and normalizing whitespace
        public static string FormatGeneratedCode(string sourceCode)
        {
            // Parse the input code into a syntax tree
            SyntaxTree userTree = SyntaxFactory.ParseSyntaxTree(sourceCode);
            // Normalize whitespace for consistent formatting
            SyntaxNode node = userTree.GetRoot().NormalizeWhitespace();
            // Return the formatted code as a string
            sourceCode = node.ToFullString();
            return sourceCode;
        }

        // Generates the C# body for a test method to be embedded in an SQL procedure
        private static string CreateCSharpBody(string className, string methodName, List<string> functionArguments, string constructorArguments, string comment, bool isAsync)
        {
            // Add ".Wait()" for async methods to ensure synchronous execution in SQL
            string waitCall = isAsync ? ".Wait()" : string.Empty;

            // Use StringBuilder to construct the C# code block
            var sb = new StringBuilder();
            // Add method documentation with class and method name
            sb.AppendLine($"/// {className}.{methodName}:");
            // Append the provided comment (e.g., source file line number)
            sb.AppendLine(comment);
            // Start a try-catch block to handle errors during execution
            sb.AppendLine("try {");
            // Instantiate the test class with the provided constructor arguments
            sb.AppendLine($"var test = new {className}({constructorArguments});");

            // If the method has no arguments, call it directly
            if (functionArguments.Count == 0)
            {
                sb.AppendLine($"test.{methodName}(){waitCall};");
            }
            // Otherwise, generate a call for each set of function arguments
            else
            {
                foreach (var arg in functionArguments)
                {
                    sb.AppendLine($"test.{methodName}({arg}){waitCall};");
                }
            }

            // Log success message using Elog.Info
            sb.AppendLine("Elog.Info(\"Working fine on pldotnet 👍\");} catch (Exception e) {");
            // Log error message with exception details
            sb.AppendLine("Elog.Info(\"Fail on C#...\\n\"+ e.ToString()); }");
            // Format the generated C# code
            return FormatGeneratedCode(sb.ToString());
        }

        // Generates an SQL procedure that embeds the C# test method call
        private static string CreateSQLFunction(string className, string methodName, List<string> functionArguments, string constructorArguments, string comment, bool isAsync)
        {
            // Use StringBuilder to construct the SQL procedure
            var sb = new StringBuilder();
            // Define the SQL procedure with the class and method name
            sb.AppendLine($"CREATE OR REPLACE PROCEDURE {className}_{methodName}() AS $$");
            // Embed the C# body generated by CreateCSharpBody
            sb.AppendLine(CreateCSharpBody(className, methodName, functionArguments, constructorArguments, comment, isAsync));
            // Specify the language as plcsharp (PL/pgSQL with C# support)
            sb.AppendLine("$$ LANGUAGE plcsharp;");
            // Call the generated procedure
            sb.AppendLine($"CALL {className}_{methodName}();\n");

            return sb.ToString();
        }

        // Generates test cases for a given C# file
        private static void GenerateTestCases(string filePath, string fileName)
        {
            // Skip generation if the file is in the ignore list
            if (FilesToIgnore.Contains(fileName))
            {
                Console.WriteLine($"Skipping the tests in the {fileName} class...");
                return;
            }

            // Default comment template with placeholder for line number
            string comment = $"/// Method defined on the line XXX of the {filePath} file.";
            // StringBuilder to accumulate SQL content
            var sqlFile = new StringBuilder();
            // Parse the C# file into a syntax tree
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath));
            var root = tree.GetRoot();

            // Find all public class declarations in the file
            var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Where(c => c.Modifiers.Any(SyntaxKind.PublicKeyword));
            foreach (var classDeclaration in classDeclarations)
            {
                // Get the class name
                string className = classDeclaration.Identifier.ValueText;
                // Process only the class matching the file name
                if (className != fileName)
                {
                    continue;
                }

                Console.WriteLine($"Generating SQL files for the functions in the {className} class...");

                // Get all constructors in the class
                var constructors = classDeclaration.Members.OfType<ConstructorDeclarationSyntax>();

                // Collect constructor arguments from the first constructor
                List<string> arguments = new();
                try
                {
                    foreach (var constructor in constructors)
                    {
                        foreach (var p in constructor.ParameterList.Parameters)
                        {
                            // Map constructor parameters to default values from ConstructorArguments
                            arguments.Add(ConstructorArguments[p.Type + " " + p.Identifier.ValueText]);
                        }
                        // Use only the first constructor
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Fail to generate the tests in the {className} class.");
                }

                // Join constructor arguments into a comma-separated string
                string constructorArguments = arguments.Count > 0 ? string.Join(", ", arguments) : string.Empty;

                // Filter public methods that return Task or void
                var testMethods = classDeclaration.Members
                                   .OfType<MethodDeclarationSyntax>()
                                   .Where(m => (m.ReturnType.ToString() == "Task" || m.ReturnType.ToString() == "void") &&
                                               m.Modifiers.Any(x => x.ValueText == "public"));

                foreach (var testMethod in testMethods)
                {
                    // Check if the method is async
                    bool isAsync = testMethod.Modifiers.Any(x => x.ValueText == "async");

                    try
                    {
                        var methodBody = testMethod.Body;

                        // Skip methods containing any ignored keywords
                        if (methodBody != null)
                        {
                            bool toIgnore = false;
                            foreach (var key in KeyToIgnore)
                            {
                                if (methodBody.ToFullString().Contains(key))
                                {
                                    toIgnore = true;
                                    break;
                                }
                            }

                            if (toIgnore)
                            {
                                Console.WriteLine($"Skipping test: {className}.{testMethod.Identifier.ValueText}");
                                continue;
                            }
                        }

                        // Get method parameters
                        var parameters = testMethod.ParameterList.Parameters;

                        // Find TestCase attributes to extract argument values
                        var testCaseAttributes = testMethod.AttributeLists
                            .SelectMany(al => al.Attributes)
                            .Where(a => a.Name.ToString() == "TestCase");

                        // Collect function arguments from TestCase attributes
                        List<string> functionArguments = new();
                        foreach (var testCaseAttribute in testCaseAttributes)
                        {
                            List<string> args = new();
                            var caseArgs = testCaseAttribute.ArgumentList.Arguments;
                            int cont = 0;
                            foreach (var caseArg in caseArgs)
                            {
                                string caseArgStr = caseArg.ToString();
                                // Exclude TestName arguments
                                if (!caseArgStr.Contains("TestName"))
                                {
                                    args.Add(caseArgStr);
                                    cont++;
                                    // Stop adding arguments if exceeding parameter count
                                    if (cont > parameters.Count)
                                    {
                                        break;
                                    }
                                }
                            }
                            functionArguments.Add(string.Join(", ", args));
                        }

                        // If no TestCase attributes, use default arguments from FunctionArguments
                        if (functionArguments.Count == 0)
                        {
                            if (parameters.Count > 0)
                            {
                                List<string> args = new();
                                foreach (var p in parameters)
                                {
                                    args.Add(FunctionArguments[p.Type + " " + p.Identifier.ValueText]);
                                }
                                functionArguments.Add(string.Join(", ", args));
                            }
                        }

                        // Update comment with the method's line number
                        string commentMethod = comment.Replace("XXX", (testMethod.GetLocation().GetLineSpan().StartLinePosition.Line + 2 + testCaseAttributes.Count()).ToString());

                        // Get the method name
                        string methodName = testMethod.Identifier.ValueText;
                        // Write the generated SQL procedure to a file
                        File.WriteAllText(PathToSaveGeneratedFile.Replace("file-name", $"{className}_{methodName}"), CreateSQLFunction(className, methodName, functionArguments, constructorArguments, commentMethod, isAsync), Encoding.UTF8);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Fail to generate test: {className}.{testMethod.Identifier.ValueText}");
                    }
                }
            }
        }

        // Entry point of the program, processes each input file
        private static void Main(string[] args)
        {
            // Iterate through each file path provided as a command-line argument
            foreach (var file in args)
            {
                // Generate test cases for the file, using its name without extension
                GenerateTestCases(file, Path.GetFileNameWithoutExtension(file));
            }
        }
    }
}