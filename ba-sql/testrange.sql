--- INT4RANGE

CREATE OR REPLACE FUNCTION IncreaseInt4Range(orig_value INT4RANGE, increment_value INTEGER) RETURNS INT4RANGE AS $$
return new NpgsqlRange<int>(orig_value.LowerBound + increment_value, orig_value.LowerBoundIsInclusive, orig_value.LowerBoundInfinite, orig_value.UpperBound + increment_value, orig_value.UpperBoundIsInclusive, orig_value.UpperBoundInfinite);
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INT4RANGE[]', 'IncreaseInt4Range1', IncreaseInt4Range('[2,6)'::INT4RANGE, 1) = '[3,7)'::INT4RANGE;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INT4RANGE[]', 'IncreaseInt4Range2', IncreaseInt4Range('[,87)'::INT4RANGE, 3) = '(,90)'::INT4RANGE;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INT4RANGE[]', 'IncreaseInt4Range3', IncreaseInt4Range('[,)'::INT4RANGE, 3) = '(,)'::INT4RANGE;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INT4RANGE[]', 'IncreaseInt4Range4', IncreaseInt4Range('(-2147483648,2147483644)'::INT4RANGE, 3) = '[-2147483644,2147483647)'::INT4RANGE;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INT4RANGE[]', 'IncreaseInt4Range5', IncreaseInt4Range('(-456,-123]'::INT4RANGE, 1) = '[-454,-121)'::INT4RANGE;

--- INT8RANGE

CREATE OR REPLACE FUNCTION IncreaseInt8Range(orig_value INT8RANGE, increment_value integer) RETURNS INT8RANGE AS $$
return new NpgsqlRange<long>(orig_value.LowerBound + increment_value, orig_value.LowerBoundIsInclusive, orig_value.LowerBoundInfinite, orig_value.UpperBound + increment_value, orig_value.UpperBoundIsInclusive, orig_value.UpperBoundInfinite);
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INT8RANGE[]', 'IncreaseInt8Range1', IncreaseInt8Range('[2,6)'::INT8RANGE, 1) = '[3,7)'::INT8RANGE;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INT8RANGE[]', 'IncreaseInt8Range2', IncreaseInt8Range('[,87)'::INT8RANGE, 3) = '(,90)'::INT8RANGE;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INT8RANGE[]', 'IncreaseInt8Range3', IncreaseInt8Range('[,)'::INT8RANGE, 3) = '(,)'::INT8RANGE;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INT8RANGE[]', 'IncreaseInt8Range8', IncreaseInt8Range('(-9223372036854775808,9223372036854775804)'::INT8RANGE, 3) = '[-9223372036854775804,9223372036854775807)'::INT8RANGE;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INT8RANGE[]', 'IncreaseInt8Range5', IncreaseInt8Range('(-456,-123]'::INT8RANGE, 1) = '[-454,-121)'::INT8RANGE;

--- TSRANGEOID

CREATE OR REPLACE FUNCTION IncreaseTimestampRange(orig_value TSRANGE, days_to_add INTEGER) RETURNS TSRANGE AS $$
return new NpgsqlRange<DateTime>(orig_value.LowerBound.AddDays(days_to_add), orig_value.LowerBoundIsInclusive, orig_value.LowerBoundInfinite, orig_value.UpperBound.AddDays(days_to_add), orig_value.UpperBoundIsInclusive, orig_value.UpperBoundInfinite);
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TSRANGE[]', 'IncreaseTimestampRange1', IncreaseTimestampRange('[2021-01-01 14:30, 2021-01-01 15:30)'::TSRANGE, 1) = '[2021-01-02 14:30, 2021-01-02 15:30)'::TSRANGE;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TSRANGE[]', 'IncreaseTimestampRange2', IncreaseTimestampRange('[, 2021-01-01 15:30)'::TSRANGE, 3) = '[, 2021-01-04 15:30)'::TSRANGE;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TSRANGE[]', 'IncreaseTimestampRange3', IncreaseTimestampRange('[,)'::TSRANGE, 3) = '(,)'::TSRANGE;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TSRANGE[]', 'IncreaseTimestampRange4', IncreaseTimestampRange('(2021-01-01 14:30, 2021-01-01 15:30]'::TSRANGE, 3) = '(2021-01-04 14:30, 2021-01-04 15:30]'::TSRANGE;

