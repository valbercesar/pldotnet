CREATE OR REPLACE FUNCTION maxSmallIntFsharp() RETURNS smallint AS $$
Some 32767s
$$ LANGUAGE plfsharp;
SELECT maxSmallIntFsharp() = integer '32767';

CREATE OR REPLACE FUNCTION sum2SmallIntFsharp(a smallint, b smallint) RETURNS smallint AS $$
match (a, b) with
| (Some _a, Some _b) -> Some ((int16) (_a + _b))
| _ -> None
$$ LANGUAGE plfsharp;
SELECT sum2SmallIntFsharp(CAST(100 AS smallint), CAST(101 AS smallint)) = smallint '201';

CREATE OR REPLACE FUNCTION maxIntegerFsharp() RETURNS integer AS $$
Some 2147483647
$$ LANGUAGE plfsharp;
SELECT maxIntegerFsharp() = integer '2147483647';

CREATE OR REPLACE FUNCTION sum2IntegerFsharp(a integer, b integer) RETURNS integer AS $$
match (a, b) with
| Some _a, Some _b -> Some (_a + _b)
| _ -> None
$$ LANGUAGE plfsharp;
SELECT sum2IntegerFsharp(32770, 100) = int '32870';

CREATE OR REPLACE FUNCTION maxBigIntFsharp() RETURNS bigint AS $$
Some 9223372036854775807L
$$ LANGUAGE plfsharp;
SELECT maxBigIntFsharp() = bigint '9223372036854775807';

CREATE OR REPLACE FUNCTION sum2BigIntFsharp(a bigint, b bigint) RETURNS bigint AS $$
match (a, b) with
| Some _a, Some _b -> Some ((int64) _a + _b)
| _ -> None
$$ LANGUAGE plfsharp;
SELECT sum2BigIntFsharp(9223372036854775707, 100) = bigint '9223372036854775807';

CREATE OR REPLACE FUNCTION mixedBigIntFsharp(a integer, b integer, c bigint) RETURNS bigint AS $$
match (a, b, c) with
| Some _a, Some _b, Some _c -> Some (((int64) _a) + ((int64) _b) + _c)
| _ -> None
$$ LANGUAGE plfsharp;
SELECT mixedBigIntFsharp(32767,  2147483647, 100) = bigint '2147516514';

CREATE OR REPLACE FUNCTION mixedIntFsharp(a smallint, b smallint, c integer) RETURNS integer AS $$
match (a, b, c) with
| Some _a, Some _b, Some _c -> Some ((int) _a + ((int) _b) + _c)
| _ -> None
$$ LANGUAGE plfsharp;
SELECT mixedIntFsharp(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100) = integer '65634';

CREATE OR REPLACE FUNCTION mixedBigInt16Fsharp(b smallint, c bigint) RETURNS bigint AS $$
match (b, c) with
| Some _b, Some _c -> Some ((int64) (int64 _b) + _c)
| _ -> None
$$ LANGUAGE plfsharp;
SELECT mixedBigInt16Fsharp(CAST(32 AS SMALLINT), CAST(100 AS BIGINT)) = bigint '132';

CREATE OR REPLACE FUNCTION mixedBigInt16SmallFsharp(b smallint, c bigint) RETURNS smallint AS $$
match (b, c) with
| Some _b, Some _c -> Some ((int16) ((int64 _b) + _c))
| _ -> None
$$ LANGUAGE plfsharp;
SELECT mixedBigInt16SmallFsharp(CAST(32 AS SMALLINT), CAST(100 AS BIGINT)) = smallint '132';
