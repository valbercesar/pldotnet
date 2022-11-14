--- DATEOID
CREATE OR REPLACE FUNCTION modifyInputDate(orig_date DATE) RETURNS DATE AS $$
int day = orig_date.Day;
int month = orig_date.Month;
int year = orig_date.Year;
DateOnly new_date = new DateOnly(year+3,month+1,day+6);
return new_date;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'DATE', 'modifyInputDate', modifyInputDate(DATE 'Oct-14-2022') = DATE 'Nov-20-2025';

--- TIMEOID
CREATE OR REPLACE FUNCTION addMinutes(orig_time TIME, min_to_add INT) RETURNS TIME AS $$
TimeOnly new_time = orig_time.AddMinutes((double) min_to_add);
return new_time;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIME', 'addMinutes', addMinutes(TIME '05:30 PM', 75) = TIME '06:45 PM';

--- TIMETZOID
CREATE OR REPLACE FUNCTION addHours(orig_time TIMETZ, hours_to_add FLOAT) RETURNS TIMETZ AS $$
return orig_time.AddHours((double)hours_to_add);
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIMETZ', 'addHours', addHours(TIMETZ '04:05:06-08:00',1.5) = TIMETZ '05:35:06-08:00';

--- TIMESTAMP
CREATE OR REPLACE FUNCTION setNewDate(orig_timestamp TIMESTAMP, new_date DATE) RETURNS TIMESTAMP AS $$
int new_day = new_date.Day;
int new_month = new_date.Month;
int new_year = new_date.Year;
DateTime new_timestamp = new DateTime(new_year, new_month, new_day, orig_timestamp.Hour, orig_timestamp.Minute, orig_timestamp.Second);
return new_timestamp;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIMESTAMP', 'setNewDate', setNewDate(TIMESTAMP '2004-10-19 10:23:54 PM', DATE '2022-10-17') = TIMESTAMP '2022-10-17 10:23:54 PM';

--- TIMESTAMPTZ
CREATE OR REPLACE FUNCTION addDays(my_timestamp TIMESTAMP WITH TIME ZONE, days_to_add INT) RETURNS TIMESTAMP WITH TIME ZONE AS $$
return my_timestamp.AddDays((double)days_to_add);
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIMESTAMPTZ', 'addDays', addDays(TIMESTAMP WITH TIME ZONE '2004-10-19 10:23:54 PM +02', 2) = TIMESTAMP WITH TIME ZONE '2004-10-21 22:23:54 +02';

--- INTERVAL
CREATE OR REPLACE FUNCTION modifyInterval(orig_interval INTERVAL, days_to_add INT, months_to_add INT) RETURNS INTERVAL AS $$
NpgsqlInterval new_interval = new NpgsqlInterval(orig_interval.Months + months_to_add, orig_interval.Days + days_to_add, orig_interval.Time);
return new_interval;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INTERVAL', 'modifyInterval', modifyInterval(INTERVAL '4 hours 5 minutes 6 seconds', 15, 20) = INTERVAL '1 YEAR 8 MONTHS 15 DAYS 4 HOURS 5 MINUTES 6 SECONDS'; 

--- DATEOID Arrays
CREATE OR REPLACE FUNCTION updateArrayDateIndex(dates DATE[], desired DATE, index integer[]) RETURNS DATE[] AS $$
int[] arrayInteger = index.Cast<int>().ToArray();
dates.SetValue(desired, arrayInteger);
return dates;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'DATE[]', 'updateArrayDateIndex1', updateArrayDateIndex(ARRAY[DATE 'Oct-14-2022', DATE 'Oct-15-2022', null::date, DATE 'Oct-16-2022'], DATE 'Nov-18-2022', ARRAY[2]) = ARRAY[DATE 'Oct-14-2022', DATE 'Oct-15-2022', DATE 'Nov-18-2022', DATE 'Oct-16-2022'];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'DATE[]', 'updateArrayDateIndex2', updateArrayDateIndex(ARRAY[[DATE 'Oct-14-2022', DATE 'Oct-15-2022'], [null::date, DATE 'Oct-16-2022']], DATE 'Nov-18-2022', ARRAY[1, 0]) = ARRAY[[DATE 'Oct-14-2022', DATE 'Oct-15-2022'], [DATE 'Nov-18-2022', DATE 'Oct-16-2022']];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'DATE[]', 'updateArrayDateIndex3', updateArrayDateIndex(ARRAY[[null::date, null::date], [null::date, DATE 'Oct-16-2022']], DATE 'Nov-18-2022', ARRAY[1, 0]) = ARRAY[[null::date, null::date], [DATE 'Nov-18-2022', DATE 'Oct-16-2022']];

