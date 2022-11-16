INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INTEGER', 'fibonacci2', fibonacci(3) = integer '2';
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INTEGER', 'fibonacci3', fibonacci(24) = integer '46368';
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INTEGER', 'fibonacci1', fibonacci(30) = integer '832040';

INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MONEY[]', 'updateMoneyArray1', updateMoneyArray(ARRAY['32500.0'::MONEY, '-500.4'::MONEY, null::MONEY, '900540.2'::MONEY], '1390540.2'::MONEY, ARRAY[2]) = ARRAY['32500.0'::MONEY, '-500.4'::MONEY, '1390540.2'::MONEY, '900540.2'::MONEY];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MONEY[]', 'updateMoneyArray2', updateMoneyArray(ARRAY[['32500.0'::MONEY, '-500.4'::MONEY], [null::MONEY, null::MONEY]], '1390540.2'::MONEY, ARRAY[1,0]) = ARRAY[['32500.0'::MONEY, '-500.4'::MONEY], ['1390540.2'::MONEY, null::MONEY]];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MONEY[]', 'updateMoneyArray3', updateMoneyArray(ARRAY[[null::MONEY, null::MONEY], [null::MONEY, null::MONEY]], '1390540.2'::MONEY, ARRAY[1,0]) = ARRAY[[null::MONEY, null::MONEY], ['1390540.2'::MONEY, null::MONEY]];
