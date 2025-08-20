using DotNetEnv;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using Xunit;

[Collection("Sequential")]
public partial class PlDotNetTest
{
    protected SqlFunctionInfo? FunctionInfo;

    readonly string DatabaseConnectionString =
        Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") + ";Include Error Detail=true"
        ?? throw new InvalidOperationException("DATABASE_CONNECTION_STRING not set.");

    int TestCount = 0;

    public enum LanguageType
    {
        PlcSharp,
        PlfSharp,
        PlPython,
        PlR,
        PlPgSQL,
        PlV8,
        PlTCL,
        PlLua,
        PlPerl
    }

    public enum SqlTestType
    {
        Function,
        Procedure,
        DoBlock
    }

    public Dictionary<SqlTestType, string> TestTypeMap = new()
    {
        { SqlTestType.Function, "FUNCTION" },
        { SqlTestType.Procedure, "PROCEDURE" },
        { SqlTestType.DoBlock, "DO BLOCK" }
    };

    public class SqlFunctionInfo
    {
        // New properties for dynamic SQL construction

        public string CteStatement { get; set; } = string.Empty;
        public string CustomAssertion { get; set; } = string.Empty; // For complex assertion logic
        public string QuerySuffix { get; set; } = string.Empty; // For additional WHERE, LIMIT, etc.
        public SqlTestType TestType { get; set; } = SqlTestType.Function;
        public string Name { get; set; } = string.Empty;
        public List<FunctionArgument> Arguments { get; set; } = [];
        public string ReturnType { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public LanguageType Language { get; set; }
        public bool IsStrict { get; set; }
        public string FunctionName { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;
        public string FeatureName { get; set; } = string.Empty;
        public string InputStr { get; set; } = string.Empty;
        public string ExpectedResult { get; set; } = string.Empty;
        public string CastFunctionAs { get; set; } = string.Empty;

        public int? TestId { get; set; }

        public bool FunctionCreatedSuccessfully { get; set; } = false;

        public bool TestInsertedSuccessfully { get; set; } = false;

        public string SqlFunctionDefinition { get; set; } = string.Empty;

        public string SqlFunctionCall { get; set; } = string.Empty;

        public List<string> SqlSettings { get; set; } = [];

        public string LanguageString
        {
            get
            {
                return Language switch
                {
                    LanguageType.PlcSharp => "plcsharp",
                    LanguageType.PlfSharp => "plfsharp",
                    LanguageType.PlPython => "plpython3u",
                    LanguageType.PlR => "plr",
                    LanguageType.PlPgSQL => "plpgsql",
                    LanguageType.PlV8 => "plv8",
                    LanguageType.PlTCL => "pltcl",
                    LanguageType.PlLua => "pllua",
                    LanguageType.PlPerl => "plperl",
                    _ => throw new System.NotImplementedException()
                };
            }
        }

        public SqlFunctionInfo() { }
    }

    public class FunctionArgument(string name, string type)
    {
        public string Name { get; set; } = name;
        public string Type { get; set; } = type;
    }

    public class TestQueryParameters
    {
        public List<(string Name, string Query)> Ctes { get; set; } =
            [];
        public string TestCategory { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;
        public string Assertion { get; set; } = string.Empty;
        public string FinalSelect { get; set; } = string.Empty;
    }

    /// <summary>
    /// Constructs and returns the SQL definition of a function based on the provided SqlFunctionInfo.
    /// </summary>
    /// <param name="functionInfo">The information about the SQL function.</param>
    /// <returns>A string representing the SQL definition of the function.</returns>
    /// <remarks>
    /// The method uses the properties of the SqlFunctionInfo object to create a SQL CREATE OR REPLACE FUNCTION statement.
    /// If the IsStrict property of the SqlFunctionInfo object is true, the "STRICT" keyword is added to the SQL definition.
    /// </remarks>
    public virtual string GetFunctionDefinition(SqlFunctionInfo functionInfo)
    {
        var arguments = string.Join(
            ", ",
            functionInfo.Arguments.Select(arg => $"{arg.Name} {arg.Type}")
        );

        string methodKeyword =
            functionInfo.TestType == SqlTestType.Function ? "FUNCTION" : "PROCEDURE";

        string strictKeyword = functionInfo.IsStrict ? "STRICT" : "";

        string settings = functionInfo.SqlSettings != null && functionInfo.SqlSettings.Count != 0
            ? Environment.NewLine + string.Join(Environment.NewLine, functionInfo.SqlSettings.Select(s => $"SET {s}"))
            : string.Empty;

        // Conditionally build the returnTypeString
        string returnTypeString = string.IsNullOrEmpty(functionInfo.ReturnType)
            ? string.Empty
            : $"RETURNS {functionInfo.ReturnType}";

        return $@"CREATE OR REPLACE {methodKeyword} {functionInfo.Name}({arguments})
{returnTypeString} AS $$
    {functionInfo.Body}
$$ LANGUAGE {functionInfo.LanguageString} {strictKeyword}{settings};";
    }

    /// <summary>
    /// Constructs and returns the SQL call statement for a function or procedure based on the provided SqlFunctionInfo.
    /// </summary>
    /// <param name="functionInfo">The information about the SQL function or procedure.</param>
    /// <param name="forceCte">Whether to force the use of CTE strategy for result validation.</param>
    /// <returns>A string representing the SQL call statement for the function or procedure.</returns>
    public virtual string GetFunctionCall(SqlFunctionInfo functionInfo, bool forceCte = false)
    {
        bool isProcedure = functionInfo.TestType == SqlTestType.Procedure;

        // Build the CALL statement for procedures
        string callSql = isProcedure
            ? $"CALL {functionInfo.Name}({functionInfo.InputStr});"
            : string.Empty;

        if (isProcedure && !forceCte)
        {
            // In case the procedure doesn't use a CTE to validate the assertion, returning the simple CALL statement
            return callSql;
        }

        // Build the SQL statement to check the test result
        string verificationSql = (!isProcedure || forceCte)
            ? TestResultStrategyFactory.GetStrategy(functionInfo, forceCte).BuildInsertSql(functionInfo)
            : string.Empty;

        if (!isProcedure)
        {
            return verificationSql;
        }

        // Create the SQL statement to call procedures and verify an assertion using CTE
        return $"{callSql}\n{verificationSql}".Trim();
    }

    /// <summary>
    /// Defines the contract for test result strategies that determine how to build SQL insert statements for test results.
    /// </summary>
    public interface ITestResultStrategy
    {
        /// <summary>
        /// Determines if the strategy should be used for the given function information.
        /// </summary>
        /// <param name="functionInfo">The function information to evaluate.</param>
        /// <returns>True if this strategy applies to the given function; otherwise, false.</returns>
        bool AppliesTo(SqlFunctionInfo functionInfo);

        /// <summary>
        /// Returns the SQL code to insert the test result.
        /// </summary>
        /// <param name="functionInfo">The function information to build the SQL for.</param>
        /// <returns>A string containing the SQL INSERT statement.</returns>
        string BuildInsertSql(SqlFunctionInfo functionInfo);
    }

    /// <summary>
    /// Default implementation of ITestResultStrategy that handles standard function test result insertion.
    /// </summary>
    public class DefaultTestResultStrategy : ITestResultStrategy
    {
        /// <summary>
        /// Determines if this strategy applies to the given function information.
        /// This default strategy applies to all functions.
        /// </summary>
        /// <param name="functionInfo">The function information to evaluate.</param>
        /// <returns>Always returns true as this is the default strategy.</returns>
        public bool AppliesTo(SqlFunctionInfo functionInfo)
        {
            return true;
        }

        /// <summary>
        /// Builds an SQL INSERT statement for inserting test results using standard function call validation.
        /// </summary>
        /// <param name="functionInfo">The function information to build the SQL for.</param>
        /// <returns>A string containing the SQL INSERT statement with result validation.</returns>
        public string BuildInsertSql(SqlFunctionInfo functionInfo)
        {
            string functionCall = $"{functionInfo.Name}({functionInfo.InputStr})";

            string cast = !string.IsNullOrWhiteSpace(functionInfo.CastFunctionAs)
                ? $"CAST({functionCall} AS {functionInfo.CastFunctionAs})"
                : functionCall;

            string nullComparison =
                functionInfo.ExpectedResult == "= null"
                    ? "IS NULL"
                    : $"{cast} {functionInfo.ExpectedResult}";

            if (string.IsNullOrWhiteSpace(functionInfo.ExpectedResult))
            {
                nullComparison = "IS NOT NULL";
            }

            string sqlCode =
                $@"INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
                            VALUES ('{functionInfo.FeatureName}', '{functionInfo.TestName}', {nullComparison})
                            RETURNING id;";

            return sqlCode;
        }
    }

    /// <summary>
    /// Implementation of ITestResultStrategy that handles test result insertion using Common Table Expressions (CTEs).
    /// </summary>
    public partial class CteTestResultStrategy : ITestResultStrategy
    {
        /// <summary>
        /// Determines if the strategy applies to the given SQL function based on the presence of a CTE statement.
        /// </summary>
        /// <param name="functionInfo">The function information to evaluate.</param>
        /// <returns>True if the CteStatement property is not null or whitespace; otherwise, false.</returns>
        public bool AppliesTo(SqlFunctionInfo functionInfo)
        {
            return !string.IsNullOrWhiteSpace(functionInfo.CteStatement);
        }

        /// <summary>
        /// Builds an SQL INSERT statement using the information provided in the SqlFunctionInfo parameter.
        /// </summary>
        /// <param name="functionInfo">The function information to build the SQL for.</param>
        /// <returns>A string containing the SQL statement with CTE and INSERT operation.</returns>
        public string BuildInsertSql(SqlFunctionInfo functionInfo)
        {
            // Extract the name of the CTE from its SQL statement to use in the final query
            string cteName = ExtractCteName(functionInfo.CteStatement)!;

            // Construct an SQL statement that includes the CTE statement and an INSERT INTO operation
            string sqlCode =
                $@"
{functionInfo.CteStatement}
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT '{functionInfo.FeatureName}', '{functionInfo.TestName}', {functionInfo.CustomAssertion} FROM {cteName} {functionInfo.QuerySuffix} RETURNING id;";

            return sqlCode;
        }

        /// <summary>
        /// A private helper method to extract the name of the CTE from its SQL statement.
        /// </summary>
        /// <param name="cteStatement">The CTE statement to parse.</param>
        /// <returns>The CTE name if found; otherwise, null.</returns>
        private static string? ExtractCteName(string cteStatement)
        {
            // Use a regular expression to find the CTE name in the statement
            var match = MyRegex().Match(cteStatement);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }

        [System.Text.RegularExpressions.GeneratedRegex(@"WITH\s+(\w+)\s+AS"
        )]
        private static partial System.Text.RegularExpressions.Regex MyRegex();
    }

    /// <summary>
    /// Implementation of ITestResultStrategy that handles test result insertion for JSON and XML data types.
    /// </summary>
    public class JsonOrXmlTestResultStrategy : ITestResultStrategy
    {
        /// <summary>
        /// Determines if the strategy applies to functions that return JSON or XML data types.
        /// </summary>
        /// <param name="functionInfo">The function information to evaluate.</param>
        /// <returns>True if the function returns JSON or XML types; otherwise, false.</returns>
        public bool AppliesTo(SqlFunctionInfo functionInfo)
        {
            return (
                    functionInfo.ReturnType != null
                    && (
                        functionInfo.ReturnType.Contains("JSON", StringComparison.OrdinalIgnoreCase)
                        || functionInfo.ReturnType.Contains("XML", StringComparison.OrdinalIgnoreCase
)
                    )
                )
                || (
                    functionInfo.ExpectedResult != null
                    && functionInfo.ExpectedResult.Contains("::JSON::TEXT", StringComparison.OrdinalIgnoreCase
)
                );
        }

        /// <summary>
        /// Builds an SQL INSERT statement for JSON or XML test result validation.
        /// </summary>
        /// <param name="functionInfo">The function information to build the SQL for.</param>
        /// <returns>A string containing the SQL INSERT statement with TEXT casting for comparison.</returns>
        public string BuildInsertSql(SqlFunctionInfo functionInfo)
        {
            string functionCall = $"{functionInfo.Name}({functionInfo.InputStr})::TEXT";
            string expectedResult = functionInfo.ExpectedResult + "::TEXT";

            string sqlCode =
                $@"   INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
            VALUES ('{functionInfo.FeatureName}', '{functionInfo.TestName}', {functionCall} {expectedResult})
            RETURNING id;";
            return sqlCode;
        }
    }

    /// <summary>
    /// Factory class for selecting the appropriate test result strategy based on function characteristics.
    /// </summary>
    public class TestResultStrategyFactory
    {
        private static readonly List<ITestResultStrategy> Strategies =
        [
            new CteTestResultStrategy(),
            new JsonOrXmlTestResultStrategy(),
            new DefaultTestResultStrategy()
        ];

        /// <summary>
        /// Gets the appropriate test result strategy for the given function information.
        /// </summary>
        /// <param name="functionInfo">The function information to evaluate.</param>
        /// <param name="forceCte">Whether to force the use of CTE strategy.</param>
        /// <returns>The most appropriate test result strategy for the given function.</returns>
        public static ITestResultStrategy GetStrategy(
            SqlFunctionInfo functionInfo,
            bool forceCte = false
        )
        {
            if (forceCte)
            {
                return Strategies[0];
            }
            return Strategies.FirstOrDefault(strategy => strategy.AppliesTo(functionInfo))
                ?? new DefaultTestResultStrategy();
        }
    }

    /// <summary>
    /// Executes the provided SQL command that is expected to return an integer ID.
    /// This is typically used with SQL INSERT commands that use the RETURNING clause
    /// to return the ID of the newly inserted row.
    /// </summary>
    /// <param name="sqlCode">The SQL code to execute.</param>
    /// <returns>The retrieved integer ID if the command executed successfully and returned a value,
    /// null otherwise.</returns>
    protected int? ExecuteSqlReturnId(string sqlCode)
    {
        StringBuilder messages = new();
        Exception exception = null!;

        try
        {
            using var connection = new NpgsqlConnection(DatabaseConnectionString);
            connection.Open();
            connection.Notice += (sender, e) =>
            {
                messages.AppendLine($"{e.Notice.MessageText}");
            };
            using var command = new NpgsqlCommand(sqlCode, connection);
            command.CommandType = CommandType.Text;
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return reader.GetInt32(0);
            }
            return null;
        }
        catch (Exception ex)
        {
            exception = ex;
            return null;
        }
        finally
        {
            LogTestExecutionDetails(exception, messages);
        }
    }

    /// <summary>
    /// Fetches the test result for the given function info based on its test ID.
    /// </summary>
    /// <param name="functionInfo">Information about the test whose result is to be fetched.</param>
    /// <returns>The test result if found; null otherwise.</returns>
    public bool? FetchTestResult(SqlFunctionInfo functionInfo)
    {
        if (!functionInfo.TestId.HasValue)
        {
            return null;
        }

        string sqlSelect =
            $@"
SELECT RESULT
FROM automated_test_results
WHERE id = {functionInfo.TestId.Value};";

        object? result = ExecuteSql(sqlSelect);

        if (result is bool booleanResult)
        {
            return booleanResult;
        }

        return null;
    }

    /// <summary>
    /// Logs the execution details of a test, including any exception and SQL messages.
    /// </summary>
    /// <param name="exception">The exception that occurred during the test execution.</param>
    /// <param name="elogMessages">The SQL messages generated during the test execution.</param>
    protected void LogTestExecutionDetails(Exception exception, StringBuilder elogMessages)
    {
        if (elogMessages.Length > 0 || exception != null)
        {
            Console.WriteLine($"[START SQL EXECUTION DETAILS - TEST {TestCount}]");
            if (exception != null)
            {
                Console.WriteLine($"SQL Error:\n{exception.Message}");
            }
            if (elogMessages.Length > 0)
            {
                Console.WriteLine($"SQL Messages:\n{elogMessages.ToString().Trim()}");
            }
            Console.WriteLine($"[END SQL EXECUTION DETAILS - TEST {TestCount}]");
        }
    }

    /// <summary>
    /// Executes the provided SQL command.
    /// It returns true for successful execution
    /// and false for failures.
    /// </summary>
    /// <param name="sqlCode">The SQL code to execute.</param>
    /// <returns>It returns true for successful execution,
    /// and false otherwise.</returns>
    protected bool ExecuteSql(string sqlCode)
    {
        StringBuilder messages = new();
        Exception exception = null!;

        try
        {
            using var connection = new NpgsqlConnection(DatabaseConnectionString);
            connection.Open();
            connection.Notice += (sender, e) =>
            {
                messages.AppendLine($"{e.Notice.MessageText}");
            };

            using var command = new NpgsqlCommand(sqlCode, connection);
            if (sqlCode.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    object value = reader.GetValue(0);
                    if (value is bool booleanValue)
                    {
                        return booleanValue;
                    }
                }
                return false;
            }
            else
            {
                command.ExecuteNonQuery();
                return true;
            }
        }
        catch (Exception ex)
        {
            exception = ex;
            return false;
        }
        finally
        {
            LogTestExecutionDetails(exception, messages);
        }
    }

