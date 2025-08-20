using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseGetSkipCountTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseGetSkipCountTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            Name = "GetSkipCount",
            Arguments = [new FunctionArgument("skip", "INT")],
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
            ["c#-efcore-read-skip", "GetSkipCount(2)", "2", "= (SELECT GREATEST(0, COUNT(*) - 2) FROM test_entity_framework)"],
        ];
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestGetSkipCount(string featureName, string testName, string input, string expectedResult)
    {
        RunGenericTest(featureName, testName, input, expectedResult);
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "EntityFramework")]
public class GetSkipCountTestsCSharp : BaseGetSkipCountTests
{
    protected override string FunctionBody => @"
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Skip(skip ?? 0).Count();
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}
