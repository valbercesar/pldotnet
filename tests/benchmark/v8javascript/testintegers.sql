CREATE OR REPLACE FUNCTION maxSmallIntV8() RETURNS smallint AS $$
return 32767;
$$ LANGUAGE plv8;
SELECT maxSmallIntV8() = integer '32767';

CREATE OR REPLACE FUNCTION sum2SmallIntV8(a smallint, b smallint) RETURNS smallint AS $$
return (a+b);
$$ LANGUAGE plv8;
SELECT sum2SmallIntV8(CAST(100 AS smallint), CAST(101 AS smallint)) = smallint '201';

CREATE OR REPLACE FUNCTION maxIntegerV8() RETURNS integer AS $$
return 2147483647;
$$ LANGUAGE plv8;
SELECT maxIntegerV8() = integer '2147483647';

CREATE OR REPLACE FUNCTION sum2IntegerV8(a integer, b integer) RETURNS integer AS $$
return a+b;
$$ LANGUAGE plv8;
SELECT sum2IntegerV8(32770, 100) = bigint '32870';

CREATE OR REPLACE FUNCTION maxBigIntV8() RETURNS bigint AS $$
return 9223372036854775807;
$$ LANGUAGE plv8;
SELECT maxBigIntV8() = bigint '9223372036854775807';

CREATE OR REPLACE FUNCTION sum2BigIntV8(a bigint, b bigint) RETURNS bigint AS $$
return a+b;
$$ LANGUAGE plv8;
SELECT sum2BigIntV8(9223372036854775707, 100) = bigint '9223372036854775807';

CREATE OR REPLACE FUNCTION mixedBigIntV8(a integer, b bigint, c bigint) RETURNS bigint AS $$
return BigInt(a)+b+c;
$$ LANGUAGE plv8;
SELECT mixedBigIntV8(32767,  CAST(2147483647 as bigint), CAST(100 as bigint)) = bigint '2147516514';

CREATE OR REPLACE FUNCTION mixedIntV8(a smallint, b smallint, c integer) RETURNS integer AS $$
return Number(a)+Number(b)+c;
$$ LANGUAGE plv8;
SELECT mixedIntV8(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100) = integer '65634';
