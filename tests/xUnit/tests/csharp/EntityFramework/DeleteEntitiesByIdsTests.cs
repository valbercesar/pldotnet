using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseDeleteEntitiesByIdsTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    protected string cteStatement = string.Empty;

    public BaseDeleteEntitiesByIdsTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            TestType = SqlTestType.Procedure,
            Name = "DeleteEntitiesByIds",
            Arguments = [new FunctionArgument("ids", "INT[]")],
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
            "c#-efcore-write-delete-range",
            "DeleteEntitiesByIds",
            "ARRAY[2,4]",
            @"WITH data AS (
    SELECT (SELECT COUNT(*) FROM test_entity_framework) AS current_count,
           (SELECT COUNT(*) FROM bkp_test_entity_framework WHERE id NOT IN (2, 4)) AS expected_count
)",
            "current_count = expected_count"
        };
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestDeleteEntitiesByIds(
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
public class DeleteEntitiesByIdsTestsCSharp : BaseDeleteEntitiesByIdsTests
{
    protected override string FunctionBody => @"
  using var ctx = new EFCoreTest.TestEntitiesContext();
  ctx.Database.AutoTransactionsEnabled = false;
  var toDelete = ctx.TestEntities.Where(e => ids.Cast<int>().ToArray().Contains(e.Id)).ToList();
  if (toDelete.Count > 0)
  {
      ctx.TestEntities.RemoveRange(toDelete);
      ctx.SaveChanges();
  }
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}
