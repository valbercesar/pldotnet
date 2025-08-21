using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseGetMaxPriceTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseGetMaxPriceTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            Name = "GetMaxPrice",
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
            ["c#-efcore-read-max", "GetMaxPrice", "", "= (SELECT MAX(price) FROM test_entity_framework)"],
        ];
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestGetMaxPrice(string featureName, string testName, string input, string expectedResult)
    {
        RunGenericTest(featureName, testName, input, expectedResult);
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "EntityFramework")]
public class GetMaxPriceTestsCSharp : BaseGetMaxPriceTests
{
    protected override string FunctionBody => @"
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Max(e => e.Price);
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}
