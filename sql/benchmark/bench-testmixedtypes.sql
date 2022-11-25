\i sql/benchmark/includes.sql

\i sql/testmixedtypes.sql
\i sql/python/testmixedtypes.sql
--- \i sql/java/testmixedtypes.sql

\set runs 1000

SELECT
    'ageTest(Billy)',
    plbench('SELECT ageTest(''Billy'', 10, ''The KID'')', :runs) as plcsharp,
    plbench('SELECT ageTestPython(''Billy'', 10, ''The KID'')', :runs) as plpython;
    --- plbench('SELECT ageTestJava(''Billy'', 10, ''The KID'')', :runs) as pljava;

SELECT
    'ageTest(John)',
    plbench('SELECT ageTest(''John'', 33, ''Smith'')', :runs) as plcsharp,
    plbench('SELECT ageTestPython(''John'', 33, ''Smith'')', :runs) as plpython;
    --- plbench('SELECT ageTestJava(''John'', 33, ''Smith'')', :runs) as pljava;

SELECT
    'ageTest(Robson)',
    plbench('SELECT ageTest(''Robson'', 41, ''Cruzoe'')', :runs) as plcsharp,
    plbench('SELECT ageTestPython(''Robson'', 41, ''Cruzoe'')', :runs) as plpython;
    --- plbench('SELECT ageTestJava(''Robson'', 41, ''Cruzoe'')', :runs) as pljava;
