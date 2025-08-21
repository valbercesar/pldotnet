SET pldotnet.user_assembly_paths = '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll';

-- 1. COUNT
CREATE OR REPLACE FUNCTION GetTotalCount() RETURNS INT AS $$
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Count();
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-read-count',
  'GetTotalCount',
  GetTotalCount() = (SELECT COUNT(*) FROM test_entity_framework);

-- 2. SUM
CREATE OR REPLACE FUNCTION GetSumPrice() RETURNS MONEY AS $$
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Sum(e => e.Price);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-read-sum',
  'GetSumPrice',
  GetSumPrice() = (SELECT SUM(price) FROM test_entity_framework);

-- 3. MIN
CREATE OR REPLACE FUNCTION GetMinPrice() RETURNS MONEY AS $$
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Min(e => e.Price);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-read-min',
  'GetMinPrice',
  GetMinPrice() = (SELECT MIN(price) FROM test_entity_framework);

-- 4. MAX
CREATE OR REPLACE FUNCTION GetMaxPrice() RETURNS MONEY AS $$
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Max(e => e.Price);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-read-max',
  'GetMaxPrice',
  GetMaxPrice() = (SELECT MAX(price) FROM test_entity_framework);

-- 5. AVG
CREATE OR REPLACE FUNCTION GetAverageID() RETURNS FLOAT8 AS $$
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Average(e => e.Id);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-read-avg',
  'GetAverageID',
  GetAverageID() = (SELECT AVG(ID) FROM test_entity_framework);

-- 6. ANY
CREATE OR REPLACE FUNCTION GetAnyInactive() RETURNS BOOLEAN AS $$
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Any(e => !e.IsActive);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-read-any',
  'GetAnyInactive',
  GetAnyInactive() = EXISTS (SELECT 1 FROM test_entity_framework WHERE NOT is_active);

-- 7. ALL
CREATE OR REPLACE FUNCTION GetAllActive() RETURNS BOOLEAN AS $$
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.All(e => e.IsActive);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-read-all',
  'GetAllActive',
  GetAllActive() = NOT EXISTS (SELECT 1 FROM test_entity_framework WHERE NOT is_active);

-- 8. FIRST
CREATE OR REPLACE FUNCTION GetFirstName() RETURNS TEXT AS $$
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Select(e => e.Name).FirstOrDefault();
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-read-first',
  'GetFirstName',
  GetFirstName() = (SELECT name FROM test_entity_framework ORDER BY id LIMIT 1);

-- 9. FIRST OR DEFAULT
CREATE OR REPLACE FUNCTION GetFirstOrDefaultName() RETURNS TEXT AS $$
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Select(e => e.Name).FirstOrDefault();
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-read-firstordefault',
  'GetFirstOrDefaultName',
  GetFirstOrDefaultName() = (SELECT name FROM test_entity_framework ORDER BY id LIMIT 1);

-- 10. SINGLE
CREATE OR REPLACE FUNCTION GetSingleByName(test_name TEXT) RETURNS INT AS $$
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Single(e => e.Name == test_name)?.Id ?? 0;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-read-single',
  'GetSingleByName(Entity 3)',
  GetSingleByName('Entity 3') = (SELECT id FROM test_entity_framework WHERE name = 'Entity 3');

-- 11. SINGLE OR DEFAULT
CREATE OR REPLACE FUNCTION GetSingleOrDefaultByName(test_name TEXT) RETURNS INT AS $$
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.SingleOrDefault(e => e.Name == test_name)?.Id ?? 0;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-read-singleordefault',
  'GetSingleOrDefaultByName(Entity 3)',
  GetSingleOrDefaultByName('Entity 3') = (SELECT id FROM test_entity_framework WHERE name = 'Entity 3');

-- 12. DISTINCT
CREATE OR REPLACE FUNCTION GetDistinctCategoriesCount() RETURNS INT AS $$
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Select(e => e.Category).Distinct().Count();
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-read-distinct',
  'GetDistinctCategoriesCount',
  GetDistinctCategoriesCount() = (SELECT COUNT(DISTINCT category) FROM test_entity_framework);

