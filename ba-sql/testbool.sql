DROP TABLE boolResults;
CREATE TABLE boolResults(testName varchar(255), result boolean);

CREATE OR REPLACE FUNCTION returnBool() RETURNS boolean AS $$
return false;
$$ LANGUAGE plcsharp;
INSERT INTO boolResults (testName, result)
SELECT 'TEST: returnBool', returnBool() is false;

CREATE OR REPLACE FUNCTION BooleanAnd(a boolean, b boolean) RETURNS boolean AS $$
return a&b;
$$ LANGUAGE plcsharp;
INSERT INTO boolResults (testName, result)
SELECT 'TEST: BooleanAnd', BooleanAnd(true, true) is true;

CREATE OR REPLACE FUNCTION BooleanOr(a boolean, b boolean) RETURNS boolean AS $$
return a|b;
$$ LANGUAGE plcsharp;
INSERT INTO boolResults (testName, result)
SELECT 'TEST: BooleanOr', BooleanOr(false, false) is false;

CREATE OR REPLACE FUNCTION BooleanXor(a boolean, b boolean) RETURNS boolean AS $$
return a^b;
$$ LANGUAGE plcsharp;
INSERT INTO boolResults (testName, result)
SELECT 'TEST: BooleanXor', BooleanXor(false, false) is false;

SELECT testName, result from boolResults;