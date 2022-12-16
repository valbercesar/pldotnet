CREATE OR REPLACE FUNCTION maxSmallIntPython() RETURNS smallint AS $$
return 32767
$$ LANGUAGE plpython3u;
SELECT maxSmallIntPython() = integer '32767';

CREATE OR REPLACE FUNCTION sum2SmallIntPython(a smallint, b smallint) RETURNS smallint AS $$
return a + b
$$ LANGUAGE plpython3u;
SELECT sum2SmallIntPython(CAST(100 AS smallint), CAST(101 AS smallint)) = smallint '201';

CREATE OR REPLACE FUNCTION maxIntegerPython() RETURNS integer AS $$
return 2147483647
$$ LANGUAGE plpython3u;
SELECT maxIntegerPython() = integer '2147483647';

CREATE OR REPLACE FUNCTION sum2IntegerPython(a integer, b integer) RETURNS integer AS $$
return a + b
$$ LANGUAGE plpython3u;
SELECT sum2IntegerPython(32770, 100) = bigint '32870';

CREATE OR REPLACE FUNCTION maxBigIntPython() RETURNS bigint AS $$
return 9223372036854775807
$$ LANGUAGE plpython3u;
SELECT maxBigIntPython() = bigint '9223372036854775807';

CREATE OR REPLACE FUNCTION sum2BigIntPython(a bigint, b bigint) RETURNS bigint AS $$
return a + b
$$ LANGUAGE plpython3u;
SELECT sum2BigIntPython(9223372036854775707, 100) = bigint '9223372036854775807';

CREATE OR REPLACE FUNCTION mixedBigIntPython(a integer, b integer, c bigint) RETURNS bigint AS $$
return a + b + c
$$ LANGUAGE plpython3u;
SELECT mixedBigIntPython(32767,  2147483647, 100) = bigint '2147516514';

CREATE OR REPLACE FUNCTION mixedIntPython(a smallint, b smallint, c integer) RETURNS integer AS $$
return a + b + c
$$ LANGUAGE plpython3u;
SELECT mixedIntPython(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100) = integer '65634';
