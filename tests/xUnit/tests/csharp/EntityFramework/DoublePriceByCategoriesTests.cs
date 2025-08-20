using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseDoublePriceByCategoriesTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    protected string cteStatement = string.Empty;

    public BaseDoublePriceByCategoriesTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            TestType = SqlTestType.Procedure,
            Name = "DoublePriceByCategories",
            Arguments = [new FunctionArgument("categories", "TEXT[]")],
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
            "c#-efcore-write-update-range",
            "DoublePriceByCategories",
            "ARRAY['Category 1','Category 3']",
            @"WITH data AS (
    SELECT ((SELECT SUM(price) FROM test_entity_framework) =
            (2 * COALESCE((SELECT SUM(price) FROM bkp_test_entity_framework WHERE category = ANY(ARRAY['Category 1','Category 3'])), '0'::MONEY)) +
            (COALESCE((SELECT SUM(price) FROM bkp_test_entity_framework WHERE category <> ALL(ARRAY['Category 1','Category 3'])), '0'::MONEY))) AS test_result
)",
            "test_result = true"
        };
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestDoublePriceByCategories(
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
public class DoublePriceByCategoriesTestsCSharp : BaseDoublePriceByCategoriesTests
{
    protected override string FunctionBody => @"
  using var ctx = new EFCoreTest.TestEntitiesContext();
  ctx.Database.AutoTransactionsEnabled = false;
  var toUpdate = ctx.TestEntities.Where(e => categories.Cast<string>().ToArray().Contains(e.Category)).ToList();
  foreach (var e in toUpdate)
      e.Price *= 2;
  if (toUpdate.Count > 0)
      ctx.SaveChanges();
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}
