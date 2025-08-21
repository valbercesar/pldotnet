using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseGetSingleOrDefaultByNameTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseGetSingleOrDefaultByNameTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            Name = "GetSingleOrDefaultByName",
            Arguments = [new FunctionArgument("test_name", "TEXT")],
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
            ["c#-efcore-read-singleordefault", "GetSingleOrDefaultByName(Entity 3)", "'Entity 3'", "= (SELECT id FROM test_entity_framework WHERE name = 'Entity 3')"],
        ];
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestGetSingleOrDefaultByName(string featureName, string testName, string input, string expectedResult)
    {
        // Restore backup data before running the test to ensure original entity names exist
        ExecuteSql("CALL CopyTable('bkp_test_entity_framework', 'test_entity_framework');");
        RunGenericTest(featureName, testName, input, expectedResult);
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "EntityFramework")]
public class GetSingleOrDefaultByNameTestsCSharp : BaseGetSingleOrDefaultByNameTests
{
    protected override string FunctionBody => @"
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.SingleOrDefault(e => e.Name == test_name)?.Id ?? 0;
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}
