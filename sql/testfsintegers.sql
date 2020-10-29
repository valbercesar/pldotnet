CREATE OR REPLACE FUNCTION maxSmallIntFsharp() RETURNS smallint AS $$
32767s
$$ LANGUAGE plfsharp;
SELECT maxSmallIntFsharp() = integer '32767';

CREATE OR REPLACE FUNCTION sum2SmallIntFsharp(a smallint, b smallint) RETURNS smallint AS $$
(int16)(a+b) // -- F# requires int16 cast
$$ LANGUAGE plfsharp;
SELECT sum2SmallIntFsharp(CAST(100 AS smallint), CAST(101 AS smallint)) = smallint '201';

CREATE OR REPLACE FUNCTION maxIntegerFsharp() RETURNS integer AS $$
2147483647
$$ LANGUAGE plfsharp;
SELECT maxIntegerFsharp() = integer '2147483647';

CREATE OR REPLACE FUNCTION sum2IntegerFsharp(a integer, b integer) RETURNS integer AS $$
a+b
$$ LANGUAGE plfsharp;
SELECT sum2IntegerFsharp(32770, 100) = int '32870';

CREATE OR REPLACE FUNCTION maxBigIntFsharp() RETURNS bigint AS $$
9223372036854775807L
$$ LANGUAGE plfsharp;
SELECT maxBigIntFsharp() = bigint '9223372036854775807';

CREATE OR REPLACE FUNCTION sum2BigIntFsharp(a bigint, b bigint) RETURNS bigint AS $$
(int64) a + b
$$ LANGUAGE plfsharp;
SELECT sum2BigIntFsharp(9223372036854775707, 100) = bigint '9223372036854775807';

CREATE OR REPLACE FUNCTION mixedBigIntFsharp(a integer, b integer, c bigint) RETURNS bigint AS $$
((int64) a) + ((int64) b) + c;
$$ LANGUAGE plfsharp;
SELECT mixedBigIntFsharp(32767,  2147483647, 100) = bigint '2147516514';

CREATE OR REPLACE FUNCTION mixedIntFsharp(a smallint, b smallint, c integer) RETURNS integer AS $$
(int)a + ((int) b) + c;
$$ LANGUAGE plfsharp;
SELECT mixedIntFsharp(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100) = integer '65634';

CREATE OR REPLACE FUNCTION mixedBigInt16Fsharp(b smallint, c bigint) RETURNS bigint AS $$
(int64) (int64 b) + c
$$ LANGUAGE plfsharp;
SELECT mixedBigInt16Fsharp(CAST(32 AS SMALLINT), CAST(100 AS BIGINT)) = bigint '132';

CREATE OR REPLACE FUNCTION mixedBigInt16SmallFsharp(b smallint, c bigint) RETURNS smallint AS $$
(int16) ((int64 b) + c)
$$ LANGUAGE plfsharp;
SELECT mixedBigInt16SmallFsharp(CAST(32 AS SMALLINT), CAST(100 AS BIGINT)) = smallint '132';
