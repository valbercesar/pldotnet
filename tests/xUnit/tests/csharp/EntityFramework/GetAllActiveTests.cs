using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseGetAllActiveTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseGetAllActiveTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            Name = "GetAllActive",
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
            ["c#-efcore-read-all", "GetAllActive", "", "= NOT EXISTS (SELECT 1 FROM test_entity_framework WHERE NOT is_active)"],
        ];
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestGetAllActive(string featureName, string testName, string input, string expectedResult)
    {
        RunGenericTest(featureName, testName, input, expectedResult);
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "EntityFramework")]
public class GetAllActiveTestsCSharp : BaseGetAllActiveTests
{
    protected override string FunctionBody => @"
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.All(e => e.IsActive);
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}