--- TSTZRANGEOID

CREATE OR REPLACE FUNCTION IncreaseTimestampTzRange(orig_value TSTZRANGE, days_to_add INTEGER) RETURNS TSTZRANGE AS $$
return new NpgsqlRange<DateTime>(orig_value.LowerBound.AddDays(days_to_add), orig_value.LowerBoundIsInclusive, orig_value.LowerBoundInfinite, orig_value.UpperBound.AddDays(days_to_add), orig_value.UpperBoundIsInclusive, orig_value.UpperBoundInfinite);
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TSTZRANGE[]', 'IncreaseTimestampTzRange1', IncreaseTimestampTzRange('[2021-01-01 14:30 -03, 2021-01-04 15:30 +05)'::TSTZRANGE, 1) = '[2021-01-02 14:30 -03, 2021-01-05 15:30 +05)'::TSTZRANGE;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TSTZRANGE[]', 'IncreaseTimestampTzRange2', IncreaseTimestampTzRange('[, 2021-01-01 15:30 -03)'::TSTZRANGE, 3) = '[, 2021-01-04 15:30 -03)'::TSTZRANGE;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TSTZRANGE[]', 'IncreaseTimestampTzRange3', IncreaseTimestampTzRange('[,)'::TSTZRANGE, 3) = '(,)'::TSTZRANGE;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TSTZRANGE[]', 'IncreaseTimestampTzRange4', IncreaseTimestampTzRange('(2021-01-01 14:30 -03, 2021-01-04 15:30 +05]'::TSTZRANGE, 3) = '(2021-01-04 14:30 -03, 2021-01-07 15:30 +05]'::TSTZRANGE;

--- DATERANGEOID

CREATE OR REPLACE FUNCTION IncreaseDateonlyRange(orig_value DATERANGE, days_to_add INTEGER) RETURNS DATERANGE AS $$
return new NpgsqlRange<DateOnly>(orig_value.LowerBound.AddDays(days_to_add), orig_value.LowerBoundIsInclusive, orig_value.LowerBoundInfinite, orig_value.UpperBound.AddDays(days_to_add), orig_value.UpperBoundIsInclusive, orig_value.UpperBoundInfinite);
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'DATERANGE[]', 'IncreaseDateonlyRange1', IncreaseDateonlyRange('[2021-01-01, 2021-01-04)'::DATERANGE, 1) = '[2021-01-02, 2021-01-05)'::DATERANGE;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'DATERANGE[]', 'IncreaseDateonlyRange2', IncreaseDateonlyRange('[, 2021-01-01)'::DATERANGE, 3) = '[, 2021-01-04)'::DATERANGE;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'DATERANGE[]', 'IncreaseDateonlyRange3', IncreaseDateonlyRange('[,)'::DATERANGE, 3) = '(,)'::DATERANGE;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'DATERANGE[]', 'IncreaseDateonlyRange4', IncreaseDateonlyRange('(2021-01-01, 2021-01-04]'::DATERANGE, 3) = '(2021-01-04, 2021-01-07]'::DATERANGE;

--- INT4RANGE Arrays
CREATE OR REPLACE FUNCTION updateInt4RangeIndex(values_array INT4RANGE[], desired INT4RANGE, index integer[]) RETURNS INT4RANGE[] AS $$
int[] arrayInteger = index.Cast<int>().ToArray();
values_array.SetValue(desired, arrayInteger);
return values_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INT4RANGE[]', 'updateInt4RangeIndex1', updateInt4RangeIndex(ARRAY['[2,6)'::INT4RANGE, '(,6)'::INT4RANGE, null::INT4RANGE, '[,)'::INT4RANGE], '[6,)'::INT4RANGE, ARRAY[2]) = ARRAY['[2,6)'::INT4RANGE, '(,6)'::INT4RANGE, '[6,)'::INT4RANGE, '[,)'::INT4RANGE];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INT4RANGE[]', 'updateInt4RangeIndex2', updateInt4RangeIndex(ARRAY[['[2,6)'::INT4RANGE, '(,6)'::INT4RANGE], [null::INT4RANGE, '[,)'::INT4RANGE]], '[6,)'::INT4RANGE, ARRAY[1, 0]) = ARRAY[['[2,6)'::INT4RANGE, '(,6)'::INT4RANGE], ['[6,)'::INT4RANGE, '[,)'::INT4RANGE]];

