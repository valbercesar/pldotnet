CREATE OR REPLACE FUNCTION returnXPython() RETURNS integer AS $$
return 10
$$ LANGUAGE plpython3u;
SELECT returnXPython() = integer '10';

CREATE OR REPLACE FUNCTION inc2Python(val integer) RETURNS integer AS $$
return val + 2
$$ LANGUAGE plpython3u;
SELECT inc2Python(8) = integer '10';

CREATE OR REPLACE FUNCTION sum2Python(a integer, b integer) RETURNS integer AS $$
return a + b
$$ LANGUAGE plpython3u;
SELECT sum2Python(3,2) = integer '5';

CREATE OR REPLACE FUNCTION sum3Python(aaa integer, bbb integer, ccc integer) RETURNS integer AS $$
return aaa + bbb + ccc
$$
LANGUAGE plpython3u;
SELECT sum3Python(3,2,1) = integer '6';

CREATE OR REPLACE FUNCTION sum4Python(a integer, b integer, c integer, d integer) RETURNS integer AS $$
return a + b + c + d
$$ LANGUAGE plpython3u;
SELECT sum4Python(4,3,2,1) = integer '10';
