\i sql/benchmark/includes.sql

\i sql/testmixedtypes.sql
\i sql/v8javascript/testmixedtypes.sql

SELECT
    'ageTest(Billy)',
    plbench('SELECT ageTest(''Billy'', 10, ''The KID'')', 500) as pldotnet,
    plbench('SELECT ageTestV8(''Billy'', 10, ''The KID'')', 500) as plv8;

SELECT
    'ageTest(John)',
    plbench('SELECT ageTest(''John'', 33, ''Smith'')', 500) as pldotnet,
    plbench('SELECT ageTestV8(''John'', 33, ''Smith'')', 500) as plv8;

SELECT
    'ageTest(Robson)',
    plbench('SELECT ageTest(''Robson'', 41, ''Cruzoe'')', 500) as pldotnet,
    plbench('SELECT ageTestV8(''Robson'', 41, ''Cruzoe'')', 500) as plv8;
