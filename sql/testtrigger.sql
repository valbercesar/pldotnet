------------------------------------------------------------------
-------------------------- PREPARATION ---------------------------
------------------------------------------------------------------
--
CREATE TABLE my_table(x int, y int, z real);
CREATE TABLE update_events(event_time TIMESTAMPTZ NOT NULL);
--
-- we need this while creating new trigger functions
-- it skips the function validation step.
-- We do not have any information about the incoming tuples at
-- this time, so we can not generate C# code.
-- We have to enable it again at the end of this test
--
SET check_function_bodies = false;
--
PREPARE insert_my_values(int) AS INSERT INTO my_table
    SELECT
        generate_series(1, $1 * 3, 3) AS x,
        generate_series(2, $1 * 3, 3) AS y,
        generate_series(3, $1 * 3, 3) AS z;
--
PREPARE verify_entries(int, int, int, int) AS
    WITH generated_values AS (
        SELECT
            generate_series(1, $1 * 3, 3) + $2 AS x,
            generate_series(2, $1 * 3, 3) + $3 AS y,
            generate_series(3, $1 * 3, 3) + $4 AS z
    )
    SELECT $1 = COUNT(*)
    FROM generated_values AS gv
    RIGHT JOIN my_table AS mt
    ON
        mt.x = gv.x AND
        mt.y = gv.y AND
        mt.z = gv.z;