CREATE OR REPLACE FUNCTION IncreaseMonthDateArray(dates DATE[]) RETURNS DATE[] AS $$
Array flatten_dates = Array.CreateInstance(typeof(object), dates.Length);
ArrayHandler.FlatArray(dates, ref flatten_dates);
for(int i = 0; i < flatten_dates.Length; i++)
{   
    if (flatten_dates.GetValue(i) == null)
        continue;

    DateOnly orig_date = (DateOnly)flatten_dates.GetValue(i);
    int day = orig_date.Day;
    int month = orig_date.Month;
    int year = orig_date.Year;
    DateOnly new_date = new DateOnly(year,month+1,day);
    
    flatten_dates.SetValue((DateOnly)new_date, i);
}
return flatten_dates;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'DATE[]', 'IncreaseMonthDateArray1', IncreaseMonthDateArray(ARRAY[DATE 'Oct-14-2022', DATE 'Oct-15-2022', null::date, DATE 'Oct-16-2022']) = ARRAY[DATE 'Nov-14-2022', DATE 'Nov-15-2022', null::date, DATE 'Nov-16-2022'];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'DATE[]', 'IncreaseMonthDateArray2', IncreaseMonthDateArray(ARRAY[[DATE 'Oct-14-2022', DATE 'Jan-15-2022'], [DATE 'Nov-18-2022', null::date]]) = ARRAY[DATE 'Nov-14-2022', DATE 'Feb-15-2022', DATE 'Dec-18-2022', null::date];

CREATE OR REPLACE FUNCTION CreateDateMultidimensionalArray() RETURNS DATE[] AS $$
int day = 25;
int month = 12;
int year = 2022;
DateOnly arrayDate = new DateOnly(year,month,day);
DateOnly?[, ,] three_dimensional_array = new DateOnly?[2, 2, 2] {{{arrayDate, arrayDate}, {null, null}}, {{arrayDate, null}, {arrayDate, arrayDate}}};
return three_dimensional_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'DATE[]', 'CreateDateMultidimensionalArray', CreateDateMultidimensionalArray() = ARRAY[[[DATE 'Dec-25-2022'::date, DATE 'Dec-25-2022'::date], [null::date, null::date]], [[DATE 'Dec-25-2022'::date, null::date], [DATE 'Dec-25-2022'::date, DATE 'Dec-25-2022'::date]]];

--- TIMEOID Arrays
CREATE OR REPLACE FUNCTION updateArrayTimeIndex(values_array TIME[], desired TIME, index integer[]) RETURNS TIME[] AS $$
int[] arrayInteger = index.Cast<int>().ToArray();
values_array.SetValue(desired, arrayInteger);
return values_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIME[]', 'updateArrayTimeIndex1', updateArrayTimeIndex(ARRAY[TIME '05:30 PM', TIME '06:30 PM', null::time, TIME '09:30 AM'], TIME '5:45 AM', ARRAY[2]) = ARRAY[TIME '05:30 PM', TIME '06:30 PM', TIME '05:45 AM', TIME '09:30 AM'];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIME[]', 'updateArrayTimeIndex2', updateArrayTimeIndex(ARRAY[[TIME '05:30 PM', TIME '06:30 PM'], [null::time, TIME '09:30 AM']], TIME '5:45 AM', ARRAY[1, 0]) = ARRAY[[TIME '05:30 PM', TIME '06:30 PM'], [TIME '05:45 AM', TIME '09:30 AM']];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIME[]', 'updateArrayTimeIndex3', updateArrayTimeIndex(ARRAY[[null::TIME, null::TIME], [null::time, TIME '09:30 AM']], TIME '5:45 AM', ARRAY[1, 0]) = ARRAY[[null::TIME, null::TIME], [TIME '05:45 AM', TIME '09:30 AM']];

