CREATE OR REPLACE FUNCTION returnBool() RETURNS boolean AS $$
return false;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BOOL', 'returnBool', returnBool() is false;

CREATE OR REPLACE FUNCTION BooleanAnd(a boolean, b boolean) RETURNS boolean AS $$
return a&b;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BOOL', 'BooleanAnd', BooleanAnd(true, true) is true;

CREATE OR REPLACE FUNCTION BooleanOr(a boolean, b boolean) RETURNS boolean AS $$
return a|b;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BOOL', 'BooleanOr', BooleanOr(false, false) is false;

CREATE OR REPLACE FUNCTION BooleanXor(a boolean, b boolean) RETURNS boolean AS $$
return a^b;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BOOL', 'BooleanXor', BooleanXor(false, false) is false;

--- Arrays

CREATE OR REPLACE FUNCTION returnBooleanArray(booleans boolean[]) RETURNS boolean[] AS $$
return booleans;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BOOL[]', 'returnBooleanArray1', returnBooleanArray(ARRAY[true, null::boolean, false, false]) = ARRAY[true, null::boolean, false, false];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BOOL[]', 'returnBooleanArray2', returnBooleanArray(ARRAY[[true, false], [null::boolean, null::boolean]]) = ARRAY[[true, false], [null::boolean, null::boolean]];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BOOL[]', 'returnBooleanArray3', returnBooleanArray(ARRAY[[[true, false], [null::boolean, null::boolean]], [[true, null::boolean], [true, null::boolean]]]) = ARRAY[[[true, false], [null::boolean, null::boolean]], [[true, null::boolean], [true, null::boolean]]];

CREATE OR REPLACE FUNCTION countBool(booleans boolean[], desired boolean) RETURNS Integer AS $$
Array flatten_booleans = Array.CreateInstance(typeof(object), booleans.Length);
array_handler.flatArray(booleans, ref flatten_booleans);
int count = 0;
for(int i = 0; i < flatten_booleans.Length; i++)
{   
    if (flatten_booleans.GetValue(i) == null)
        continue;
    if((bool)flatten_booleans.GetValue(i) == desired)
        count++;
}
return count;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BOOL[]', 'countBool1', countBool(ARRAY[true, true, false, true, null::boolean], true) = integer '3';
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BOOL[]', 'countBool2', countBool(ARRAY[true, true, false, true, null::boolean], false) = integer '1';
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BOOL[]', 'countBool3', countBool(ARRAY[[true, null::boolean, true], [true, false, null::boolean]], true) = integer '3';
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BOOL[]', 'countBool4', countBool(ARRAY[[true, null::boolean, true], [true, false, null::boolean]], false) = integer '1';
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BOOL[]', 'countBool5', countBool(ARRAY[[[true, true, null::boolean], [true, null::boolean, false]], [[null::boolean, true, false], [true, null::boolean, false]]], true) = integer '5';
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BOOL[]', 'countBool6', countBool(ARRAY[[[true, true, null::boolean], [true, null::boolean, false]], [[null::boolean, true, false], [true, null::boolean, false]]], false) = integer '3';

CREATE OR REPLACE FUNCTION CreateBooleanMultidimensionalArray() RETURNS boolean[] AS $$
bool?[, ,] boolean_three_dimensional = new bool?[2, 2, 2] {{{true, false}, {null, null}}, {{false, false}, {true, null}}};
return boolean_three_dimensional;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BOOL[]', 'CreateBooleanMultidimensionalArray', CreateBooleanMultidimensionalArray() = ARRAY[[[true, false], [null::boolean, null::boolean]], [[false, false], [true, null::boolean]]];

CREATE OR REPLACE FUNCTION updateArrayBooleanIndex(booleans boolean[], desired boolean, index integer[]) RETURNS boolean[] AS $$
int[] arrayInteger = index.Cast<int>().ToArray();
booleans.SetValue(desired, arrayInteger);
return booleans;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BOOL[]', 'updateArrayBooleanIndex1', updateArrayBooleanIndex(ARRAY[true, false, true], true, ARRAY[1]) = ARRAY[true, true, true];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BOOL[]', 'updateArrayBooleanIndex2', updateArrayBooleanIndex(ARRAY[[true, false], [true, false]], false, ARRAY[1, 0]) = ARRAY[[true, false], [false, false]];