--
\set entries_quantity 5
\set offset_x 0
\set offset_y 0
\set offset_z 0
--
-------------------------- PREPARATION ---------------------------
------------------------------------------------------------------
--
------------------------------------------------------------------
--------------------------- BEFORE INSERT ------------------------
--
-- create dummy trigger function
-- it always return null
-- so PG must save the NEW tuple in the database
--
CREATE FUNCTION returnNullBeforeInsertMyTable() RETURNS TRIGGER AS $$
return null;
$$ LANGUAGE plcsharp;
--
-- create the current trigger that calls
-- the dummy function above
-- before any insert
--
CREATE TRIGGER returnNullBeforeInsertMyTable
BEFORE INSERT ON my_table
FOR EACH ROW EXECUTE PROCEDURE returnNullBeforeInsertMyTable();
--
-- test valid insertion
--
EXECUTE insert_my_values(:entries_quantity);
--
-- verify if the 5 entries are present in the database
--
EXECUTE verify_entries(:entries_quantity, :offset_x, :offset_y, :offset_z);
--
-- truncate the table
--
TRUNCATE TABLE my_table;
--
\set entries_quantity 10
--
-- test valid insertion
--
EXECUTE insert_my_values(:entries_quantity);
--
-- verify if the 10 entries are present in the database
--
EXECUTE verify_entries(:entries_quantity, :offset_x, :offset_y, :offset_z);
--
-- remove the previous trigger
--
DROP TRIGGER returnNullBeforeInsertMyTable ON my_table;
--
------------------------------------------------------------------
--------------------------- BEFORE INSERT ------------------------
--
------------------------------------------------------------------
--------------------------- AVOID INSERT -------------------------
--
-- truncate the table
--
TRUNCATE TABLE my_table;
--
\set entries_quantity 5
--
-- test valid insertion
--
EXECUTE insert_my_values(:entries_quantity);
--
-- create another dummy trigger function
-- this one, always return "SKIP", so PG must
-- discard the NEW tuple (do not save in DB)
--
CREATE FUNCTION returnSkipBeforeInsertMyTable() RETURNS TRIGGER AS $$
return "SKIP";
$$ LANGUAGE plcsharp;
--
-- create a new trigger to call the
-- before_insert_on_my_table_skip for each row
-- before any insert
--
CREATE TRIGGER returnSkipBeforeInsertMyTable
BEFORE INSERT ON my_table
FOR EACH ROW EXECUTE PROCEDURE returnSkipBeforeInsertMyTable();
--
-- try to insert more data into my_values table
EXECUTE insert_my_values(:entries_quantity);
--
-- try to insert even more data into my_values table
EXECUTE insert_my_values(:entries_quantity);
--
-- verify if the table still contains only data inserted
-- before the trigger function.
-- the function above should avoid any insertion
-- so we do not expect new entries
--
EXECUTE verify_entries(:entries_quantity, :offset_x, :offset_y, :offset_z);
--
-- remove the previous trigger
--
DROP TRIGGER returnSkipBeforeInsertMyTable ON my_table;
--
--------------------------- AVOID INSERT -------------------------
------------------------------------------------------------------
--
------------------------------------------------------------------
---------------------- MODIFY BEFORE INSERT ----------------------
--
-- create a new trigger function to modify the content
-- inside the NEW tuple and return "MODIFY", so PG must
-- update the tuple and save it.
--
CREATE FUNCTION returnModifyBeforeInsertMyTable() RETURNS TRIGGER AS $$
TD.tg_tuples.NEW.x += 101;
TD.tg_tuples.NEW.y += 102;
TD.tg_tuples.NEW.z += 103;
return "MODIFY";
$$ LANGUAGE plcsharp;
--
-- create a new trigger to call the
-- before_insert_on_my_table_modify for each row
-- before any insert
--
CREATE TRIGGER returnModifyBeforeInsertMyTable
BEFORE INSERT ON my_table
FOR EACH ROW EXECUTE PROCEDURE returnModifyBeforeInsertMyTable();
--
-- truncate the table
--
TRUNCATE TABLE my_table;
--
\set entries_quantity 5
\set offset_x 101
\set offset_y 102
\set offset_z 103
--
-- insert the data again
--
EXECUTE insert_my_values(:entries_quantity);
--
-- now we must validate the modified tuple
--
EXECUTE verify_entries(:entries_quantity, :offset_x, :offset_y, :offset_z);
--
-- remove the previous trigger
--
DROP TRIGGER returnModifyBeforeInsertMyTable ON my_table;
--
---------------------- MODIFY BEFORE INSERT ----------------------
------------------------------------------------------------------
--
------------------------------------------------------------------
-------------------- VALIDATE BEFORE INSERT ----------------------
--
-- create a new function to be called before INSERT
-- It shows how to dynamically avoid an insertion based
-- on some validation step
--
CREATE FUNCTION validateBeforeInsertMyTable() RETURNS TRIGGER AS $$
return (int) TD.tg_tuples.NEW.x == (int) 1 ? "SKIP" : null;
$$ LANGUAGE plcsharp;
--
CREATE TRIGGER validateBeforeInsertMyTable
BEFORE INSERT ON my_table
FOR EACH ROW EXECUTE PROCEDURE validateBeforeInsertMyTable();
--
-- truncate the table again
--
TRUNCATE TABLE my_table;
--
-- try to insert an invalid tuples with NEW.x == 1
--
INSERT INTO my_table (x, y, z) VALUES (1, 2, 3), (1, 3, 2);
--
-- my_table must be empty now
--
SELECT 0 = COUNT(*) FROM my_table;
--
-- try to insert 2 valid tuples and 1 invalid
--
INSERT INTO my_table VALUES (1, 2, 3), (2, 3, 1), (3, 1, 2);
--
-- my table must have two rows
--
SELECT 2 = COUNT(*) FROM my_table;
SELECT 1 = COUNT(*) FROM my_table AS mt WHERE mt.x = 2 AND mt.y = 3 AND mt.z = 1;
SELECT 1 = COUNT(*) FROM my_table AS mt WHERE mt.x = 3 AND mt.y = 1 AND mt.z = 2;
--
-- remove the previous trigger
--
DROP TRIGGER validateBeforeInsertMyTable ON my_table;
--
-------------------- VALIDATE BEFORE INSERT ----------------------
------------------------------------------------------------------
--
------------------------------------------------------------------
-------------------- VALIDATE BEFORE UPDATE ----------------------
--
-- create a new function to be called before UPDATE
-- It shows how to dynamically avoid an insertion based
-- on the value inside a tuple
--
CREATE FUNCTION validateBeforeUpdateMyTable() RETURNS TRIGGER AS $$
return (int) TD.tg_tuples.NEW.x == (int) 1 ? "SKIP" : null;
$$ LANGUAGE plcsharp;
--
CREATE TRIGGER validateBeforeUpdateMyTable
BEFORE UPDATE ON my_table
FOR EACH ROW EXECUTE PROCEDURE validateBeforeUpdateMyTable();
--
-- truncate both tables
--
TRUNCATE TABLE my_table;
TRUNCATE TABLE update_events;
--
\set entries_quantity 5
--
-- insert the data again
--
EXECUTE insert_my_values(:entries_quantity);
--
-- update x in the first row with allowed value(x == 2)
--
UPDATE my_table SET x = 2 WHERE x = 1;
--
-- we should have the first row with the following tuple:
-- (2, 2, 3)
--
SELECT 1 = COUNT(*) FROM my_table WHERE x = 2 AND y = 2 AND z = 3;
--
-- At this point, the user can not set x = 1 again!
--
UPDATE my_table SET x = 1 WHERE x = 2;
--
-- the first row still have the same values:
-- (2, 2, 3) instead of (1, 2, 3)
--
SELECT 0 = COUNT(*) FROM my_table WHERE x = 1 AND y = 2 AND z = 3;
SELECT 1 = COUNT(*) FROM my_table WHERE x = 2 AND y = 2 AND z = 3;
--
-- try to update all x to 1
--
UPDATE my_table SET X = 1;
--
-- we should not have any row with x == 1
--
SELECT 0 = COUNT(*) FROM my_table WHERE x = 1;
--
-- remove the previous trigger
--
DROP TRIGGER validateBeforeUpdateMyTable ON my_table;
--
-------------------- VALIDATE BEFORE UPDATE ----------------------
------------------------------------------------------------------
--
------------------------------------------------------------------
----------------- VERIFY OLD TUPLE BEFORE UPDATE -----------------
--
--
-- create a new function to be called before UPDATE
-- It shows how to access the old tuple
--
CREATE FUNCTION verifyOldBeforeUpdateMyTable() RETURNS TRIGGER AS $$
bool old_y_is_5 = (int) TD.tg_tuples.OLD.y == (int) 5;
bool new_x_is_3 = (int) TD.tg_tuples.NEW.x == (int) 3;
return old_y_is_5 && new_x_is_3 ? "SKIP" : null;
$$ LANGUAGE plcsharp;
--
CREATE TRIGGER verifyOldBeforeUpdateMyTable
BEFORE UPDATE ON my_table
FOR EACH ROW EXECUTE PROCEDURE verifyOldBeforeUpdateMyTable();
--
-- truncate both tables
--
TRUNCATE TABLE my_table;
--
\set entries_quantity 5
--
-- insert the data again
--
EXECUTE insert_my_values(:entries_quantity);
--
-- verify the second row
SELECT 1 = COUNT(*) FROM my_table WHERE 4 = 0 AND y = 5 AND z = 6;
--
-- perform a valid update in the second row
UPDATE my_table SET x = 0 WHERE x = 4;
--
-- verify the second row
SELECT 1 = COUNT(*) FROM my_table WHERE x = 0 AND y = 5 AND z = 6;
--
-- try to make an invalid update in the second row
UPDATE my_table SET x = 4 WHERE x = 0;
--
-- verify the second row again
SELECT 0 = COUNT(*) FROM my_table WHERE 4 = 0 AND y = 5 AND z = 6;
--
DROP TRIGGER verifyOldBeforeUpdateMyTable ON my_table;
--
----------------- VERIFY OLD TUPLE BEFORE UPDATE -----------------
------------------------------------------------------------------
--
------------------------------------------------------------------
-------------------- REGISTER UPDATE EVENT -----------------------
--
-- create a new function to update the update_events table
-- when some change occurs in my_table
--
CREATE FUNCTION registerEventAfterUpdateMyTable() RETURNS TRIGGER AS $$
SPI.Execute("INSERT INTO update_events VALUES (now()::timestamptz)", 1);
return null;
$$ LANGUAGE plcsharp;
--
CREATE TRIGGER registerEventAfterUpdateMyTable
AFTER UPDATE ON my_table
FOR EACH ROW EXECUTE PROCEDURE registerEventAfterUpdateMyTable();
--
-- truncate both tables
--
TRUNCATE TABLE my_table;
TRUNCATE TABLE update_events;
--
\set entries_quantity 5
--
-- insert some data
--
EXECUTE insert_my_values(:entries_quantity);
--
-- the update_events table should be empty at this point
--
SELECT 0 = COUNT(*) FROM update_events;
--
-- lets update the first row
--
UPDATE my_table SET x = 0 WHERE x = 1;
--
-- we should have a new entry in the update_events table
--
SELECT 1 = COUNT(*) FROM update_events;
--
-- lets update the same row again
--
UPDATE my_table SET x = 1 WHERE x = 0;
--
-- we should have another entry
--
SELECT 2 = COUNT(*) FROM update_events;
--
-- lets update two rows
--
UPDATE my_table SET x = x - 1 WHERE x < 7;
--
-- we should have two additional entries
--
SELECT 4 = COUNT(*) FROM update_events;
--
-- remove the previous trigger
--
DROP TRIGGER registerEventAfterUpdateMyTable ON my_table;
--
-------------------- REGISTER UPDATE EVENT -----------------------
------------------------------------------------------------------
--
------------------------------------------------------------------
------------------- ACCESS TRIGGER VARIABLES ---------------------
--
-- This special function will be called before and after INSERT and UPDATES
CREATE FUNCTION accessTriggerVariables() RETURNS TRIGGER AS $$
bool name = TD.tg_info.tg_name.Contains("accesstriggervariables");
bool table_schema = "public" == TD.tg_info.tg_table_schema;
bool table_name = "my_table" == TD.tg_info.tg_table_name;
bool is_row = "ROW" == TD.tg_info.tg_level;
bool is_before = "BEFORE" == TD.tg_info.tg_when;
bool insertion = "INSERT" == TD.tg_info.tg_event;
bool has_foo = TD.tg_args.Any(
    a => String.Equals(
        a,
        "foo",
        StringComparison.InvariantCultureIgnoreCase
    )
);

