/**************** Null return test functions ***************************/
CREATE OR REPLACE FUNCTION returnNullIntFSharp() RETURNS integer AS $$
None
$$ LANGUAGE plfsharp;
SELECT returnNullIntFSharp() is NULL;

CREATE OR REPLACE FUNCTION returnNullSmallIntFSharp() RETURNS smallint AS $$
None
$$ LANGUAGE plfsharp;
SELECT returnNullSmallIntFSharp() is NULL;

CREATE OR REPLACE FUNCTION returnNullBigIntFSharp() RETURNS bigint AS $$
None
$$ LANGUAGE plfsharp;
SELECT returnNullBigIntFSharp() is NULL;

/**************** Null operations test functions ***************************/
CREATE OR REPLACE FUNCTION sumNullArgIntFSharp(a integer, b integer) RETURNS integer AS $$
match (a, b) with
| Some _a, Some _b -> Some (_a + _b)
| _ -> None
$$ LANGUAGE plfsharp;
SELECT sumNullArgIntFSharp(null,null) is NULL;
SELECT sumNullArgIntFSharp(null,3) is NULL;
SELECT sumNullArgIntFSharp(3,null) is NULL;
SELECT sumNullArgIntFSharp(3,3) = integer '6';

CREATE OR REPLACE FUNCTION sumNullArgSmallIntFSharp(a smallint, b smallint) RETURNS smallint AS $$
match (a, b) with
| Some _a, Some _b -> Some (_a + _b)
| _ -> None
$$ LANGUAGE plfsharp;
SELECT sumNullArgSmallIntFSharp(null,null) is NULL;
SELECT sumNullArgSmallIntFSharp(null,CAST(101 AS smallint)) is NULL;
SELECT sumNullArgSmallIntFSharp(CAST(101 AS smallint),null) is NULL;
SELECT sumNullArgSmallIntFSharp(CAST(101 AS smallint),CAST(101 AS smallint)) = smallint '202';

CREATE OR REPLACE FUNCTION sumNullArgBigIntFSharp(a bigint, b bigint) RETURNS bigint AS $$
match (a, b) with
| Some _a, Some _b -> Some ((int64) _a + _b);
| _ -> None
$$ LANGUAGE plfsharp;
SELECT sumNullArgBigIntFSharp(null,null) is NULL;
SELECT sumNullArgBigIntFSharp(null,100) is NULL;
SELECT sumNullArgBigIntFSharp(9223372036854775707,null) is NULL;
SELECT sumNullArgBigIntFSharp(9223372036854775707,100) = bigint '9223372036854775807';

/**************** Conditional return test functions ***************************/
CREATE OR REPLACE FUNCTION checkedSumNullArgIntFSharp(a integer, b integer) RETURNS integer AS $$
match (a, b) with
| Some _a, Some _b -> Some (_a + _b)
| _ -> None
$$ LANGUAGE plfsharp;
SELECT checkedSumNullArgIntFSharp(null,null) is NULL;
SELECT checkedSumNullArgIntFSharp(null,3) is NULL;
SELECT checkedSumNullArgIntFSharp(3,null) is NULL;
SELECT checkedSumNullArgIntFSharp(3,3) = integer '6';

CREATE OR REPLACE FUNCTION checkedSumNullArgSmallIntFSharp(a smallint, b smallint) RETURNS smallint AS $$
match (a, b) with
| Some _a, Some _b -> Some ((int16) (_a + _b))
| _ -> None
$$ LANGUAGE plfsharp;
SELECT checkedSumNullArgSmallIntFSharp(null,null) is NULL;
SELECT checkedSumNullArgSmallIntFSharp(null,CAST(133 AS smallint)) is NULL;
SELECT checkedSumNullArgSmallIntFSharp(CAST(133 AS smallint),null) is NULL;
SELECT checkedSumNullArgSmallIntFSharp(CAST(133 AS smallint),CAST(133 AS smallint)) = smallint '266';

CREATE OR REPLACE FUNCTION checkedSumNullArgBigIntFSharp(a bigint, b bigint) RETURNS bigint AS $$
match (a, b) with
| Some _a, Some _b -> Some ((int64) _a + _b)
| _ -> None
$$ LANGUAGE plfsharp;
SELECT checkedSumNullArgBigIntFSharp(null,null) is NULL;
SELECT checkedSumNullArgBigIntFSharp(null,100) is NULL;
SELECT checkedSumNullArgBigIntFSharp(9223372036854775707,null) is NULL;
SELECT checkedSumNullArgBigIntFSharp(9223372036854775707,100) = bigint '9223372036854775807';

/**************** Conditional return test functions (Mixed Args)  ***************************/
CREATE OR REPLACE FUNCTION checkedSumNullArgMixedFSharp(a integer, b smallint, c bigint) RETURNS bigint AS $$
match (a, b, c) with
| Some _a, Some _b, Some _c -> Some (((int64) _a) + ((int64) _b) + ((int64) _c))
| _ -> None
$$ LANGUAGE plfsharp;
SELECT checkedSumNullArgMixedFSharp(null,null,null) is NULL;
SELECT checkedSumNullArgMixedFSharp(null,CAST(1313 as smallint),null) is NULL;
SELECT checkedSumNullArgMixedFSharp(1313,null,null) is NULL;
SELECT checkedSumNullArgMixedFSharp(null,null,3) is NULL;
SELECT checkedSumNullArgMixedFSharp(1313,CAST(1313 as smallint), 1313) = smallint '3939';

