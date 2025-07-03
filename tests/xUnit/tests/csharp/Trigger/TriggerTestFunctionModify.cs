using PlDotNET.Tests.Helper;
using System;
using System.Collections.Generic;
using Xunit;

public abstract class BaseTriggerTestFunctionModifyTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseTriggerTestFunctionModifyTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            Name = "trigger_test_function_modify",
            Arguments = new List<FunctionArgument>(),
            ReturnType = "TRIGGER",
            Body = FunctionBody,
            Language = Language,
            IsStrict = false
        };
    }

    private static string TriggerSetup => @"
    CREATE TRIGGER test_trigger_BIR_1
        BEFORE INSERT ON trigger_test_table
        FOR EACH ROW
        WHEN (NEW.id = 2)
        EXECUTE FUNCTION trigger_test_function_modify('BEFORE/INSERT/ROW', '1');
    ";

    public static object[][] TestCases()
    {
        var sql = SqlHelperScript.CommonTriggerTableSetup
                    + TriggerSetup
                    + @"INSERT INTO trigger_test_table VALUES (2, 'Inserted Second Text');";

        return new[]
        {
            new object[]
            {
                "c#-trigger",
                "rowModifiedByTrigger",
                sql,
                "(SELECT message = 'MODIFIED Text!!!' FROM trigger_test_table WHERE id = 2)"
            }
        };
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestTriggerTestFunctionModify(string featureName, string testName, string input, string expectedResult)
    {
        RunGenericTest(featureName, testName, input, expectedResult);
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "Trigger")]
public class TriggerTestFunctionModifyTestsCSharp : BaseTriggerTestFunctionModifyTests
{
    protected override string FunctionBody => @"
    if (tg.Arguments[1] != ""1""){
        throw new SystemException($""Assertion failed: wrong trigger argument, '{tg.Arguments[1]}' != '1'"");
    }

    if ((int)tg.NewRow[0] != 2) {
        throw new SystemException($""Assertion failed: wrong row value, {tg.NewRow[1]} != 2"");
    }

    tg.NewRow[1] = ""MODIFIED Text!!!"";
    return ReturnMode.TriggerModify;
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}