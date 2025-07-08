using PlDotNET.Tests.Helper;
using System;
using System.Collections.Generic;
using Xunit;

public abstract class BaseTriggerTestTgValsTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseTriggerTestTgValsTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            Name = "trigger_test_tg_vals",
            Arguments = new List<FunctionArgument>(),
            ReturnType = "TRIGGER",
            Body = FunctionBody,
            Language = Language,
            IsStrict = false
        };
    }

    private static string TriggerSetup => @"
    CREATE OR REPLACE TRIGGER test_trigger_BIR_3
        BEFORE INSERT ON trigger_test_table
        FOR EACH ROW
        WHEN (new.id = 6)
        EXECUTE FUNCTION trigger_test_tg_vals('BEFORE/INSERT/ROW', 3);
    ";

    public static object[][] TestCases()
    {
        var sql = SqlHelperScript.CommonTriggerTableSetup
                    + TriggerSetup
                    + @"INSERT INTO trigger_test_table VALUES (6, 'Inserted Sixth Text (for values check modify)');";

        return new[]
        {
            new object[]
            {
                "c#-trigger",
                "tgValuesCorrect",
                sql,
                "EXISTS (SELECT 1 FROM trigger_test_table WHERE id = 6 and message = 'TG value assertions passed')"
            }
        };
    }

    // [Theory]
    [Theory(Skip="pulando para focar num só")]
    [MemberData(nameof(TestCases))]
    public void TestTriggerTestTgVals(string featureName, string testName, string input, string expectedResult)
    {
        RunGenericTest(featureName, testName, input, expectedResult);
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "Trigger")]
public class TriggerTestTgValsTestsCSharp : BaseTriggerTestTgValsTests
{
    protected override string FunctionBody => @"
    if((int)tg.NewRow[0] == 6) {
        if(
            (tg.TriggerName == ""test_trigger_bir_3"") &&
            (tg.TriggerWhen == ""BEFORE"") &&
            (tg.TriggerLevel == ""ROW"") &&
            (tg.TriggerEvent == ""INSERT"") &&
            (tg.RelationId > 0) &&
            (tg.TableName == ""trigger_test_table"") &&
            (tg.TableSchema == ""public"") &&
            (tg.NewRow.Length == 2) &&
            ((int)tg.NewRow[0] == 6) &&
            (tg.Arguments[0] == ""BEFORE/INSERT/ROW"") &&
            (tg.Arguments[1] == ""3"")
        )
        {
            tg.NewRow[1] = ""TG value assertions passed"";
            return ReturnMode.TriggerModify;
        }
    }
    Elog.Warning($""failed test, tg values didn't check out: {tg}"");
    return ReturnMode.Normal;
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}