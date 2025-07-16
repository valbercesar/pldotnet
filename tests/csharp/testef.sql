DROP TABLE IF EXISTS public.test_entity_framework CASCADE;
CREATE TABLE public.test_entity_framework (
  id          SERIAL PRIMARY KEY,
  name        VARCHAR(100)  NOT NULL,
  category    VARCHAR(50)   NOT NULL,
  price       MONEY NOT NULL,
  created_at  TIMESTAMP     NOT NULL DEFAULT now(),
  is_active   BOOLEAN       NOT NULL DEFAULT true
);
INSERT INTO public.test_entity_framework (name, category, price, created_at, is_active)
VALUES
  ('Entity 1','Category 1',  '10.00'::MONEY, '2025-07-10 10:00:00', TRUE),
  ('Entity 2','Category 2',  '20.50'::MONEY, '2025-07-09 11:30:00', FALSE),
  ('Entity 3','Category 1',  '30.75'::MONEY, '2025-07-08 09:45:00', TRUE),
  ('Entity 4','Category 3',  '40.25'::MONEY, '2025-07-07 14:15:00', FALSE),
  ('Entity 5','Category 2',  '50.00'::MONEY, '2025-07-06 13:00:00', TRUE);

DROP TABLE IF EXISTS public.test_categories CASCADE;
CREATE TABLE public.test_categories (
  category   VARCHAR(50) PRIMARY KEY,
  sort_order INT         NOT NULL
);
INSERT INTO public.test_categories(category, sort_order)
VALUES
  ('Category 1', 1),
  ('Category 2', 2);

-- 1. COUNT
CREATE OR REPLACE FUNCTION GetTotalCount() RETURNS INT
  AS '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll:EFCoreTest.TestEntityFunctions!GetTotalCount'
  LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-count',
  'GetTotalCount',
  GetTotalCount() = (SELECT COUNT(*) FROM test_entity_framework);

-- 2. SUM
CREATE OR REPLACE FUNCTION GetSumPrice() RETURNS MONEY
  AS '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll:EFCoreTest.TestEntityFunctions!GetSumPrice'
  LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-sum',
  'GetSumPrice',
  GetSumPrice() = (SELECT SUM(price) FROM test_entity_framework);

-- 3. MIN
CREATE OR REPLACE FUNCTION GetMinPrice() RETURNS MONEY
  AS '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll:EFCoreTest.TestEntityFunctions!GetMinPrice'
  LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-min',
  'GetMinPrice',
  GetMinPrice() = (SELECT MIN(price) FROM test_entity_framework);

-- 4. MAX
CREATE OR REPLACE FUNCTION GetMaxPrice() RETURNS MONEY
  AS '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll:EFCoreTest.TestEntityFunctions!GetMaxPrice'
  LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-max',
  'GetMaxPrice',
  GetMaxPrice() = (SELECT MAX(price) FROM test_entity_framework);

-- 5. AVG
CREATE OR REPLACE FUNCTION GetAverageID() RETURNS FLOAT8
  AS '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll:EFCoreTest.TestEntityFunctions!GetAverageID'
  LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-avg',
  'GetAverageID',
  GetAverageID() = (SELECT AVG(ID) FROM test_entity_framework);

-- 6. ANY
CREATE OR REPLACE FUNCTION GetAnyInactive() RETURNS BOOLEAN
  AS '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll:EFCoreTest.TestEntityFunctions!GetAnyInactive'
  LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-any',
  'GetAnyInactive',
  GetAnyInactive() = EXISTS (SELECT 1 FROM test_entity_framework WHERE NOT is_active);

-- 7. ALL
CREATE OR REPLACE FUNCTION GetAllActive() RETURNS BOOLEAN
  AS '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll:EFCoreTest.TestEntityFunctions!GetAllActive'
  LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-all',
  'GetAllActive',
  GetAllActive() = NOT EXISTS (SELECT 1 FROM test_entity_framework WHERE NOT is_active);

-- 8. FIRST
CREATE OR REPLACE FUNCTION GetFirstName() RETURNS TEXT
  AS '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll:EFCoreTest.TestEntityFunctions!GetFirstName'
  LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-first',
  'GetFirstName',
  GetFirstName() = (SELECT name FROM test_entity_framework ORDER BY id LIMIT 1);

-- 9. FIRST OR DEFAULT
CREATE OR REPLACE FUNCTION GetFirstOrDefaultName() RETURNS TEXT
  AS '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll:EFCoreTest.TestEntityFunctions!GetFirstOrDefaultName'
  LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-firstordefault',
  'GetFirstOrDefaultName',
  GetFirstOrDefaultName() = (SELECT name FROM test_entity_framework ORDER BY id LIMIT 1);

