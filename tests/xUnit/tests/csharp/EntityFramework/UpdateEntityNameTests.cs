using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseUpdateEntityNameTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    protected string cteStatement = string.Empty;

    public BaseUpdateEntityNameTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            TestType = SqlTestType.Procedure,
            Name = "UpdateEntityName",
            Arguments = [
                new FunctionArgument("id", "INT"),
                new FunctionArgument("new_name", "TEXT")
            ],
            ReturnType = "",
            Body = FunctionBody,
            Language = Language,
            IsStrict = false,
            SqlSettings =
            [
                "pldotnet.user_assembly_paths = '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll'"
            ]
        };
    }

    public static IEnumerable<object[]> TestCases()
    {
        yield return new object[]
        {
            "c#-efcore-write-update",
            "UpdateEntityName",
            "3, 'Renamed Entity 3'",
            @"WITH data AS (
    SELECT ((SELECT COUNT(*) FROM test_entity_framework) = (SELECT COUNT(*) FROM bkp_test_entity_framework)
            AND
            (SELECT name FROM test_entity_framework WHERE id = 3) = 'Renamed Entity 3') AS test_result
)",
            "test_result = true"
        };
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestUpdateEntityName(
        string featureName,
        string testName,
        string input,
        string cteStatement,
        string customAssertion,
        string querySuffix = null!
    )
    {
        // Restore backup data before running the test
        ExecuteSql("CALL CopyTable('bkp_test_entity_framework', 'test_entity_framework');");
        RunTestWithSuffix(featureName, testName, cteStatement, customAssertion, querySuffix, true, input);
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "EntityFramework")]
public class UpdateEntityNameTestsCSharp : BaseUpdateEntityNameTests
{
    protected override string FunctionBody => @"
  using var ctx = new EFCoreTest.TestEntitiesContext();
  ctx.Database.AutoTransactionsEnabled = false;
  var entity = ctx.TestEntities.SingleOrDefault(e => e.Id == id);
  if (entity == null) return;
  entity.Name = new_name;
  ctx.SaveChanges();
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}
