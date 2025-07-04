using PlDotNET.Tests.Helper;
using System;
using System.Collections.Generic;
using Xunit;

public abstract class BaseTriggerTestExceptionTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseTriggerTestExceptionTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            Name = "trigger_test_exception",
            Arguments = new List<FunctionArgument>(),
            ReturnType = "TRIGGER",
            Body = FunctionBody,
            Language = Language,
            IsStrict = false
        };
    }

    private static string TriggerSetup => @"
    CREATE OR REPLACE TRIGGER test_trigger_AUS_4
        AFTER UPDATE ON trigger_test_table
        FOR EACH STATEMENT
        EXECUTE FUNCTION trigger_test_exception ('AFTER/UPDATE/STATEMENT', 4);
    ";

    public static object[][] TestCases()
    {
        var sql = SqlHelperScript.CommonTriggerTableSetup
                    + TriggerSetup
                    + @"
                        INSERT INTO trigger_test_table(id, message) VALUES (1, 'Initial');
                        UPDATE trigger_test_table SET message = 'Changed' WHERE id = 1;
                    ";

        return new[]
        {
            new object[]
            {
                "c#-trigger",
                "exceptionThrown",
                sql,
                null
            }
        };
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestTriggerException(string featureName, string testName, string input, string _ignoredAssertion)
    {
        SetupTest(input);

        Assert.Throws<SystemException>(() =>
            RunTestWithSuffix(featureName, testName, this.cteStatement, null, null)
        );
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "Trigger")]
public class TriggerTestExceptionTestsCSharp : BaseTriggerTestExceptionTests
{
    protected override string FunctionBody => @"
    throw new SystemException(""This is a test of exception handling"");
    return ReturnMode.Normal;
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}