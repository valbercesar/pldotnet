using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseGetJoinedCountTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    protected string cteStatement = string.Empty;

    public BaseGetJoinedCountTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            Name = "GetJoinedCount",
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

    protected void SetupTest(string cteStatement)
    {
        this.cteStatement = cteStatement;
        FunctionInfo!.CteStatement = cteStatement;
    }

    public static IEnumerable<object[]> TestCases()
    {
        yield return new object[]
        {
            "c#-efcore-read-join",
            "GetJoinedCount",
            @"WITH data AS (
    SELECT GetJoinedCount() AS result_value,
           (SELECT COUNT(*)
            FROM test_entity_framework t
            JOIN test_categories       c ON t.category = c.category) AS expected_value
)",
            "result_value = expected_value"
        };
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestGetJoinedCount(
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
public class GetJoinedCountTestsCSharp : BaseGetJoinedCountTests
{
    protected override string FunctionBody => @"
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Join(ctx.Categories, t => t.Category, c => c.Category, (t, c) => t).Count();
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}
