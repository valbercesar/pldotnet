CREATE OR REPLACE FUNCTION returnBool() RETURNS boolean AS $$
return false;
$$ LANGUAGE plv8;
SELECT returnBool() is false;

CREATE OR REPLACE FUNCTION BooleanAnd(a boolean, b boolean) RETURNS boolean AS $$
return a&b;
$$ LANGUAGE plv8;
SELECT BooleanAnd(true, true) is true;

CREATE OR REPLACE FUNCTION BooleanOr(a boolean, b boolean) RETURNS boolean AS $$
return a||b;
$$ LANGUAGE plv8;
SELECT BooleanOr(false, false) is false;

CREATE OR REPLACE FUNCTION BooleanXor(a boolean, b boolean) RETURNS boolean AS $$
return a^b;
$$ LANGUAGE plv8;
SELECT BooleanXor(false, false) is false;
