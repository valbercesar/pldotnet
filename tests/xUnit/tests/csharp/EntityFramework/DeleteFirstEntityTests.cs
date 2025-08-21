using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseDeleteFirstEntityTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseDeleteFirstEntityTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            TestType = SqlTestType.Procedure,
            Name = "DeleteFirstEntity",
            Arguments = [],
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
            "c#-efcore-write-delete",
            "DeleteFirstEntity",
            @"WITH data AS (
    SELECT (SELECT COUNT(*) FROM test_entity_framework) AS current_count,
           (SELECT COUNT(*) FROM bkp_test_entity_framework WHERE id <> 1) AS expected_count
)",
            "current_count = expected_count"
        };
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestDeleteFirstEntity(
        string featureName,
        string testName,
        string cteStatement,
        string customAssertion,
        string querySuffix = null!
    )
    {
        // Restore backup data before running the test
        ExecuteSql("CALL CopyTable('bkp_test_entity_framework', 'test_entity_framework');");
        RunTestWithSuffix(featureName, testName, cteStatement, customAssertion, querySuffix, true);
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "EntityFramework")]
public class DeleteFirstEntityTestsCSharp : BaseDeleteFirstEntityTests
{
    protected override string FunctionBody => @"
  using var ctx = new EFCoreTest.TestEntitiesContext();
  ctx.Database.AutoTransactionsEnabled = false;
  var first = ctx.TestEntities.OrderBy(e => e.Id).FirstOrDefault();
  if (first != null)
  {
      ctx.TestEntities.Remove(first);
      ctx.SaveChanges();
  }
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}
