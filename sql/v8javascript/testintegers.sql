CREATE OR REPLACE FUNCTION maxSmallInt() RETURNS smallint AS $$
return 32767;
$$ LANGUAGE plv8;
SELECT maxSmallInt() = integer '32767';

CREATE OR REPLACE FUNCTION sum2SmallInt(a smallint, b smallint) RETURNS smallint AS $$
return (a+b);
$$ LANGUAGE plv8;
SELECT sum2SmallInt(CAST(100 AS smallint), CAST(101 AS smallint)) = smallint '201';

CREATE OR REPLACE FUNCTION maxInteger() RETURNS integer AS $$
return 2147483647;
$$ LANGUAGE plv8;
SELECT maxInteger() = integer '2147483647';

CREATE OR REPLACE FUNCTION sum2Integer(a integer, b integer) RETURNS integer AS $$
return a+b;
$$ LANGUAGE plv8;
SELECT sum2Integer(32770, 100) = bigint '32870';

CREATE OR REPLACE FUNCTION maxBigInt() RETURNS bigint AS $$
return 9223372036854775807;
$$ LANGUAGE plv8;
SELECT maxBigInt() = bigint '9223372036854775807';

CREATE OR REPLACE FUNCTION sum2BigInt(a bigint, b bigint) RETURNS bigint AS $$
return a+b;
$$ LANGUAGE plv8;
SELECT sum2BigInt(9223372036854775707, 100) = bigint '9223372036854775807';

CREATE OR REPLACE FUNCTION mixedBigInt(a integer, b integer, c bigint) RETURNS bigint AS $$
return a+b+c;
$$ LANGUAGE plv8;
SELECT mixedBigInt(32767,  2147483647, 100) = bigint '2147516514';

CREATE OR REPLACE FUNCTION mixedInt(a smallint, b smallint, c integer) RETURNS integer AS $$
return a+b+c;
$$ LANGUAGE plv8;
SELECT mixedInt(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100) = integer '65634';