-- 13. GROUP
CREATE OR REPLACE FUNCTION GetCategoryCounts() RETURNS TEXT AS $$
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return string.Join(',', ctx.TestEntities
    .GroupBy(e => e.Category)
    .Select(g => $"{g.Key}:{g.Count()}"));
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-read-group',
  'GetCategoryCounts',
  GetCategoryCounts() = (
    SELECT string_agg(cat || ':' || catcnt, ',' ORDER BY cat)
    FROM (
      SELECT category AS cat, COUNT(*)::text AS catcnt
      FROM test_entity_framework
      GROUP BY category
    ) t
  );

-- 14. SKIP
CREATE OR REPLACE FUNCTION GetSkipCount(skip INT) RETURNS INT AS $$
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Skip(skip ?? 0).Count();
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-read-skip',
  'GetSkipCount(2)',
  GetSkipCount(2) = (SELECT GREATEST(0, COUNT(*) - 2) FROM test_entity_framework);

-- 15. TAKE
CREATE OR REPLACE FUNCTION GetTakeCount(take INT) RETURNS INT AS $$
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Take(take ?? 0).Count();
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-read-take',
  'GetTakeCount(3)',
  GetTakeCount(3) = LEAST(3, (SELECT COUNT(*) FROM test_entity_framework));


-- 16. JOIN
CREATE OR REPLACE FUNCTION GetJoinedCount() RETURNS INT AS $$
  using var ctx = new EFCoreTest.TestEntitiesContext();
  return ctx.TestEntities.Join(ctx.Categories, t => t.Category, c => c.Category, (t, c) => t).Count();
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-read-join',
  'GetJoinedCount',
  GetJoinedCount() = (
    SELECT COUNT(*)
    FROM test_entity_framework t
    JOIN test_categories       c ON t.category = c.category
  );

-- 17. DELETE
CREATE OR REPLACE PROCEDURE DeleteFirstEntity() AS $$
  using var ctx = new EFCoreTest.TestEntitiesContext();
  ctx.Database.AutoTransactionsEnabled = false;
  var first = ctx.TestEntities.OrderBy(e => e.Id).FirstOrDefault();
  if (first != null)
  {
      ctx.TestEntities.Remove(first);
      ctx.SaveChanges();
  }
$$ LANGUAGE plcsharp;
CALL CopyTable('bkp_test_entity_framework', 'test_entity_framework');
CALL DeleteFirstEntity();
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-write-delete',
  'DeleteFirstEntity',
  (
    SELECT COUNT(*) FROM test_entity_framework
  ) = (
    SELECT COUNT(*) FROM bkp_test_entity_framework WHERE id <> 1
  );

-- 18. DELETE RANGE
CREATE OR REPLACE PROCEDURE DeleteEntitiesByIds(ids INT[]) AS $$
  using var ctx = new EFCoreTest.TestEntitiesContext();
  ctx.Database.AutoTransactionsEnabled = false;
  var toDelete = ctx.TestEntities.Where(e => ids.Cast<int>().ToArray().Contains(e.Id)).ToList();
  if (toDelete.Count > 0)
  {
      ctx.TestEntities.RemoveRange(toDelete);
      ctx.SaveChanges();
  }
$$ LANGUAGE plcsharp;
CALL CopyTable('bkp_test_entity_framework', 'test_entity_framework');
CALL DeleteEntitiesByIds(ARRAY[2,4]);
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-write-delete-range',
  'DeleteEntitiesByIds',
  (SELECT COUNT(*) FROM test_entity_framework) = (SELECT COUNT(*) FROM bkp_test_entity_framework WHERE id NOT IN (2, 4));

-- 19. UPDATE
CREATE OR REPLACE PROCEDURE UpdateEntityName(id INT, new_name TEXT) AS $$
  using var ctx = new EFCoreTest.TestEntitiesContext();
  ctx.Database.AutoTransactionsEnabled = false;
  var entity = ctx.TestEntities.SingleOrDefault(e => e.Id == id);
  if (entity == null) return;
  entity.Name = new_name;
  ctx.SaveChanges();
