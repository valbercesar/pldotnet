using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseGetAverageIDTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseGetAverageIDTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            Name = "GetAverageID",
            Arguments = [],
            ReturnType = "FLOAT8",
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
            ["c#-efcore-read-avg", "GetAverageID", "", "= (SELECT AVG(ID) FROM test_entity_framework)"],
        ];
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestGetAverageID(string featureName, string testName, string input, string expectedResult)
    {
        RunGenericTest(featureName, testName, input, expectedResult);
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "EntityFramework")]
public class GetAverageIDTestsCSharp : BaseGetAverageIDTests
{
    protected override string FunctionBody => @"
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Average(e => e.Id);
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}