    /// <summary>
    /// Executes a generic test for the provided SQL function, logs the test result, and asserts the success of each step.
    /// <para>This method performs the following actions and validations:</para>
    /// <para>1. Sets up the function information based on the provided parameters.</para>
    /// <para>2. Attempts to define the function in the database.</para>
    /// <para>3. Validates (via assertion) that the function was successfully created.</para>
    /// <para>4. Inserts the test result for the defined function into the database.</para>
    /// <para>5. Validates (via assertion) that the test result was successfully inserted.</para>
    /// <para>6. Fetches the test result from the database.</para>
    /// <para>7. Validates (via assertion) that a test result was retrieved.</para>
    /// <para>8. Validates (via assertion) that the retrieved test result matches the expected result.</para>
    /// </summary>
    /// <param name="featureName">The feature name associated with the test.</param>
    /// <param name="testName">The name of the SQL function to test.</param>
    /// <param name="input">Input string for the SQL function.</param>
    /// <param name="expectedResult">Expected result string for the SQL function.</param>
    public void RunGenericTest(
        string featureName,
        string testName,
        string input,
        string expectedResult
    )
    {
        RunTest(
            featureName: featureName,
            testName: testName,
            input: input,
            expectedResult: expectedResult
        );
    }

    /// <summary>
    /// Executes a test with CTE statement and custom assertion logic.
    /// </summary>
    /// <param name="featureName">The feature name associated with the test.</param>
    /// <param name="testName">The name of the test being executed.</param>
    /// <param name="cteStatement">The Common Table Expression statement to use.</param>
    /// <param name="customAssertion">The custom assertion logic for the test.</param>
    /// <param name="querySuffix">Additional query suffix (WHERE, LIMIT, etc.).</param>
    /// <param name="forceCte">Whether to force the use of CTE strategy.</param>
    /// <param name="input">Input parameters for the function call.</param>
    public void RunTestWithSuffix(
        string featureName,
        string testName,
        string cteStatement,
        string customAssertion,
        string querySuffix = null!,
        bool forceCte = false,
        string input = ""
    )
    {
        RunTest(
            featureName: featureName,
            testName: testName,
            cteStatement: cteStatement,
            customAssertion: customAssertion,
            querySuffix: querySuffix,
            forceCte: forceCte,
            input: input
        );
    }