CREATE OR REPLACE FUNCTION IncreaseInt4Ranges(values_array INT4RANGE[]) RETURNS INT4RANGE[] AS $$
Array flatten_values = Array.CreateInstance(typeof(object), values_array.Length);
ArrayHandler.FlatArray(values_array, ref flatten_values);
for(int i = 0; i < flatten_values.Length; i++)
{   
    if (flatten_values.GetValue(i) == null)
        continue;

    NpgsqlRange<int> orig_value = (NpgsqlRange<int>)flatten_values.GetValue(i);
    NpgsqlRange<int> new_value = new NpgsqlRange<int>(orig_value.LowerBound + 1, orig_value.LowerBoundIsInclusive, orig_value.LowerBoundInfinite, orig_value.UpperBound + 1, orig_value.UpperBoundIsInclusive, orig_value.UpperBoundInfinite);
    
    flatten_values.SetValue((NpgsqlRange<int>)new_value, i);
}
return flatten_values;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INT4RANGE[]', 'IncreaseInt4Ranges1', IncreaseInt4Ranges(ARRAY['[2,6)'::INT4RANGE, '(,6)'::INT4RANGE, null::INT4RANGE, '[,)'::INT4RANGE]) = ARRAY['[3,7)'::INT4RANGE, '(,7)'::INT4RANGE, null::INT4RANGE, '[,)'::INT4RANGE];

CREATE OR REPLACE FUNCTION CreateInt4MultidimensionalArray() RETURNS INT4RANGE[] AS $$
NpgsqlRange<int> objects_value = new NpgsqlRange<int>(64, true, false, 89, false, false);
NpgsqlRange<int>?[, ,] three_dimensional_array = new NpgsqlRange<int>?[2, 2, 2] {{{objects_value, objects_value}, {null, null}}, {{objects_value, null}, {objects_value, objects_value}}};
return three_dimensional_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INT4RANGE[]', 'CreateInt4MultidimensionalArray1', CreateInt4MultidimensionalArray() = ARRAY[[['[64,89)'::INT4RANGE,'[64,89)'::INT4RANGE], [null::INT4RANGE, null::INT4RANGE]], [['[64,89)'::INT4RANGE, null::INT4RANGE], ['[64,89)'::INT4RANGE, '[64,89)'::INT4RANGE]]];

--- INT8RANGE Arrays
CREATE OR REPLACE FUNCTION updateInt8RangeIndex(values_array INT8RANGE[], desired INT8RANGE, index integer[]) RETURNS INT8RANGE[] AS $$
int[] arrayInteger = index.Cast<int>().ToArray();
values_array.SetValue(desired, arrayInteger);
return values_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INT8RANGE[]', 'updateInt8RangeIndex1', updateInt8RangeIndex(ARRAY['[2,6)'::INT8RANGE, '(,6)'::INT8RANGE, null::INT8RANGE, '[,)'::INT8RANGE], '[6,)'::INT8RANGE, ARRAY[2]) = ARRAY['[2,6)'::INT8RANGE, '(,6)'::INT8RANGE, '[6,)'::INT8RANGE, '[,)'::INT8RANGE];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INT8RANGE[]', 'updateInt8RangeIndex2', updateInt8RangeIndex(ARRAY[['[2,6)'::INT8RANGE, '(,6)'::INT8RANGE], [null::INT8RANGE, '[,)'::INT8RANGE]], '[6,)'::INT8RANGE, ARRAY[1, 0]) = ARRAY[['[2,6)'::INT8RANGE, '(,6)'::INT8RANGE], ['[6,)'::INT8RANGE, '[,)'::INT8RANGE]];

