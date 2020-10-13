\i sql/benchmark/includes.sql

\i sql/testmixedtypes.sql
\i sql/v8javascript/testmixedtypes.sql
\i sql/python/testmixedtypes.sql

SELECT
    'ageTest(Billy)',
    plbench('SELECT ageTest(''Billy'', 10, ''The KID'')', 1000) as pldotnet,
    plbench('SELECT ageTestV8(''Billy'', 10, ''The KID'')', 1000) as plv8,
    plbench('SELECT ageTestPython(''Billy'', 10, ''The KID'')', 1000) as plpython;

SELECT
    'ageTest(John)',
    plbench('SELECT ageTest(''John'', 33, ''Smith'')', 1000) as pldotnet,
    plbench('SELECT ageTestV8(''John'', 33, ''Smith'')', 1000) as plv8,
    plbench('SELECT ageTestPython(''John'', 33, ''Smith'')', 1000) as plpython;

SELECT
    'ageTest(Robson)',
    plbench('SELECT ageTest(''Robson'', 41, ''Cruzoe'')', 1000) as pldotnet,
    plbench('SELECT ageTestV8(''Robson'', 41, ''Cruzoe'')', 1000) as plv8,
    plbench('SELECT ageTestPython(''Robson'', 41, ''Cruzoe'')', 1000) as plpython;