    /// <summary>
    /// Executes a comprehensive test for the provided SQL function, including function creation,
    /// execution, result validation, and assertion verification.
    /// </summary>
    /// <param name="featureName">The feature name associated with the test.</param>
    /// <param name="testName">The name of the test being executed.</param>
    /// <param name="input">Input parameters for the function call.</param>
    /// <param name="expectedResult">The expected result for comparison.</param>
    /// <param name="cteStatement">The Common Table Expression statement to use.</param>
    /// <param name="customAssertion">The custom assertion logic for the test.</param>
    /// <param name="querySuffix">Additional query suffix (WHERE, LIMIT, etc.).</param>
    /// <param name="forceCte">Whether to force the use of CTE strategy.</param>
    public void RunTest(
        string featureName,
        string testName,
        string input = "",
        string expectedResult = "",
        string cteStatement = "",
        string customAssertion = "",
        string querySuffix = "",
        bool forceCte = false
    )
    {
        TestCount++;
        Console.WriteLine(
            $"[START TEST {TestCount}] Running test {testName} for feature {featureName}."
        );

        if (FunctionInfo == null)
        {
            Assert.Fail("FunctionInfo is null, test cannot proceed.");
            return;
        }

        // Set up the FunctionInfo object with the provided parameters
        FunctionInfo.TestName = testName;
        FunctionInfo.FeatureName = featureName;
        FunctionInfo.InputStr = input;
        FunctionInfo.ExpectedResult = expectedResult;
        FunctionInfo.CteStatement = cteStatement; // Updated to use the full CTE statement
        FunctionInfo.CustomAssertion = customAssertion;
        FunctionInfo.QuerySuffix = querySuffix;

        // Combine pieces of FunctionInfo to create SQL codes
        FunctionInfo.SqlFunctionDefinition = GetFunctionDefinition(FunctionInfo);
        FunctionInfo.SqlFunctionCall = GetFunctionCall(FunctionInfo, forceCte);
        Console.WriteLine(
            $"[START SQL FUNCTION - TEST {TestCount}]\n```sql\n{FunctionInfo.SqlFunctionDefinition}\n```\n[END SQL FUNCTION - TEST {TestCount}]\n"
        );
        Console.WriteLine(
            $"[START SQL CALL FUNCTION - TEST {TestCount}]\n```sql\n{FunctionInfo.SqlFunctionCall}\n```\n[END SQL CALL FUNCTION - TEST {TestCount}]"
        );

        // Create the function in the PostgreSQL database
        FunctionInfo.FunctionCreatedSuccessfully = ExecuteSql(FunctionInfo.SqlFunctionDefinition);
        Assert.True(
            FunctionInfo.FunctionCreatedSuccessfully,
            $"[COMPILATION ERROR] Test {TestCount} failed to create function in the PostgreSQL database."
        );

        if (FunctionInfo.TestType == SqlTestType.Procedure)
        {
            bool? procedureResult = ExecuteSql(FunctionInfo.SqlFunctionCall);
            Assert.True(
                procedureResult.HasValue && procedureResult.Value,
                $"[EXECUTION ERROR] Test {TestCount} failed to execute procedure test."
            );
            Console.WriteLine($"[END TEST {TestCount}] Test {testName} executed successfully.\n");
            return;
        }

        // Call test and insert the test result into the PostgreSQL table
        FunctionInfo.TestId = ExecuteSqlReturnId(FunctionInfo.SqlFunctionCall);

        Assert.True(
            FunctionInfo.TestId.HasValue,
            $"[EXECUTION ERROR] Test {TestCount} failed to execute test and insert test result into table."
        );

        // Fetch the test result from the PostgreSQL table and validate it
        bool? testResult = FetchTestResult(FunctionInfo);
        Assert.True(
            testResult.HasValue,
            $"[UNEXPECTED ERROR] Test {TestCount} failed to get the test result value."
        );
        Assert.True(
            testResult.Value,
            $"[ASSERTION ERROR] Test {TestCount} returned unexpected result."
        );

        Console.WriteLine($"[END TEST {TestCount}] Test {testName} executed successfully.\n");
    }
}
