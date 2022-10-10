CREATE OR REPLACE FUNCTION returnBool() RETURNS boolean AS $$
return false;
$$ LANGUAGE plcsharp;
INSERT INTO results (testName, result)
SELECT 'TEST BOOL: returnBool', returnBool() is false;

CREATE OR REPLACE FUNCTION BooleanAnd(a boolean, b boolean) RETURNS boolean AS $$
return a&b;
$$ LANGUAGE plcsharp;
INSERT INTO results (testName, result)
SELECT 'TEST BOOL: BooleanAnd', BooleanAnd(true, true) is true;

CREATE OR REPLACE FUNCTION BooleanOr(a boolean, b boolean) RETURNS boolean AS $$
return a|b;
$$ LANGUAGE plcsharp;
INSERT INTO results (testName, result)
SELECT 'TEST BOOL: BooleanOr', BooleanOr(false, false) is false;

CREATE OR REPLACE FUNCTION BooleanXor(a boolean, b boolean) RETURNS boolean AS $$
return a^b;
$$ LANGUAGE plcsharp;
INSERT INTO results (testName, result)
SELECT 'TEST BOOL: BooleanXor', BooleanXor(false, false) is false;