CREATE OR REPLACE FUNCTION IncreaseMinutesTimeArray(values_array TIME[], min_to_add INT) RETURNS TIME[] AS $$
Array flatten_values = Array.CreateInstance(typeof(object), values_array.Length);
ArrayHandler.FlatArray(values_array, ref flatten_values);
for(int i = 0; i < flatten_values.Length; i++)
{   
    if (flatten_values.GetValue(i) == null)
        continue;


    TimeOnly orig_value = (TimeOnly)flatten_values.GetValue(i);
    TimeOnly new_value = orig_value.AddMinutes((double) min_to_add);
    
    flatten_values.SetValue((TimeOnly)new_value, i);
}
return flatten_values;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIME[]', 'IncreaseMinutesTimeArray1', IncreaseMinutesTimeArray(ARRAY[TIME '05:30 PM', TIME '06:30 PM', null::time, TIME '09:30 AM'], 15) = ARRAY[TIME '05:45 PM', TIME '06:45 PM', null::time, TIME '09:45 AM'];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIME[]', 'IncreaseMinutesTimeArray', IncreaseMinutesTimeArray(ARRAY[[TIME '05:30 PM', TIME '06:30 PM'], [null::time, TIME '09:30 AM']], 15) = ARRAY[TIME '05:45 PM', TIME '06:45 PM', null::time, TIME '09:45 AM'];

CREATE OR REPLACE FUNCTION CreateTimeMultidimensionalArray() RETURNS TIME[] AS $$
int hour = 10;
int minute = 33;
int second = 55;
TimeOnly objects_value = new TimeOnly(hour,minute,second);
TimeOnly?[, ,] three_dimensional_array = new TimeOnly?[2, 2, 2] {{{objects_value, objects_value}, {null, null}}, {{objects_value, null}, {objects_value, objects_value}}};
return three_dimensional_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIME[]', 'CreateTimeMultidimensionalArray', CreateTimeMultidimensionalArray() = ARRAY[[[TIME '10:33:55 AM', TIME '10:33:55 AM'], [null::time, null::time]], [[TIME '10:33:55 AM', null::time], [TIME '10:33:55 AM', TIME '10:33:55 AM']]];

--- TIMETZOID Arrays
CREATE OR REPLACE FUNCTION updateArrayTimetzIndex(values_array TIMETZ[], desired TIMETZ, index integer[]) RETURNS TIMETZ[] AS $$
int[] arrayInteger = index.Cast<int>().ToArray();
values_array.SetValue(desired, arrayInteger);
return values_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIMETZ[]', 'updateArrayTimetzIndex1', updateArrayTimetzIndex(ARRAY[TIMETZ '05:30-03:00', TIMETZ '06:30-03:00', null::timetz, TIMETZ '22:30-03:00'], TIMETZ '02:30-05:00', ARRAY[2]) = ARRAY[TIMETZ '05:30-03:00', TIMETZ '06:30-03:00', TIMETZ '02:30-05:00', TIMETZ '22:30-03:00'];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIMETZ[]', 'updateArrayTimetzIndex2', updateArrayTimetzIndex(ARRAY[[TIMETZ '05:30-03:00', TIMETZ '06:30-03:00'], [null::timetz, TIMETZ '22:30-03:00']], TIMETZ '02:30-05:00', ARRAY[1, 0]) = ARRAY[[TIMETZ '05:30-03:00', TIMETZ '06:30-03:00'], [TIMETZ '02:30-05:00', TIMETZ '22:30-03:00']];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIMETZ[]', 'updateArrayTimetzIndex3', updateArrayTimetzIndex(ARRAY[[null::TIMETZ, null::TIMETZ], [null::timetz, TIMETZ '22:30-03:00']], TIMETZ '02:30-05:00', ARRAY[1, 0]) = ARRAY[[null::TIMETZ, null::TIMETZ], [TIMETZ '02:30-05:00', TIMETZ '22:30-03:00']];