-- 10. SINGLE
CREATE OR REPLACE FUNCTION GetSingleByName(test_name TEXT) RETURNS INT
  AS '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll:EFCoreTest.TestEntityFunctions!GetSingleByName'
  LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-single',
  'GetSingleByName(Entity 3)',
  GetSingleByName('Entity 3') = (SELECT id FROM test_entity_framework WHERE name = 'Entity 3');

-- 11. SINGLE OR DEFAULT
CREATE OR REPLACE FUNCTION GetSingleOrDefaultByName(test_name TEXT) RETURNS INT
  AS '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll:EFCoreTest.TestEntityFunctions!GetSingleOrDefaultByName'
  LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-singleordefault',
  'GetSingleOrDefaultByName(Entity 3)',
  GetSingleOrDefaultByName('Entity 3') = (SELECT id FROM test_entity_framework WHERE name = 'Entity 3');

-- 12. DISTINCT
CREATE OR REPLACE FUNCTION GetDistinctCategoriesCount() RETURNS INT
  AS '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll:EFCoreTest.TestEntityFunctions!GetDistinctCategoriesCount'
  LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-distinct',
  'GetDistinctCategoriesCount',
  GetDistinctCategoriesCount() = (SELECT COUNT(DISTINCT category) FROM test_entity_framework);

-- 13. GROUP (FIX)
CREATE OR REPLACE FUNCTION GetCategoryCounts() RETURNS TEXT
  AS '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll:EFCoreTest.TestEntityFunctions!GetCategoryCounts'
  LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-group',
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
CREATE OR REPLACE FUNCTION GetSkipCount(skip INT) RETURNS INT
  AS '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll:EFCoreTest.TestEntityFunctions!GetSkipCount'
  LANGUAGE plcsharp STRICT;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-skip',
  'GetSkipCount(2)',
  GetSkipCount(2) = (SELECT GREATEST(0, COUNT(*) - 2) FROM test_entity_framework);

-- 15. TAKE
CREATE OR REPLACE FUNCTION GetTakeCount(take INT) RETURNS INT
  AS '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll:EFCoreTest.TestEntityFunctions!GetTakeCount'
  LANGUAGE plcsharp STRICT;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-take',
  'GetTakeCount(3)',
  GetTakeCount(3) = LEAST(3, (SELECT COUNT(*) FROM test_entity_framework));


-- 16. JOIN
CREATE OR REPLACE FUNCTION GetJoinedCount() RETURNS INT
  AS '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll:EFCoreTest.TestEntityFunctions!GetJoinedCount'
  LANGUAGE plcsharp;
INSERT INTO automated_test_results(feature, test_name, result)
SELECT
  'c#-efcore-join',
  'GetJoinedCount',
  GetJoinedCount() = (
    SELECT COUNT(*)
    FROM test_entity_framework t
    JOIN test_categories       c ON t.category = c.category
  );

-- 17. DELETE
CREATE OR REPLACE FUNCTION DeleteEntitiesByIds(ids INT[]) RETURNS INT
  AS '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll:EFCoreTest.TestEntityFunctions!DeleteEntitiesByIds'
  LANGUAGE plcsharp STRICT;
INSERT INTO automated_test_results(feature,test_name,result)
SELECT
  'c#-efcore-write-delete-multi',
  'DeleteEntitiesByIds{2,4}',
  DeleteEntitiesByIds(ARRAY[2,4]) = 2
  AND NOT EXISTS (
    SELECT 1 FROM test_entity_framework WHERE id = ANY(ARRAY[2,4])
  );


CREATE OR REPLACE PROCEDURE DeleteFirstEntity()
  AS '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll:EFCoreTest.TestEntityFunctions!DeleteFirstEntity'
  LANGUAGE plcsharp;
-- CALL DeleteFirstEntity();

CREATE OR REPLACE PROCEDURE ModifyCategory(id INTEGER, newCategory TEXT)
  AS '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll:EFCoreTest.TestEntityFunctions!ModifyCategory'
  LANGUAGE plcsharp;
-- CALL ModifyCategory(3, 'Category Added by EFCore');

CREATE OR REPLACE PROCEDURE DeleteEntitiesAbovePrice(priceThreshold MONEY)
  AS '/app/pldotnet/tests/csharp/DotNetTestProject/bin/Release/CSharpTest.dll:EFCoreTest.TestEntityFunctions!DeleteEntitiesAbovePrice'
  LANGUAGE plcsharp;
-- CALL DeleteEntitiesAbovePrice('30.00'::MONEY);
