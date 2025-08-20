using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseInsertEntityTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    protected string cteStatement = string.Empty;

    public BaseInsertEntityTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            TestType = SqlTestType.Procedure,
            Name = "InsertEntity",
            Arguments = [
                new FunctionArgument("name", "TEXT"),
                new FunctionArgument("category", "TEXT"),
                new FunctionArgument("price", "MONEY"),
                new FunctionArgument("created_at", "TIMESTAMP"),
                new FunctionArgument("is_active", "BOOLEAN")
            ],
            ReturnType = "",
            Body = FunctionBody,
            Language = Language,
            IsStrict = false,
            SqlSettings =
            [
                "pldotnet.user_assembly_paths = '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll'"
            ]
        };
    }

    public static IEnumerable<object[]> TestCases()
    {
        yield return new object[]
        {
            "c#-efcore-write-insert",
            "InsertEntity",
            "'New Entity', 'Category X', '15.00'::MONEY, '2025-07-11 12:00:00', TRUE",
            @"WITH data AS (
    SELECT (EXISTS (
        SELECT 1
          FROM test_entity_framework
         WHERE
           name = 'New Entity'
           AND category = 'Category X'
           AND price = '15.00'::money
           AND created_at = '2025-07-11 12:00:00'::timestamp
           AND is_active = TRUE
      )
      AND
      (SELECT id FROM test_entity_framework ORDER BY id DESC LIMIT 1) =
      (SELECT id + 1 FROM bkp_test_entity_framework ORDER BY id DESC LIMIT 1)) AS test_result
)",
            "test_result = true"
        };
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestInsertEntity(
        string featureName,
        string testName,
        string input,
        string cteStatement,
        string customAssertion,
        string querySuffix = null!
    )
    {
        // Restore backup data before running the test
        ExecuteSql("CALL CopyTable('bkp_test_entity_framework', 'test_entity_framework');");
        RunTestWithSuffix(featureName, testName, cteStatement, customAssertion, querySuffix, true, input);
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "EntityFramework")]
public class InsertEntityTestsCSharp : BaseInsertEntityTests
{
    protected override string FunctionBody => @"
  using var ctx = new EFCoreTest.TestEntitiesContext();
  ctx.Database.AutoTransactionsEnabled = false;
  var ent = new EFCoreTest.TestEntityFramework
  {
      Name = name,
      Category = category,
      Price = price ?? 0m,
      CreatedAt = created_at ?? DateTime.Now,
      IsActive = is_active ?? false
  };
  ctx.TestEntities.Add(ent);
  ctx.SaveChanges();
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}