CREATE OR REPLACE FUNCTION IncreaseMinutesTimetzArray(values_array TIMETZ[], min_to_add INT) RETURNS TIMETZ[] AS $$
Array flatten_values = Array.CreateInstance(typeof(object), values_array.Length);
ArrayHandler.FlatArray(values_array, ref flatten_values);
for(int i = 0; i < flatten_values.Length; i++)
{   
    if (flatten_values.GetValue(i) == null)
        continue;

    DateTimeOffset orig_value = (DateTimeOffset)flatten_values.GetValue(i);
    DateTimeOffset new_value = orig_value.AddMinutes((double) min_to_add);
    
    flatten_values.SetValue((DateTimeOffset)new_value, i);
}
return flatten_values;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIMETZ[]', 'IncreaseMinutesTimetzArray1', IncreaseMinutesTimetzArray(ARRAY[TIMETZ '05:30-03:00', TIMETZ '06:30-03:00', null::timetz, TIMETZ '22:30-03:00'], 15) = ARRAY[TIMETZ '05:45-03:00', TIMETZ '06:45-03:00', null::timetz, TIMETZ '22:45-03:00'];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIMETZ[]', 'IncreaseMinutesTimetzArray2', IncreaseMinutesTimetzArray(ARRAY[[TIMETZ '05:30-03:00', TIMETZ '06:30-03:00'], [null::timetz, TIMETZ '22:30-03:00']], 15) = ARRAY[TIMETZ '05:45-03:00', TIMETZ '06:45-03:00', null::timetz, TIMETZ '22:45-03:00'];

CREATE OR REPLACE FUNCTION CreateTimetzMultidimensionalArray() RETURNS TIMETZ[] AS $$
int hour = 10;
int minute = 33;
int second = 55;
DateTimeOffset objects_value = new DateTimeOffset(2022, 12, 25, hour, minute, second, new TimeSpan(2, 0, 0));
DateTimeOffset?[, ,] three_dimensional_array = new DateTimeOffset?[2, 2, 2] {{{objects_value, objects_value}, {null, null}}, {{objects_value, null}, {objects_value, objects_value}}};
return three_dimensional_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIMETZ[]', 'CreateTimetzMultidimensionalArray', CreateTimetzMultidimensionalArray() = ARRAY[[[TIMETZ '10:33:55+02:00', TIMETZ '10:33:55+02:00'], [null::timetz, null::timetz]], [[TIMETZ '10:33:55+02:00', null::timetz], [TIMETZ '10:33:55+02:00', TIMETZ '10:33:55+02:00']]];

--- TIMESTAMP Arrays
CREATE OR REPLACE FUNCTION updateArrayTimestampIndex(values_array TIMESTAMP[], desired TIMESTAMP, index integer[]) RETURNS TIMESTAMP[] AS $$
int[] arrayInteger = index.Cast<int>().ToArray();
values_array.SetValue(desired, arrayInteger);
return values_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIMESTAMP[]', 'updateArrayTimestampIndex1', updateArrayTimestampIndex(ARRAY[TIMESTAMP '2004-10-19 10:23:54 PM', TIMESTAMP '2020-10-19 10:23:54 PM', null::timestamp, TIMESTAMP '2022-12-25 10:23:54 PM'], TIMESTAMP '2025-10-19 10:23:54 PM', ARRAY[2]) = ARRAY[TIMESTAMP '2004-10-19 10:23:54 PM', TIMESTAMP '2020-10-19 10:23:54 PM', TIMESTAMP '2025-10-19 10:23:54 PM', TIMESTAMP '2022-12-25 10:23:54 PM'];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIMESTAMP[]', 'updateArrayTimestampIndex2', updateArrayTimestampIndex(ARRAY[[null::timestamp, null::timestamp], [null::timestamp, TIMESTAMP '2022-12-25 10:23:54 PM']], TIMESTAMP '2025-10-19 10:23:54 PM', ARRAY[1,0]) = ARRAY[[null::timestamp, null::timestamp], [TIMESTAMP '2025-10-19 10:23:54 PM', TIMESTAMP '2022-12-25 10:23:54 PM']];

