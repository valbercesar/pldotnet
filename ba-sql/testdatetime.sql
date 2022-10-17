--- DATEOID
CREATE OR REPLACE FUNCTION modifyInputDate(orig_date DATE) RETURNS DATE AS $$
int day = orig_date.Day;
int month = orig_date.Month;
int year = orig_date.Year;
DateOnly new_date = new DateOnly(year+3,month+1,day+6);
return new_date;
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'DATE', 'modifyInputDate', modifyInputDate(DATE 'Oct-14-2022') = DATE 'Nov-20-2025';

--- TIMEOID
CREATE OR REPLACE FUNCTION addMinutes(orig_time TIME, min_to_add INT) RETURNS TIME AS $$
TimeOnly new_time = orig_time.AddMinutes((double) min_to_add);
return new_time;
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIME', 'addMinutes', addMinutes(TIME '05:30 PM', 75) = TIME '06:45 PM';

--- TIMETZOID
CREATE OR REPLACE FUNCTION addHours(orig_time TIMETZ, hours_to_add FLOAT) RETURNS TIMETZ AS $$
return orig_time.AddHours((double)hours_to_add);
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIMETZ', 'addHours', addHours(TIMETZ '04:05:06-08:00',1.5) = TIMETZ '05:35:06-08:00';

--- TIMESTAMP
CREATE OR REPLACE FUNCTION setNewDate(orig_timestamp TIMESTAMP, new_date DATE) RETURNS TIMESTAMP AS $$
int new_day = new_date.Day;
int new_month = new_date.Month;
int new_year = new_date.Year;
DateTime new_timestamp = new DateTime(new_year, new_month, new_day, orig_timestamp.Hour, orig_timestamp.Minute, orig_timestamp.Second);
return new_timestamp;
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIMESTAMP', 'setNewDate', setNewDate(TIMESTAMP '2004-10-19 10:23:54 PM', DATE '2022-10-17') = TIMESTAMP '2022-10-17 10:23:54 PM';

--- TIMESTAMPTZ
CREATE OR REPLACE FUNCTION addDays(my_timestamp TIMESTAMP WITH TIME ZONE, days_to_add INT) RETURNS TIMESTAMP WITH TIME ZONE AS $$
return my_timestamp.AddDays((double)days_to_add);
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TIMESTAMPTZ', 'addDays', addDays(TIMESTAMP WITH TIME ZONE '2004-10-19 10:23:54 PM +02', 2) = TIMESTAMP WITH TIME ZONE '2004-10-21 22:23:54 +02';

--- INTERVAL
CREATE OR REPLACE FUNCTION modifyInterval(orig_interval INTERVAL, days_to_add INT, months_to_add INT) RETURNS INTERVAL AS $$
NpgsqlInterval new_interval = new NpgsqlInterval(orig_interval.Months + months_to_add, orig_interval.Days + days_to_add, orig_interval.Time);
return new_interval;
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INTERVAL', 'modifyInterval', modifyInterval(INTERVAL '4 hours 5 minutes 6 seconds', 15, 20) = INTERVAL '1 YEAR 8 MONTHS 15 DAYS 4 HOURS 5 MINUTES 6 SECONDS'; 