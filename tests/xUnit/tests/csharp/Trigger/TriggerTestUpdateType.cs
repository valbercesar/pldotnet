using PlDotNET.Tests.Helper;
using System;
using System.Collections.Generic;
using Xunit;

public abstract class BaseTriggerTestUpdateTypeTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseTriggerTestUpdateTypeTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            Name = "trigger_test_update_type",
            Arguments = new List<FunctionArgument>(),
            ReturnType = "TRIGGER",
            Body = FunctionBody,
            Language = Language,
            IsStrict = false
        };
    }

    private static string TriggerSetup => @"
    CREATE OR REPLACE TRIGGER test_trigger_BIR_4
        BEFORE INSERT ON trigger_test_table
        FOR EACH ROW
        WHEN (new.id = 7)
        EXECUTE FUNCTION trigger_test_update_type('BEFORE/INSERT/ROW', 4);
    ";

    public static object[][] TestCases()
    {
        var sql = SqlHelperScript.CommonTriggerTableSetup
                    + TriggerSetup
                    + @"INSERT INTO trigger_test_table VALUES (7, 'This should be a string');";

        return new[]
        {
            new object[]
            {
                "c#-trigger",
                "tgTypeMismatchHandling",
                sql,
                "EXISTS (SELECT 1 FROM trigger_test_table WHERE id = 7 and message = 'This should be a string')"
            }
        };
    }

    // [Theory]
    [Theory(Skip="pulando para focar num só")]
    [MemberData(nameof(TestCases))]
    public void TestTriggerTestUpdateType(string featureName, string testName, string input, string expectedResult)
    {
        RunGenericTest(featureName, testName, input, expectedResult);
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "Trigger")]
public class TriggerTestUpdateTypeTestsCSharp : BaseTriggerTestUpdateTypeTests
{
    protected override string FunctionBody => @"
    tg.NewRow[1] = 1;
    return ReturnMode.TriggerModify;
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}