CREATE OR REPLACE FUNCTION IncreaseTimestamps(values_array TIMESTAMP[], days_to_add INT) RETURNS TIMESTAMP[] AS $$
Array flatten_values = Array.CreateInstance(typeof(object), values_array.Length);
ArrayHandler.FlatArray(values_array, ref flatten_values);
for(int i = 0; i < flatten_values.Length; i++)
{   
    if (flatten_values.GetValue(i) == null)
        continue;

    DateTime orig_value = (DateTime)flatten_values.GetValue(i);
    DateTime new_value = orig_value.AddDays((double)days_to_add);
    
    flatten_values.SetValue((DateTime)new_value, i);
}
return flatten_values;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIMESTAMP[]', 'IncreaseTimestamps', IncreaseTimestamps(ARRAY[TIMESTAMP '2004-12-19 10:23:54 PM', TIMESTAMP '2020-10-19 10:23:54 PM', null::timestamp, TIMESTAMP '2022-12-25 10:23:54 PM'], 2) = ARRAY[TIMESTAMP '2004-12-21 10:23:54 PM', TIMESTAMP '2020-10-21 10:23:54 PM', null::timestamp, TIMESTAMP '2022-12-27 10:23:54 PM'];

CREATE OR REPLACE FUNCTION CreateTimestampMultidimensionalArray() RETURNS TIMESTAMP[] AS $$
int year = 2022;
int month = 11;
int day = 15;
int hour = 13;
int minute = 23;
int seconds = 45;
DateTime objects_value = new DateTime(year, month, day, hour, minute, seconds);
DateTime?[, ,] three_dimensional_array = new DateTime?[2, 2, 2] {{{objects_value, objects_value}, {null, null}}, {{objects_value, null}, {objects_value, objects_value}}};
return three_dimensional_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIMESTAMP[]', 'CreateTimestampMultidimensionalArray', CreateTimestampMultidimensionalArray() = ARRAY[[[TIMESTAMP '2022-11-15 13:23:45', TIMESTAMP '2022-11-15 13:23:45'], [null::timestamp, null::timestamp]], [[TIMESTAMP '2022-11-15 13:23:45', null::timestamp], [TIMESTAMP '2022-11-15 13:23:45', TIMESTAMP '2022-11-15 13:23:45']]];

--- TIMESTAMPTZ Arrays
CREATE OR REPLACE FUNCTION updateArrayTimestamptzIndex(values_array TIMESTAMP WITH TIME ZONE[], desired TIMESTAMP WITH TIME ZONE, index integer[]) RETURNS TIMESTAMP WITH TIME ZONE[] AS $$
int[] arrayInteger = index.Cast<int>().ToArray();
values_array.SetValue(desired, arrayInteger);
return values_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIMESTAMPTZ[]', 'updateArrayTimestamptzIndex1', updateArrayTimestamptzIndex(ARRAY[TIMESTAMP WITH TIME ZONE '2004-10-19 10:23:54 PM +02', TIMESTAMP WITH TIME ZONE '2020-10-19 10:23:54 PM +03', null::timestamptz, TIMESTAMP WITH TIME ZONE '2022-12-25 10:23:54 PM -05'], TIMESTAMP WITH TIME ZONE '2025-10-19 10:23:54 PM -03', ARRAY[2]) = ARRAY[TIMESTAMP WITH TIME ZONE '2004-10-19 10:23:54 PM +02', TIMESTAMP WITH TIME ZONE '2020-10-19 10:23:54 PM +03', TIMESTAMP WITH TIME ZONE '2025-10-19 10:23:54 PM -03', TIMESTAMP WITH TIME ZONE '2022-12-25 10:23:54 PM -05'];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIMESTAMPTZ[]', 'updateArrayTimestamptzIndex2', updateArrayTimestamptzIndex(ARRAY[[null::timestamptz, null::timestamptz], [null::timestamptz, TIMESTAMP WITH TIME ZONE '2022-12-25 10:23:54 PM -05']], TIMESTAMP WITH TIME ZONE '2025-10-19 10:23:54 PM -03', ARRAY[1,0]) = ARRAY[[null::timestamptz, null::timestamptz], [TIMESTAMP WITH TIME ZONE '2025-10-19 10:23:54 PM -03', TIMESTAMP WITH TIME ZONE '2022-12-25 10:23:54 PM -05']];

