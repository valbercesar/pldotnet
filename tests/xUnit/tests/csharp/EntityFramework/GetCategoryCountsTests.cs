using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseGetCategoryCountsTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    protected string cteStatement = string.Empty;

    public BaseGetCategoryCountsTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            Name = "GetCategoryCounts",
            Arguments = [],
            ReturnType = "TEXT",
            Body = FunctionBody,
            Language = Language,
            IsStrict = false,
            SqlSettings =
            [
                "pldotnet.user_assembly_paths = '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll'"
            ]
        };
    }

    protected void SetupTest(string cteStatement)
    {
        this.cteStatement = cteStatement;
        FunctionInfo!.CteStatement = cteStatement;
    }

    public static IEnumerable<object[]> TestCases()
    {
        yield return new object[]
        {
            "c#-efcore-read-group",
            "GetCategoryCounts",
            @"WITH data AS (
    SELECT GetCategoryCounts() AS result_value,
           (SELECT string_agg(cat || ':' || catcnt, ',' ORDER BY cat)
            FROM (
              SELECT category AS cat, COUNT(*)::text AS catcnt
              FROM test_entity_framework
              GROUP BY category
            ) t) AS expected_value
)",
            "result_value = expected_value"
        };
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestGetCategoryCounts(
        string featureName,
        string testName,
        string cteStatement,
        string customAssertion,
        string querySuffix = null!
    )
    {
        SetupTest(cteStatement);
        RunTestWithSuffix(featureName, testName, this.cteStatement, customAssertion, querySuffix);
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "EntityFramework")]
public class GetCategoryCountsTestsCSharp : BaseGetCategoryCountsTests
{
    protected override string FunctionBody => @"
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return string.Join(',', ctx.TestEntities
    .GroupBy(e => e.Category)
    .OrderBy(g => g.Key)
    .Select(g => $""{g.Key}:{g.Count()}""));
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}
