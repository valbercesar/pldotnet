-- MONEY

CREATE OR REPLACE FUNCTION computeNewSalary(salary MONEY, rate FLOAT8) RETURNS MONEY AS $$
    decimal aux = (decimal)(1.0+rate);
    return (decimal)salary*aux;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MONEY', 'computeNewSalary', computeNewSalary('32500'::MONEY, 0.059875) = '34445.9375'::MONEY;

CREATE OR REPLACE FUNCTION returnMaxMoney() RETURNS MONEY AS $$
    decimal value = 92233720368547758.07M;
    return value;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MONEY', 'returnMaxMoney', returnMaxMoney() = '92233720368547758.07'::MONEY;

CREATE OR REPLACE FUNCTION returnMinMoney() RETURNS MONEY AS $$
    decimal value = -92233720368547758.08M;
    return value;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MONEY', 'returnMinMoney', returnMinMoney() = '-92233720368547758.08'::MONEY;

-- NULL

CREATE OR REPLACE FUNCTION returnMoney(salary MONEY, bonus MONEY, discounts MONEY) RETURNS MONEY AS $$
    decimal s = salary == null ? 0.0M : (decimal)salary;
    decimal b = bonus == null ? 0.0M : (decimal)bonus;
    decimal d = discounts == null ? 0.0M : (decimal)discounts;
    return s+b-d;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MONEY', 'returnMoney1', returnMoney('32500.0'::MONEY, '1556.25'::MONEY, '899.99'::MONEY) = '33156.26'::MONEY;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MONEY', 'returnMoney2', returnMoney('13525.21'::MONEY, NULL::MONEY, '899.99'::MONEY) = '12625.22'::MONEY;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MONEY', 'returnMoney3', returnMoney(NULL::MONEY, NULL::MONEY, NULL::MONEY) = '0'::MONEY;