CREATE OR REPLACE FUNCTION IncreaseTimestampstz(values_array TIMESTAMP WITH TIME ZONE[], days_to_add INT) RETURNS TIMESTAMP WITH TIME ZONE[] AS $$
Array flatten_values = Array.CreateInstance(typeof(object), values_array.Length);
ArrayHandler.FlatArray(values_array, ref flatten_values);
for(int i = 0; i < flatten_values.Length; i++)
{   
    if (flatten_values.GetValue(i) == null)
        continue;

    DateTime orig_value = (DateTime)flatten_values.GetValue(i);
    DateTime new_value = orig_value.AddDays((double)days_to_add);
    
    flatten_values.SetValue((DateTime)new_value, i);
}
return flatten_values;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIMESTAMPTZ[]', 'IncreaseTimestampstz', IncreaseTimestampstz(ARRAY[TIMESTAMP WITH TIME ZONE '2004-10-19 10:23:54 PM +02', TIMESTAMP WITH TIME ZONE '2020-10-19 10:23:54 PM +03', null::timestamptz, TIMESTAMP WITH TIME ZONE '2022-12-25 10:23:54 PM -05'], 2) = ARRAY[TIMESTAMP WITH TIME ZONE '2004-10-21 10:23:54 PM +02', TIMESTAMP WITH TIME ZONE '2020-10-21 10:23:54 PM +03', null::timestamptz, TIMESTAMP WITH TIME ZONE '2022-12-27 10:23:54 PM -05'];

CREATE OR REPLACE FUNCTION CreateTimestamptzMultidimensionalArray() RETURNS TIMESTAMP WITH TIME ZONE[] AS $$
int year = 2022;
int month = 11;
int day = 15;
int hour = 13;
int minute = 23;
int seconds = 45;
DateTime objects_value = new DateTime(year, month, day, hour, minute, seconds, DateTimeKind.Utc);
DateTime?[, ,] three_dimensional_array = new DateTime?[2, 2, 2] {{{objects_value, objects_value}, {null, null}}, {{objects_value, null}, {objects_value, objects_value}}};
return three_dimensional_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIMESTAMPTZ[]', 'CreateTimestamptzMultidimensionalArray', CreateTimestamptzMultidimensionalArray() = ARRAY[[[TIMESTAMP WITH TIME ZONE '2022-11-15 13:23:45 +00', TIMESTAMP WITH TIME ZONE '2022-11-15 13:23:45 +00'], [null::timestamp, null::timestamp]], [[TIMESTAMP WITH TIME ZONE '2022-11-15 13:23:45 +00', null::timestamp], [TIMESTAMP WITH TIME ZONE '2022-11-15 13:23:45 +00', TIMESTAMP WITH TIME ZONE '2022-11-15 13:23:45 +00']]];

--- INTERVAL Arrays
CREATE OR REPLACE FUNCTION updateArrayIntervalIndex(values_array INTERVAL[], desired INTERVAL, index integer[]) RETURNS INTERVAL[] AS $$
int[] arrayInteger = index.Cast<int>().ToArray();
values_array.SetValue(desired, arrayInteger);
return values_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INTERVAL[]', 'updateArrayIntervalIndex1', updateArrayIntervalIndex(ARRAY[INTERVAL '4 hours 5 minutes 6 seconds', INTERVAL '8 hours 1 minutes 2 seconds', null::interval, INTERVAL '1 YEAR 8 MONTHS 15 DAYS 10 hours 5 minutes 6 seconds'], INTERVAL '5 YEAR 4 MONTHS 20 DAYS 10 hours 5 minutes 6 seconds', ARRAY[2]) = ARRAY[INTERVAL '4 hours 5 minutes 6 seconds', INTERVAL '8 hours 1 minutes 2 seconds', INTERVAL '5 YEAR 4 MONTHS 20 DAYS 10 hours 5 minutes 6 seconds', INTERVAL '1 YEAR 8 MONTHS 15 DAYS 10 hours 5 minutes 6 seconds'];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INTERVAL[]', 'updateArrayIntervalIndex2', updateArrayIntervalIndex(ARRAY[[INTERVAL '4 hours 5 minutes 6 seconds', INTERVAL '8 hours 1 minutes 2 seconds'], [null::interval, INTERVAL '1 YEAR 8 MONTHS 15 DAYS 10 hours 5 minutes 6 seconds']], INTERVAL '5 YEAR 4 MONTHS 20 DAYS 10 hours 5 minutes 6 seconds', ARRAY[1, 0]) = ARRAY[[INTERVAL '4 hours 5 minutes 6 seconds', INTERVAL '8 hours 1 minutes 2 seconds'], [INTERVAL '5 YEAR 4 MONTHS 20 DAYS 10 hours 5 minutes 6 seconds', INTERVAL '1 YEAR 8 MONTHS 15 DAYS 10 hours 5 minutes 6 seconds']];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INTERVAL[]', 'updateArrayIntervalIndex3', updateArrayIntervalIndex(ARRAY[[null::interval, null::interval], [null::interval, INTERVAL '1 YEAR 8 MONTHS 15 DAYS 10 hours 5 minutes 6 seconds']], INTERVAL '5 YEAR 4 MONTHS 20 DAYS 10 hours 5 minutes 6 seconds', ARRAY[1, 0]) = ARRAY[[null::interval, null::interval], [INTERVAL '5 YEAR 4 MONTHS 20 DAYS 10 hours 5 minutes 6 seconds', INTERVAL '1 YEAR 8 MONTHS 15 DAYS 10 hours 5 minutes 6 seconds']];

