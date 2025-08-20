using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseGetDistinctCategoriesCountTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseGetDistinctCategoriesCountTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            Name = "GetDistinctCategoriesCount",
            Arguments = [],
            ReturnType = "INT",
            Body = FunctionBody,
            Language = Language,
            IsStrict = false,
            SqlSettings =
            [
                "pldotnet.user_assembly_paths = '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll'"
            ]
        };
    }

    public static object[][] TestCases()
    {
        return
        [
            ["c#-efcore-read-distinct", "GetDistinctCategoriesCount", "", "= (SELECT COUNT(DISTINCT category) FROM test_entity_framework)"],
        ];
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestGetDistinctCategoriesCount(string featureName, string testName, string input, string expectedResult)
    {
        RunGenericTest(featureName, testName, input, expectedResult);
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "EntityFramework")]
public class GetDistinctCategoriesCountTestsCSharp : BaseGetDistinctCategoriesCountTests
{
    protected override string FunctionBody => @"
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Select(e => e.Category).Distinct().Count();
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}