CREATE OR REPLACE FUNCTION IncreaseInt8Ranges(values_array INT8RANGE[]) RETURNS INT8RANGE[] AS $$
Array flatten_values = Array.CreateInstance(typeof(object), values_array.Length);
ArrayHandler.FlatArray(values_array, ref flatten_values);
for(int i = 0; i < flatten_values.Length; i++)
{   
    if (flatten_values.GetValue(i) == null)
        continue;

    NpgsqlRange<long> orig_value = (NpgsqlRange<long>)flatten_values.GetValue(i);
    NpgsqlRange<long> new_value = new NpgsqlRange<long>(orig_value.LowerBound + 1, orig_value.LowerBoundIsInclusive, orig_value.LowerBoundInfinite, orig_value.UpperBound + 1, orig_value.UpperBoundIsInclusive, orig_value.UpperBoundInfinite);
    
    flatten_values.SetValue((NpgsqlRange<long>)new_value, i);
}
return flatten_values;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INT8RANGE[]', 'IncreaseInt8Ranges1', IncreaseInt8Ranges(ARRAY['[2,6)'::INT8RANGE, '(,6)'::INT8RANGE, null::INT8RANGE, '[,)'::INT8RANGE]) = ARRAY['[3,7)'::INT8RANGE, '(,7)'::INT8RANGE, null::INT8RANGE, '[,)'::INT8RANGE];

CREATE OR REPLACE FUNCTION CreateInt8MultidimensionalArray() RETURNS INT8RANGE[] AS $$
NpgsqlRange<long> objects_value = new NpgsqlRange<long>(64, true, false, 89, false, false);
NpgsqlRange<long>?[, ,] three_dimensional_array = new NpgsqlRange<long>?[2, 2, 2] {{{objects_value, objects_value}, {null, null}}, {{objects_value, null}, {objects_value, objects_value}}};
return three_dimensional_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INT8RANGE[]', 'CreateInt8MultidimensionalArray1', CreateInt8MultidimensionalArray() = ARRAY[[['[64,89)'::INT8RANGE,'[64,89)'::INT8RANGE], [null::INT8RANGE, null::INT8RANGE]], [['[64,89)'::INT8RANGE, null::INT8RANGE], ['[64,89)'::INT8RANGE, '[64,89)'::INT8RANGE]]];

--- TSRANGE Arrays
CREATE OR REPLACE FUNCTION updateTimestampRangeIndex(values_array TSRANGE[], desired TSRANGE, index integer[]) RETURNS TSRANGE[] AS $$
int[] arrayInteger = index.Cast<int>().ToArray();
values_array.SetValue(desired, arrayInteger);
return values_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TSRANGE[]', 'updateTimestampRangeIndex1', updateTimestampRangeIndex(ARRAY['[2021-01-01 14:30, 2021-01-01 15:30)'::TSRANGE, '(, 2021-04-01 15:30)'::TSRANGE, null::TSRANGE, '[,)'::TSRANGE], '[2021-05-25 14:30,)'::TSRANGE, ARRAY[2]) = ARRAY['[2021-01-01 14:30, 2021-01-01 15:30)'::TSRANGE, '(, 2021-04-01 15:30)'::TSRANGE, '[2021-05-25 14:30,)'::TSRANGE, '[,)'::TSRANGE];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TSRANGE[]', 'updateTimestampRangeIndex2', updateTimestampRangeIndex(ARRAY[['[2021-01-01 14:30, 2021-01-01 15:30)'::TSRANGE, '(, 2021-04-01 15:30)'::TSRANGE], [null::TSRANGE, '[,)'::TSRANGE]], '[2021-05-25 14:30,)'::TSRANGE, ARRAY[1, 0]) = ARRAY[['[2021-01-01 14:30, 2021-01-01 15:30)'::TSRANGE, '(, 2021-04-01 15:30)'::TSRANGE], ['[2021-05-25 14:30,)'::TSRANGE, '[,)'::TSRANGE]];

