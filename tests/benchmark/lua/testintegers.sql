CREATE OR REPLACE FUNCTION maxSmallIntLua() RETURNS smallint AS $$
return 32767
$$ LANGUAGE pllua;
SELECT maxSmallIntLua() = integer '32767';

CREATE OR REPLACE FUNCTION sum2SmallIntLua(a smallint, b smallint) RETURNS smallint AS $$
return a + b
$$ LANGUAGE pllua;
SELECT sum2SmallIntLua(CAST(100 AS smallint), CAST(101 AS smallint)) = smallint '201';

CREATE OR REPLACE FUNCTION maxIntegerLua() RETURNS integer AS $$
return 2147483647
$$ LANGUAGE pllua;
SELECT maxIntegerLua() = integer '2147483647';

CREATE OR REPLACE FUNCTION sum2IntegerLua(a integer, b integer) RETURNS integer AS $$
return a + b
$$ LANGUAGE pllua;
SELECT sum2IntegerLua(32770, 100) = bigint '32870';

CREATE OR REPLACE FUNCTION maxBigIntLua() RETURNS bigint AS $$
return 9223372036854775807
$$ LANGUAGE pllua;
SELECT maxBigIntLua() = bigint '9223372036854775807';

CREATE OR REPLACE FUNCTION sum2BigIntLua(a bigint, b bigint) RETURNS bigint AS $$
return a + b
$$ LANGUAGE pllua;
SELECT sum2BigIntLua(9223372036854775707, 100) = bigint '9223372036854775807';

CREATE OR REPLACE FUNCTION mixedBigIntLua(a integer, b integer, c bigint) RETURNS bigint AS $$
return a + b + c
$$ LANGUAGE pllua;
SELECT mixedBigIntLua(32767,  2147483647, 100) = bigint '2147516514';

CREATE OR REPLACE FUNCTION mixedIntLua(a smallint, b smallint, c integer) RETURNS integer AS $$
return a + b + c
$$ LANGUAGE pllua;
SELECT mixedIntLua(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100) = integer '65634';
