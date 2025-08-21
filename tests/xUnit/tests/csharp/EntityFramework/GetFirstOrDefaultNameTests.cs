using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseGetFirstOrDefaultNameTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseGetFirstOrDefaultNameTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            Name = "GetFirstOrDefaultName",
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

    public static object[][] TestCases()
    {
        return
        [
            ["c#-efcore-read-firstordefault", "GetFirstOrDefaultName", "", "= (SELECT name FROM test_entity_framework ORDER BY id LIMIT 1)"],
        ];
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestGetFirstOrDefaultName(string featureName, string testName, string input, string expectedResult)
    {
        RunGenericTest(featureName, testName, input, expectedResult);
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "EntityFramework")]
public class GetFirstOrDefaultNameTestsCSharp : BaseGetFirstOrDefaultNameTests
{
    protected override string FunctionBody => @"
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Select(e => e.Name).FirstOrDefault();
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}