CREATE OR REPLACE FUNCTION IncreaseTimestampRanges(values_array TSRANGE[]) RETURNS TSRANGE[] AS $$
Array flatten_values = Array.CreateInstance(typeof(object), values_array.Length);
ArrayHandler.FlatArray(values_array, ref flatten_values);
for(int i = 0; i < flatten_values.Length; i++)
{   
    if (flatten_values.GetValue(i) == null)
        continue;

    NpgsqlRange<DateTime> orig_value = (NpgsqlRange<DateTime>)flatten_values.GetValue(i);
    NpgsqlRange<DateTime> new_value = new NpgsqlRange<DateTime>(orig_value.LowerBound.AddDays(1), orig_value.LowerBoundIsInclusive, orig_value.LowerBoundInfinite, orig_value.UpperBound.AddDays(1), orig_value.UpperBoundIsInclusive, orig_value.UpperBoundInfinite);
    
    flatten_values.SetValue((NpgsqlRange<DateTime>)new_value, i);
}
return flatten_values;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TSRANGE[]', 'IncreaseTimestampRanges1', IncreaseTimestampRanges(ARRAY['[2021-01-01 14:30, 2021-01-01 15:30)'::TSRANGE, '(, 2021-04-01 15:30)'::TSRANGE, null::TSRANGE, '[,)'::TSRANGE]) = ARRAY['[2021-01-02 14:30, 2021-01-02 15:30)'::TSRANGE, '(, 2021-04-02 15:30)'::TSRANGE, null::TSRANGE, '[,)'::TSRANGE];

CREATE OR REPLACE FUNCTION CreateTimestampMultidimensionalArray() RETURNS TSRANGE[] AS $$
NpgsqlRange<DateTime> objects_value = new NpgsqlRange<DateTime>(new DateTime(2022, 4, 14, 12, 30, 25), true, false, new DateTime(2022, 4, 15, 17, 30, 25), false, false);
NpgsqlRange<DateTime>?[, ,] three_dimensional_array = new NpgsqlRange<DateTime>?[2, 2, 2] {{{objects_value, objects_value}, {null, null}}, {{objects_value, null}, {objects_value, objects_value}}};
return three_dimensional_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TSRANGE[]', 'CreateTimestampMultidimensionalArray1', CreateTimestampMultidimensionalArray() = ARRAY[[['[2022-04-14 12:30:25, 2022-04-15 17:30:25)'::TSRANGE,'[2022-04-14 12:30:25, 2022-04-15 17:30:25)'::TSRANGE], [null::TSRANGE, null::TSRANGE]], [['[2022-04-14 12:30:25, 2022-04-15 17:30:25)'::TSRANGE, null::TSRANGE], ['[2022-04-14 12:30:25, 2022-04-15 17:30:25)'::TSRANGE, '[2022-04-14 12:30:25, 2022-04-15 17:30:25)'::TSRANGE]]];

--- TSTZRANGE Arrays
CREATE OR REPLACE FUNCTION updateTimestampTzRangeIndex(values_array TSTZRANGE[], desired TSTZRANGE, index integer[]) RETURNS TSTZRANGE[] AS $$
int[] arrayInteger = index.Cast<int>().ToArray();
values_array.SetValue(desired, arrayInteger);
return values_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TSTZRANGE[]', 'updateTimestampTzRangeIndex1', updateTimestampTzRangeIndex(ARRAY['[2021-01-01 14:30 +02, 2021-01-01 15:30 -05)'::TSTZRANGE, '(, 2021-04-01 15:30 +05)'::TSTZRANGE, null::TSTZRANGE, '[,)'::TSTZRANGE], '[2021-05-25 14:30 +03,)'::TSTZRANGE, ARRAY[2]) = ARRAY['[2021-01-01 14:30 +02, 2021-01-01 15:30 -05)'::TSTZRANGE, '(, 2021-04-01 15:30 +05)'::TSTZRANGE, '[2021-05-25 14:30 +03,)'::TSTZRANGE, '[,)'::TSTZRANGE];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TSTZRANGE[]', 'updateTimestampTzRangeIndex2', updateTimestampTzRangeIndex(ARRAY[['[2021-01-01 14:30 +02, 2021-01-01 15:30 -05)'::TSTZRANGE, '(, 2021-04-01 15:30 +05)'::TSTZRANGE], [null::TSTZRANGE, '[,)'::TSTZRANGE]], '[2021-05-25 14:30 +03,)'::TSTZRANGE, ARRAY[1, 0]) = ARRAY[['[2021-01-01 14:30 +02, 2021-01-01 15:30 -05)'::TSTZRANGE, '(, 2021-04-01 15:30 +05)'::TSTZRANGE], ['[2021-05-25 14:30 +03,)'::TSTZRANGE, '[,)'::TSTZRANGE]];

