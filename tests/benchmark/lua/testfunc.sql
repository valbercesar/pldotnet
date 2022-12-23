CREATE OR REPLACE FUNCTION returnXLua() RETURNS integer AS $$
return 10
$$ LANGUAGE pllua;
SELECT returnXLua() = integer '10';

CREATE OR REPLACE FUNCTION inc2Lua(val integer) RETURNS integer AS $$
return val + 2
$$ LANGUAGE pllua;
SELECT inc2Lua(8) = integer '10';

CREATE OR REPLACE FUNCTION sum2Lua(a integer, b integer) RETURNS integer AS $$
return a + b
$$ LANGUAGE pllua;
SELECT sum2Lua(3,2) = integer '5';

CREATE OR REPLACE FUNCTION sum3Lua(aaa integer, bbb integer, ccc integer) RETURNS integer AS $$
return aaa + bbb + ccc
$$
LANGUAGE pllua;
SELECT sum3Lua(3,2,1) = integer '6';

CREATE OR REPLACE FUNCTION sum4Lua(a integer, b integer, c integer, d integer) RETURNS integer AS $$
return a + b + c + d
$$ LANGUAGE pllua;
SELECT sum4Lua(4,3,2,1) = integer '10';
