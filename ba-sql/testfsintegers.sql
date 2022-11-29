--- SMALLINT
CREATE OR REPLACE FUNCTION maxSmallIntFSharp() RETURNS int2 AS $$
Nullable (32767s)
$$ LANGUAGE plfsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'f#-int2', 'maxSmallIntFSharp', maxSmallIntFSharp() = int2 '32767';

CREATE OR REPLACE FUNCTION sum2SmallIntFSharp(a int2, b int2) RETURNS int2 AS $$
Nullable (a+b)
$$ LANGUAGE plfsharp STRICT;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'f#-int2', 'sum2SmallIntFSharp', sum2SmallIntFSharp(CAST(32760 AS int2), CAST(7 AS int2)) = SMALLINT '32767';

--- INTEGER
CREATE OR REPLACE FUNCTION maxIntegerFSharp() RETURNS int4 AS $$
Nullable 2147483647
$$ LANGUAGE plfsharp STRICT;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'f#-int4', 'maxIntegerFSharp', maxIntegerFSharp() = int4 '2147483647';

CREATE OR REPLACE FUNCTION multIntegersFSharp(a int4, b int4) RETURNS int4 AS $$
Nullable (a*b)
$$ LANGUAGE plfsharp STRICT;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'f#-int4', 'multIntegersFSharp', multIntegersFSharp(15, 15) = int4 '225';
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'f#-int4', 'multIntegersFSharp', multIntegersFSharp(50, 75) = int4 '3750';

--- BIGINT
CREATE OR REPLACE FUNCTION maxBigIntFSharp() RETURNS int8 AS $$
Nullable 9223372036854775807L
$$ LANGUAGE plfsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'f#-int8', 'maxBigIntFSharp', maxBigIntFSharp() = int8 '9223372036854775807';

CREATE OR REPLACE FUNCTION mixedBigIntFSharp(a int2, b int4, c int8) RETURNS int8 AS $$
Nullable ((int64)a + (int64)b + c)
$$ LANGUAGE plfsharp STRICT;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'f#-int8', 'mixedBigIntFSharp', mixedBigIntFSharp('32767'::int2,  '2147483647'::int4, '100'::int8) = int8 '2147516514';
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'f#-int8', 'mixedBigIntFSharp', mixedBigIntFSharp('32767'::int2,  '2147483647'::int4, '2147483647'::int8) = int8 '4295000061';

CREATE OR REPLACE FUNCTION mixedBigIntFSharp2(a int2, b int4, c int8) RETURNS int8 AS $$
Nullable ((int64)a.Value * (int64)b.Value * c.Value)
$$ LANGUAGE plfsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'f#-int8', 'mixedBigIntFSharp2', mixedBigIntFSharp2('32767'::int2,  '550'::int4, '200'::int8) = '3604370000'::int8;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'f#-int8', 'mixedBigIntFSharp2', mixedBigIntFSharp2('32767'::int2,  '5500'::int4, '500'::int8) = int8 '90109250000';

--- SMALLINT[]
CREATE OR REPLACE FUNCTION returnSmallIntArrayFSharp(small_integers int2[]) RETURNS int2[] AS $$
small_integers
$$ LANGUAGE plfsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'f#-int2-null-1array', 'returnSmallIntArrayFSharp1', returnSmallIntArrayFSharp(ARRAY[12345::int2, null::int2, 123::int2, 4356::int2]) = ARRAY[12345::int2, null::int2, 123::int2, 4356::int2];
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'f#-int2-null-2array-arraynull', 'returnSmallIntArrayFSharp2', returnSmallIntArrayFSharp(ARRAY[[null::int2, null::int2], [12345::int2, 654::int2]]) = ARRAY[[null::int2, null::int2], [12345::int2, 654::int2]];
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'f#-int2-null-3array-arraynull', 'returnSmallIntArrayFSharp3', returnSmallIntArrayFSharp(ARRAY[[[null::int2, null::int2], [null::int2, null::int2]], [[186::int2, 23823::int2], [9521::int2, 934::int2]]]) = ARRAY[[[null::int2, null::int2], [null::int2, null::int2]], [[186::int2, 23823::int2], [9521::int2, 934::int2]]];

CREATE OR REPLACE FUNCTION CreateSmallIntMultidimensionalArrayFSharp() RETURNS int2[] AS $$
let arr = Array.CreateInstance(typeof<int16>, 3, 3)
arr.SetValue((int16)1, 0, 0)
arr.SetValue((int16)1, 1, 1)
arr.SetValue((int16)1, 2, 2)
arr
$$ LANGUAGE plfsharp STRICT;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'f#-int2-2array', 'CreateSmallIntMultidimensionalArrayFSharp', CreateSmallIntMultidimensionalArrayFSharp() = ARRAY[['1'::int2,'0'::int2,'0'::int2], ['0'::int2, '1'::int2, '0'::int2], ['0'::int2, '0'::int2, '1'::int2]];

--- INTEGER[]
CREATE OR REPLACE FUNCTION modify1DArrayFSharp(integers int4[], new_value int2) RETURNS int4[] AS $$
integers.SetValue((int)new_value, 0)
integers
$$ LANGUAGE plfsharp STRICT;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'f#-int4-null-1array', 'modify1DArrayFSharp1', modify1DArrayFSharp(ARRAY[2047483647::int4, null::int4, 304325::int4, 4356::int4], '250'::int2) = ARRAY[250::int4, null::int4, 304325::int4, 4356::int4];
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'f#-int4-null-1array', 'modify1DArrayFSharp2', modify1DArrayFSharp(ARRAY[null::int4, null::int4, 2047483647::int4, 304325::int4], '32767'::int2) = ARRAY[32767::int4, null::int4, 2047483647::int4, 304325::int4];

--- BIGINT[]
CREATE OR REPLACE FUNCTION modify2DArrayFSharp(integers int8[], new_value int2) RETURNS int8[] AS $$
integers.SetValue((int64)new_value, 0, 0)
integers
$$ LANGUAGE plfsharp STRICT;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'f#-int8-null-2array', 'modify2DArrayFSharp1', modify2DArrayFSharp(ARRAY[[null::int8, null::int8], [2047483647::int8, 304325::int8]], '250'::int2) = ARRAY[[250::int8, null::int8], [2047483647::int8, 304325::int8]];
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'f#-int8-null-2array', 'modify2DArrayFSharp2', modify2DArrayFSharp(ARRAY[[2047483647::int8, 304325::int8], [null::int8, 12465464::int8]], '32767'::int2) = ARRAY[[32767::int8, 304325::int8], [null::int8, 12465464::int8]];
