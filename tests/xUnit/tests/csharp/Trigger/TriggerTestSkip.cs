using PlDotNET.Tests.Helper;
using System;
using System.Collections.Generic;
using Xunit;

public abstract class BaseTriggerTestSkipTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseTriggerTestSkipTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            Name = "trigger_test_skip",
            Arguments = new List<FunctionArgument>(),
            ReturnType = "TRIGGER",
            Body = FunctionBody,
            Language = Language,
            IsStrict = false
        };
    }

    private static string TriggerSetup => @"
    CREATE OR REPLACE TRIGGER test_trigger_BIR_2
        BEFORE INSERT ON trigger_test_table
        FOR EACH ROW
        WHEN (new.id = 5)
        EXECUTE FUNCTION trigger_test_skip('BEFORE/INSERT/ROW', 2);
    ";

    public static object[][] TestCases()
    {
        var sql = SqlHelperScript.CommonTriggerTableSetup
                    + TriggerSetup
                    + @"INSERT INTO trigger_test_table VALUES (5, 'Inserted Fifth Text (for simple skip)');";

        return new[]
        {
            new object[]
            {
                "c#-trigger",
                "skipWorks",
                sql,
                "NOT EXISTS (SELECT 1 FROM trigger_test_table WHERE id = 5)"
            }
        };
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestTriggerTestSkip(string featureName, string testName, string input, string expectedResult)
    {
        RunGenericTest(featureName, testName, input, expectedResult);
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "Trigger")]
public class TriggerTestSkipTestsCSharp : BaseTriggerTestSkipTests
{
    protected override string FunctionBody => @"
    if (tg.Arguments[1] != ""2""){
        throw new SystemException($""Assertion failed: wrong trigger argument, '{tg.Arguments[1]}' != '2'"");
    }

    if((int)tg.NewRow[0] == 5) {
        return ReturnMode.TriggerSkip;
    }

    return ReturnMode.Normal;
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}