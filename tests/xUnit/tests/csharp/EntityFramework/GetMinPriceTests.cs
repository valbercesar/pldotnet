using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseGetMinPriceTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseGetMinPriceTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            Name = "GetMinPrice",
            Arguments = [],
            ReturnType = "MONEY",
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
            ["c#-efcore-read-min", "GetMinPrice", "", "= (SELECT MIN(price) FROM test_entity_framework)"],
        ];
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestGetMinPrice(string featureName, string testName, string input, string expectedResult)
    {
        RunGenericTest(featureName, testName, input, expectedResult);
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "EntityFramework")]
public class GetMinPriceTestsCSharp : BaseGetMinPriceTests
{
    protected override string FunctionBody => @"
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Min(e => e.Price);
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}
