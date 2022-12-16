CREATE OR REPLACE FUNCTION returnXV8() RETURNS integer AS $$
return 10;
$$ LANGUAGE plv8;
SELECT returnXV8() = integer '10';

CREATE OR REPLACE FUNCTION inc2V8(val integer) RETURNS integer AS $$
return val + 2;
$$
LANGUAGE plv8;
SELECT inc2V8(8) = integer '10';

CREATE OR REPLACE FUNCTION sum2V8(a integer, b integer) RETURNS integer AS $$
return a + b;
$$
LANGUAGE plv8;
SELECT sum2V8(3,2) = integer '5';

CREATE OR REPLACE FUNCTION sum3V8(aaa integer, bbb integer, ccc integer) RETURNS integer AS $$
return aaa + bbb + ccc;
$$
LANGUAGE plv8;
SELECT sum3V8(3,2,1) = integer '6';

CREATE OR REPLACE FUNCTION sum4V8(a integer, b integer, c integer, d integer) RETURNS integer AS $$
return a + b + c + d;
$$
LANGUAGE plv8;
SELECT sum4V8(4,3,2,1) = integer '10';