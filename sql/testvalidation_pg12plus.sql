CREATE OR REPLACE FUNCTION returnInvalidBool() RETURNS boolean AS $$
return True;
$$ LANGUAGE plcsharp;
SELECT returnInvalidBool() is true;

CREATE OR REPLACE FUNCTION returnValidBool() RETURNS boolean AS $$
return true;
$$ LANGUAGE plcsharp;
SELECT returnValidBool() is true;

CREATE OR REPLACE FUNCTION returnInvalidVoid() RETURNS boolean AS $$
return;
$$ LANGUAGE plcsharp;
SELECT returnInvalidVoid() is true;

CREATE OR REPLACE FUNCTION returnInvalidBody() RETURNS boolean AS $$
$$ LANGUAGE plcsharp;
SELECT returnInvalidBody() is true;

CREATE OR REPLACE FUNCTION returnMissing() RETURNS boolean AS $$
var a = true;
var b = 2;
$$ LANGUAGE plcsharp;
SELECT returnMissing() is true;

CREATE OR REPLACE FUNCTION returnInvalidReturnType(a integer, b integer) RETURNS integer AS $$
var c = a + b;
return Convert.ToString(c);
$$ LANGUAGE plcsharp;
SELECT returnInvalidReturnType() is '10';

CREATE OR REPLACE FUNCTION returnValidReturnType(a integer, b integer) RETURNS text AS $$
var c = a + b;
return Convert.ToString(c);
$$ LANGUAGE plcsharp;
SELECT returnValidReturnType(10::integer, 20::integer) = 30::integer;

CREATE OR REPLACE FUNCTION returnMissingComma(a integer, b integer) RETURNS text AS $$
var c = a + b
return Convert.ToString(c);
$$ LANGUAGE plcsharp;
SELECT returnMissingComma(10::integer, 20::integer) = 30::integer;

CREATE OR REPLACE FUNCTION returnInvalidOperation(a text, b double precision) RETURNS double precision AS $$
return Math.Sqrt(a) + b;
$$ LANGUAGE plcsharp;
SELECT returnInvalidOperation('9', 20::double precision) = 23::double precision;

CREATE OR REPLACE FUNCTION returnValidOperation(a text, b double precision) RETURNS double precision AS $$
return Math.Sqrt(Convert.ToDouble(a)) + b;
$$ LANGUAGE plcsharp;
SELECT returnValidOperation('9', 20::double precision) = 23::double precision;

CREATE OR REPLACE FUNCTION returnUnknownName(a varchar, b integer) RETURNS varchar AS $$
return append_integer_to_string(a, b);
$$ LANGUAGE plcsharp;
SELECT returnUnknownName('this is the integer: ', 123::integer) = 'this is the integer: 123';

CREATE OR REPLACE FUNCTION returnKnownName(a varchar, b integer) RETURNS varchar AS $$
string append_integer_to_string(string _a, int? _b)
{
    var v = _b.GetValueOrDefault();
    return String.Concat(_a,  _b.HasValue ? Convert.ToString(v) : "");
}
return append_integer_to_string(a, b);
$$ LANGUAGE plcsharp;
SELECT returnKnownName('this is the integer: ', 123::integer) = 'this is the integer: 123';