CREATE OR REPLACE FUNCTION IncreaseIntervals(values_array INTERVAL[],months_to_add INT, days_to_add INT) RETURNS INTERVAL[] AS $$
Array flatten_values = Array.CreateInstance(typeof(object), values_array.Length);
ArrayHandler.FlatArray(values_array, ref flatten_values);
for(int i = 0; i < flatten_values.Length; i++)
{   
    if (flatten_values.GetValue(i) == null)
        continue;

    NpgsqlInterval orig_value = (NpgsqlInterval)flatten_values.GetValue(i);
    NpgsqlInterval new_value = new NpgsqlInterval(orig_value.Months + months_to_add, orig_value.Days + days_to_add, orig_value.Time);
    
    flatten_values.SetValue((NpgsqlInterval)new_value, i);
}
return flatten_values;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INTERVAL[]', 'IncreaseIntervals1', IncreaseIntervals(ARRAY[INTERVAL '4 hours 5 minutes 6 seconds', INTERVAL '8 hours 1 minutes 2 seconds', null::interval, INTERVAL '1 YEAR 8 MONTHS 15 DAYS 10 hours 5 minutes 6 seconds'], 2, 5) = ARRAY[INTERVAL '2 mons 5 days 4 hours 5 minutes 6 seconds', INTERVAL '2 mons 5 days 8 hours 1 minutes 2 seconds', null::interval, INTERVAL '1 YEAR 10 MONTHS 20 DAYS 10 hours 5 minutes 6 seconds'];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INTERVAL[]', 'IncreaseIntervals2', IncreaseIntervals(ARRAY[[INTERVAL '4 hours 5 minutes 6 seconds', INTERVAL '8 hours 1 minutes 2 seconds'], [null::interval, INTERVAL '1 YEAR 8 MONTHS 15 DAYS 10 hours 5 minutes 6 seconds']], 2, 5) = ARRAY[INTERVAL '2 mons 5 days 4 hours 5 minutes 6 seconds', INTERVAL '2 mons 5 days 8 hours 1 minutes 2 seconds', null::interval, INTERVAL '1 YEAR 10 MONTHS 20 DAYS 10 hours 5 minutes 6 seconds'];

CREATE OR REPLACE FUNCTION CreateIntervalMultidimensionalArray() RETURNS INTERVAL[] AS $$
int months = 10;
int days = 33;
long time = 9000000000;
NpgsqlInterval objects_value = new NpgsqlInterval(months, days, time);
NpgsqlInterval?[, ,] three_dimensional_array = new NpgsqlInterval?[2, 2, 2] {{{objects_value, objects_value}, {null, null}}, {{objects_value, null}, {objects_value, objects_value}}};
return three_dimensional_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INTERVAL[]', 'CreateIntervalMultidimensionalArray', CreateIntervalMultidimensionalArray() = ARRAY[[[INTERVAL '10 months 33 days 2 hours 30 minutes', INTERVAL '10 months 33 days 2 hours 30 minutes'], [null::interval, null::interval]], [[INTERVAL '10 months 33 days 2 hours 30 minutes', null::interval], [INTERVAL '10 months 33 days 2 hours 30 minutes', INTERVAL '10 months 33 days 2 hours 30 minutes']]];
