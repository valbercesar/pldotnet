DROP TABLE floatResults;
CREATE TABLE floatResults(testName varchar(255), result boolean);

-- Float4 (real): 6 digits of precison
CREATE OR REPLACE FUNCTION returnReal() RETURNS real AS $$
return 1.50055f;
$$ LANGUAGE plcsharp;
INSERT INTO floatResults (testName, result)
SELECT 'TEST: returnReal', returnReal() = real '1.50055';

CREATE OR REPLACE FUNCTION sumReal(a real, b real) RETURNS real AS $$
return a+b;
$$ LANGUAGE plcsharp;
INSERT INTO floatResults (testName, result)
SELECT 'TEST: sumReal', sumReal(1.50055, 1.50054) = real '3.00109'; -- 3.00109

--- Float8 (double precision): 15 digits of precison
CREATE OR REPLACE FUNCTION returnDouble() RETURNS double precision AS $$
return 11.0050000000005;
$$ LANGUAGE plcsharp;
INSERT INTO floatResults (testName, result)
SELECT 'TEST: returnDouble', returnDouble() = double precision '11.0050000000005';

CREATE OR REPLACE FUNCTION sumDouble(a double precision, b double precision) RETURNS double precision AS $$
return a+b;
$$ LANGUAGE plcsharp;
INSERT INTO floatResults (testName, result)
SELECT 'TEST: sumDouble', sumDouble(10.5000000000055, 10.5000000000054) = double precision  '21.0000000000109'; -- 21.0000000000109

select testName, result from floatResults;