$$ LANGUAGE plcsharp;
CALL CopyTable('bkp_test_entity_framework', 'test_entity_framework');
CALL UpdateEntityName(3, 'Renamed Entity 3');
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-write-update',
  'UpdateEntityName',
  (
    (SELECT COUNT(*) FROM test_entity_framework)
      = (SELECT COUNT(*) FROM bkp_test_entity_framework)
    AND
    (SELECT name FROM test_entity_framework WHERE id = 3)
      = 'Renamed Entity 3'
  );

-- 20. UPDATE RANGE
CREATE OR REPLACE PROCEDURE DoublePriceByCategories(categories TEXT[]) AS $$
  using var ctx = new EFCoreTest.TestEntitiesContext();
  ctx.Database.AutoTransactionsEnabled = false;
  var toUpdate = ctx.TestEntities.Where(e => categories.Cast<string>().ToArray().Contains(e.Category)).ToList();
  foreach (var e in toUpdate)
      e.Price *= 2;
  if (toUpdate.Count > 0)
      ctx.SaveChanges();
$$ LANGUAGE plcsharp;
CALL CopyTable('bkp_test_entity_framework', 'test_entity_framework');
CALL DoublePriceByCategories(ARRAY['Category 1','Category 3']);
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-write-update-range',
  'DoublePriceByCategories',
  (
    (SELECT SUM(price) FROM test_entity_framework)
    = (
      2 * COALESCE((SELECT SUM(price) FROM bkp_test_entity_framework WHERE category = ANY(ARRAY['Category 1','Category 3'])), '0'::MONEY)
    ) + (
      COALESCE((SELECT SUM(price) FROM bkp_test_entity_framework WHERE category <> ALL(ARRAY['Category 1','Category 3'])), '0'::MONEY)
    )
  );

-- 21. INSERT
CREATE OR REPLACE PROCEDURE InsertEntity(name TEXT, category TEXT, price MONEY, created_at TIMESTAMP, is_active BOOLEAN) AS $$
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
$$ LANGUAGE plcsharp;
CALL CopyTable('bkp_test_entity_framework', 'test_entity_framework');
CALL InsertEntity('New Entity', 'Category X', '15.00'::MONEY, '2025-07-11 12:00:00', TRUE);
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-write-insert',
  'InsertEntity',
  EXISTS (
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
  (
    SELECT id FROM test_entity_framework ORDER BY id DESC LIMIT 1
  ) = (
    SELECT id + 1 FROM bkp_test_entity_framework ORDER BY id DESC LIMIT 1
  );

-- 22. INSERT RANGE
CREATE OR REPLACE PROCEDURE InsertEntitiesRange(
  names_array TEXT[], categories_array TEXT[], prices_array MONEY[], created_at_array TIMESTAMP[], is_active_array BOOLEAN[]
) AS $$
  var names = names_array.Cast<string>().ToArray();
  var categories = categories_array.Cast<string>().ToArray(\);
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
$$ LANGUAGE plcsharp;
CALL CopyTable('bkp_test_entity_framework', 'test_entity_framework');
CALL InsertEntitiesRange(
  ARRAY['Bulk1','Bulk2'],
  ARRAY['CatA','CatB'],
  ARRAY['11.11'::MONEY,'22.22'::MONEY],
  ARRAY['2025-07-12 08:00:00'::TIMESTAMP,'2025-07-12 09:00:00'::TIMESTAMP],
  ARRAY[TRUE,FALSE]
);
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-write-insert-range',
  'InsertEntitiesRange',
  (
    EXISTS (
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
    AND (SELECT COUNT(*) FROM test_entity_framework)
      = (SELECT COUNT(*) FROM bkp_test_entity_framework) + 2
    AND (SELECT MAX(id) FROM test_entity_framework)
      = (SELECT MAX(id) FROM bkp_test_entity_framework) + 2
  );
