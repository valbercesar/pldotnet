using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseGetTakeCountTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseGetTakeCountTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            Name = "GetTakeCount",
            Arguments = [new FunctionArgument("take", "INT")],
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
            ["c#-efcore-read-take", "GetTakeCount(3)", "3", "= LEAST(3, (SELECT COUNT(*) FROM test_entity_framework))"],
        ];
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestGetTakeCount(string featureName, string testName, string input, string expectedResult)
    {
        RunGenericTest(featureName, testName, input, expectedResult);
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "EntityFramework")]
public class GetTakeCountTestsCSharp : BaseGetTakeCountTests
{
    protected override string FunctionBody => @"
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Take(take ?? 0).Count();
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}