CREATE OR REPLACE FUNCTION IncreaseTimestampTzRanges(values_array TSTZRANGE[]) RETURNS TSTZRANGE[] AS $$
Array flatten_values = Array.CreateInstance(typeof(object), values_array.Length);
ArrayHandler.FlatArray(values_array, ref flatten_values);
for(int i = 0; i < flatten_values.Length; i++)
{   
    if (flatten_values.GetValue(i) == null)
        continue;

    NpgsqlRange<DateTime> orig_value = (NpgsqlRange<DateTime>)flatten_values.GetValue(i);
    NpgsqlRange<DateTime> new_value = new NpgsqlRange<DateTime>(orig_value.LowerBound.AddDays(1), orig_value.LowerBoundIsInclusive, orig_value.LowerBoundInfinite, orig_value.UpperBound.AddDays(1), orig_value.UpperBoundIsInclusive, orig_value.UpperBoundInfinite);
    
    flatten_values.SetValue((NpgsqlRange<DateTime>)new_value, i);
}
return flatten_values;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TSTZRANGE[]', 'IncreaseTimestampTzRanges1', IncreaseTimestampTzRanges(ARRAY['[2021-01-01 14:30 +02, 2021-01-01 15:30 -05)'::TSTZRANGE, '(, 2021-04-01 15:30 +05)'::TSTZRANGE, null::TSTZRANGE, '[,)'::TSTZRANGE]) = ARRAY['[2021-01-02 14:30 +02, 2021-01-02 15:30 -05)'::TSTZRANGE, '(, 2021-04-02 15:30 +05)'::TSTZRANGE, null::TSTZRANGE, '[,)'::TSTZRANGE];

CREATE OR REPLACE FUNCTION CreateTimestampTzMultidimensionalArray() RETURNS TSTZRANGE[] AS $$
NpgsqlRange<DateTime> objects_value = new NpgsqlRange<DateTime>(new DateTime(2022, 4, 14, 12, 30, 25, DateTimeKind.Utc), true, false, new DateTime(2022, 4, 15, 17, 30, 25, DateTimeKind.Utc), false, false);
NpgsqlRange<DateTime>?[, ,] three_dimensional_array = new NpgsqlRange<DateTime>?[2, 2, 2] {{{objects_value, objects_value}, {null, null}}, {{objects_value, null}, {objects_value, objects_value}}};
return three_dimensional_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TSTZRANGE[]', 'CreateTimestampTzMultidimensionalArray1', CreateTimestampTzMultidimensionalArray() = ARRAY[[['[2022-04-14 12:30:25 +00, 2022-04-15 17:30:25 +00)'::TSTZRANGE,'[2022-04-14 12:30:25 +00, 2022-04-15 17:30:25 +00)'::TSTZRANGE], [null::TSTZRANGE, null::TSTZRANGE]], [['[2022-04-14 12:30:25 +00, 2022-04-15 17:30:25 +00)'::TSTZRANGE, null::TSTZRANGE], ['[2022-04-14 12:30:25 +00, 2022-04-15 17:30:25 +00)'::TSTZRANGE, '[2022-04-14 12:30:25 +00, 2022-04-15 17:30:25 +00)'::TSTZRANGE]]];

