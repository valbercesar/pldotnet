using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseInsertEntitiesRangeTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    protected string cteStatement = string.Empty;

    public BaseInsertEntitiesRangeTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            TestType = SqlTestType.Procedure,
            Name = "InsertEntitiesRange",
            Arguments = [
                new FunctionArgument("names_array", "TEXT[]"),
                new FunctionArgument("categories_array", "TEXT[]"),
                new FunctionArgument("prices_array", "MONEY[]"),
                new FunctionArgument("created_at_array", "TIMESTAMP[]"),
                new FunctionArgument("is_active_array", "BOOLEAN[]")
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
            "c#-efcore-write-insert-range",
            "InsertEntitiesRange",
            "ARRAY['Bulk1','Bulk2'], ARRAY['CatA','CatB'], ARRAY['11.11'::MONEY,'22.22'::MONEY], ARRAY['2025-07-12 08:00:00'::TIMESTAMP,'2025-07-12 09:00:00'::TIMESTAMP], ARRAY[TRUE,FALSE]",
            @"WITH data AS (
    SELECT (EXISTS (
        SELECT 1
          FROM test_entity_framework
         WHERE name        = 'Bulk1'
           AND category    = 'CatA'
           AND price       = '11.11'::money
           AND created_at  = '2025-07-12 08:00:00'::timestamp
           AND is_active   = TRUE
      )
      AND EXISTS (
        SELECT 1
          FROM test_entity_framework
         WHERE name        = 'Bulk2'
           AND category    = 'CatB'
           AND price       = '22.22'::money
           AND created_at  = '2025-07-12 09:00:00'::timestamp
           AND is_active   = FALSE
      )
      AND (SELECT COUNT(*) FROM test_entity_framework) = (SELECT COUNT(*) FROM bkp_test_entity_framework) + 2
      AND (SELECT MAX(id) FROM test_entity_framework) = (SELECT MAX(id) FROM bkp_test_entity_framework) + 2) AS test_result
)",
            "test_result = true"
        };
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestInsertEntitiesRange(
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
public class InsertEntitiesRangeTestsCSharp : BaseInsertEntitiesRangeTests
{
    protected override string FunctionBody => @"
  var names = names_array.Cast<string>().ToArray();
  var categories = categories_array.Cast<string>().ToArray();
  var prices = prices_array.Cast<decimal?>().Select(p => p ?? 0m).ToArray();
  var createdAts = created_at_array.Cast<DateTime?>().Select(d => d ?? DateTime.Now).ToArray();
  var isActives = is_active_array.Cast<bool?>().Select(b => b ?? false).ToArray();
  var list = new List<EFCoreTest.TestEntityFramework>();
  for (int i = 0; i < names.Length; i++)
  {
      list.Add(new EFCoreTest.TestEntityFramework {
          Name      = names[i],
          Category  = categories[i],
          Price     = prices[i],
          CreatedAt = createdAts[i],
          IsActive  = isActives[i]
      });
  }
  using var ctx = new EFCoreTest.TestEntitiesContext();
  ctx.Database.AutoTransactionsEnabled = false;
  ctx.TestEntities.AddRange(list);
  ctx.SaveChanges();
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}
