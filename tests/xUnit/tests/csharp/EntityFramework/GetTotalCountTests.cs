using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseGetTotalCountTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseGetTotalCountTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            Name = "GetTotalCount",
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
            ["c#-efcore-read-count", "GetTotalCount", "", "= (SELECT COUNT(*) FROM test_entity_framework)"],
        ];
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestGetTotalCount(string featureName, string testName, string input, string expectedResult)
    {
        RunGenericTest(featureName, testName, input, expectedResult);
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "EntityFramework")]
public class GetTotalCountTestsCSharp : BaseGetTotalCountTests
{
    protected override string FunctionBody => @"
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Count();
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}