if (!(name && table_schema && table_name && is_row) || has_foo)
{
    return "SKIP";
}
if (is_before) 
{
    if (insertion)
    {
        return TD.tg_tuples.NEW.x != 0 ? null : "SKIP";
    }
    else
    {
        TD.tg_tuples.NEW.x += 100;
        return "MODIFY";
    }
}
else
{
    SPI.Execute("INSERT INTO update_events VALUES (now()::timestamptz)", 1);
}
return null;
$$ LANGUAGE plcsharp;
--
CREATE TRIGGER accessTriggerVariablesBeforeInsert
BEFORE INSERT ON my_table
FOR EACH ROW EXECUTE PROCEDURE accessTriggerVariables();
--
CREATE TRIGGER accessTriggerVariablesAfterInsert
AFTER INSERT ON my_table
FOR EACH ROW EXECUTE PROCEDURE accessTriggerVariables();
--
CREATE TRIGGER accessTriggerVariablesBeforeUpdate
BEFORE UPDATE ON my_table
FOR EACH ROW EXECUTE PROCEDURE accessTriggerVariables();
--
CREATE TRIGGER accessTriggerVariablesAfterUpdate
AFTER UPDATE ON my_table
FOR EACH ROW EXECUTE PROCEDURE accessTriggerVariables();
--
TRUNCATE TABLE my_table;
TRUNCATE TABLE update_events;
--
-- test the simplest insertion of invalid entries
INSERT INTO my_table (x, y, z) VALUES (0, 2, 3), (0, 3, 2);
--
-- The insertion above must be discarded
--
SELECT 0 = COUNT(*) FROM my_table;
--
-- Insert valid entries
INSERT INTO my_table (x, y, z) VALUES (1, 2, 3), (4, 5, 6);
--
SELECT 1 = COUNT(*) FROM my_table AS mt WHERE mt.x = 1 AND mt.y = 2 AND mt.z = 3.0;
SELECT 1 = COUNT(*) FROM my_table AS mt WHERE mt.x = 4 AND mt.y = 5 AND mt.z = 6.0;
--
-- We must have two update events
--
SELECT 2 = COUNT(*) FROM update_events;
--
-- Test the update events
UPDATE my_table SET x = 0 WHERE x = 1;
--
-- The update event must increase X by 100, according the BEFORE UDPATE trigger
--
SELECT 1 = COUNT(*) FROM my_table WHERE x = 100;
--
-- We have a new update event due to AFTER UPDATE trigger
--
SELECT 3 = COUNT(*) FROM update_events;
--
DROP TRIGGER accessTriggerVariablesBeforeInsert ON my_table;
DROP TRIGGER accessTriggerVariablesAfterInsert ON my_table;
DROP TRIGGER accessTriggerVariablesBeforeUpdate ON my_table;
DROP TRIGGER accessTriggerVariablesAfterUpdate ON my_table;
--
-- Create a new trigger with args
--
CREATE TRIGGER accessTriggerVariablesWithoutFooBeforeInsert
BEFORE INSERT ON my_table
FOR EACH ROW EXECUTE PROCEDURE accessTriggerVariables(2, "bar");
--
TRUNCATE TABLE my_table;
TRUNCATE TABLE update_events;
--
-- test the simplest insertion of invalid entries, 
--
INSERT INTO my_table (x, y, z) VALUES (0, 2, 3), (0, 3, 2);
--
-- The insertion above must be discarded
--
SELECT 0 = COUNT(*) FROM my_table;
--
-- Test the insertion of valid entries, this time it should work
-- given that we do not have the foo parameter
--
INSERT INTO my_table (x, y, z) VALUES (1, 2, 3), (4, 5, 6);
--
-- The insertion above must be ok
--
SELECT 2 = COUNT(*) FROM my_table;
--
-- remove the current trigger without "foo"
--
DROP TRIGGER accessTriggerVariablesWithoutFooBeforeInsert ON my_table;
--
TRUNCATE TABLE my_table;
TRUNCATE TABLE update_events;
--
-- create a new trigger with "foo"
CREATE TRIGGER accessTriggerVariablesWithFooBeforeInsert
BEFORE INSERT ON my_table
FOR EACH ROW EXECUTE PROCEDURE accessTriggerVariables(2, "bar", "foo");
--
-- Test the insertion of valid entries, this time it should not work
-- because we have the foo parameter
--
INSERT INTO my_table (x, y, z) VALUES (1, 2, 3), (4, 5, 6);
--
-- The insertion above must be discarded
--
SELECT 0 = COUNT(*) FROM my_table;
--
DROP TRIGGER accessTriggerVariablesWithFooBeforeInsert ON my_table;
--
------------------- ACCESS TRIGGER VARIABLES ---------------------
------------------------------------------------------------------
--
-- enable function body validation again
--
SET check_function_bodies = true;
--
DROP TABLE my_table CASCADE;
DROP TABLE update_events CASCADE;