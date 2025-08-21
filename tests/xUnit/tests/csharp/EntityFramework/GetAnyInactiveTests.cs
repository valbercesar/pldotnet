using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseGetAnyInactiveTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseGetAnyInactiveTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            Name = "GetAnyInactive",
            Arguments = [],
            ReturnType = "BOOLEAN",
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
            ["c#-efcore-read-any", "GetAnyInactive", "", "= EXISTS (SELECT 1 FROM test_entity_framework WHERE NOT is_active)"],
        ];
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestGetAnyInactive(string featureName, string testName, string input, string expectedResult)
    {
        RunGenericTest(featureName, testName, input, expectedResult);
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "EntityFramework")]
public class GetAnyInactiveTestsCSharp : BaseGetAnyInactiveTests
{
    protected override string FunctionBody => @"
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Any(e => !e.IsActive);
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}