--- DATERANGE Arrays
CREATE OR REPLACE FUNCTION updateDateonlyRangeIndex(values_array DATERANGE[], desired DATERANGE, index integer[]) RETURNS DATERANGE[] AS $$
int[] arrayInteger = index.Cast<int>().ToArray();
values_array.SetValue(desired, arrayInteger);
return values_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'DATERANGE[]', 'updateDateonlyRangeIndex1', updateDateonlyRangeIndex(ARRAY['[2021-01-01, 2021-01-01)'::DATERANGE, '(, 2021-04-01)'::DATERANGE, null::DATERANGE, '[,)'::DATERANGE], '[2021-05-25,)'::DATERANGE, ARRAY[2]) = ARRAY['[2021-01-01, 2021-01-01)'::DATERANGE, '(, 2021-04-01)'::DATERANGE, '[2021-05-25,)'::DATERANGE, '[,)'::DATERANGE];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'DATERANGE[]', 'updateDateonlyRangeIndex2', updateDateonlyRangeIndex(ARRAY[['[2021-01-01, 2021-01-01)'::DATERANGE, '(, 2021-04-01)'::DATERANGE], [null::DATERANGE, '[,)'::DATERANGE]], '[2021-05-25,)'::DATERANGE, ARRAY[1, 0]) = ARRAY[['[2021-01-01, 2021-01-01)'::DATERANGE, '(, 2021-04-01)'::DATERANGE], ['[2021-05-25,)'::DATERANGE, '[,)'::DATERANGE]];

CREATE OR REPLACE FUNCTION IncreaseDateonlyRanges(values_array DATERANGE[]) RETURNS DATERANGE[] AS $$
Array flatten_values = Array.CreateInstance(typeof(object), values_array.Length);
ArrayHandler.FlatArray(values_array, ref flatten_values);
for(int i = 0; i < flatten_values.Length; i++)
{   
    if (flatten_values.GetValue(i) == null)
        continue;

    NpgsqlRange<DateOnly> orig_value = (NpgsqlRange<DateOnly>)flatten_values.GetValue(i);
    NpgsqlRange<DateOnly> new_value = new NpgsqlRange<DateOnly>(orig_value.LowerBound.AddDays(1), orig_value.LowerBoundIsInclusive, orig_value.LowerBoundInfinite, orig_value.UpperBound.AddDays(1), orig_value.UpperBoundIsInclusive, orig_value.UpperBoundInfinite);
    
    flatten_values.SetValue((NpgsqlRange<DateOnly>)new_value, i);
}
return flatten_values;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'DATERANGE[]', 'IncreaseDateonlyRanges1', IncreaseDateonlyRanges(ARRAY['[2021-01-01, 2021-01-01)'::DATERANGE, '(, 2021-04-04)'::DATERANGE, null::DATERANGE, '[,)'::DATERANGE]) = ARRAY['[2021-01-02, 2021-01-02)'::DATERANGE, '(, 2021-04-05)'::DATERANGE, null::DATERANGE, '[,)'::DATERANGE];

CREATE OR REPLACE FUNCTION CreateDateonlyMultidimensionalArray() RETURNS DATERANGE[] AS $$
NpgsqlRange<DateOnly> objects_value = new NpgsqlRange<DateOnly>(new DateOnly(2022, 4, 14), true, false, new DateOnly(2022, 4, 15), false, false);
NpgsqlRange<DateOnly>?[, ,] three_dimensional_array = new NpgsqlRange<DateOnly>?[2, 2, 2] {{{objects_value, objects_value}, {null, null}}, {{objects_value, null}, {objects_value, objects_value}}};
return three_dimensional_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'DATERANGE[]', 'CreateDateonlyMultidimensionalArray1', CreateDateonlyMultidimensionalArray() = ARRAY[[['[2022-04-14, 2022-04-15)'::DATERANGE,'[2022-04-14, 2022-04-15)'::DATERANGE], [null::DATERANGE, null::DATERANGE]], [['[2022-04-14, 2022-04-15)'::DATERANGE, null::DATERANGE], ['[2022-04-14, 2022-04-15)'::DATERANGE, '[2022-04-14, 2022-04-15)'::DATERANGE]]];
