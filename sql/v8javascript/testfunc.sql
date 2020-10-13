CREATE OR REPLACE FUNCTION returnX() RETURNS integer AS $$
return 10;
$$ LANGUAGE plv8;
SELECT returnX() = integer '10';

CREATE OR REPLACE FUNCTION inc2(val integer) RETURNS integer AS $$
return val + 2;
$$
LANGUAGE plv8;
SELECT inc2(8) = integer '10';

CREATE OR REPLACE FUNCTION sum2(a integer, b integer) RETURNS integer AS $$
return a + b;
$$
LANGUAGE plv8;
SELECT sum2(3,2) = integer '5';

CREATE OR REPLACE FUNCTION sum3(aaa integer, bbb integer, ccc integer) RETURNS integer AS $$
return aaa + bbb + ccc;
$$
LANGUAGE plv8;
SELECT sum3(3,2,1) = integer '6';

CREATE OR REPLACE FUNCTION sum4(a integer, b integer, c integer, d integer) RETURNS integer AS $$
return a + b + c + d;
$$
LANGUAGE plv8;
SELECT sum4(4,3,2,1) = integer '10';