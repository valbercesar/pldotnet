\i sql/benchmark/includes.sql

\i sql/testmixedtypes.sql
\i sql/v8javascript/testmixedtypes.sql
\i sql/python/testmixedtypes.sql
\i sql/pgsql/testmixedtypes.sql
\i sql/java/testmixedtypes.sql
\i sql/perl/testmixedtypes.sql
\i sql/tcl/testmixedtypes.sql
\i sql/r/testmixedtypes.sql
\i sql/testfsmixedtypes.sql

SELECT
    'ageTest(Billy)',
    plbench('SELECT ageTest(''Billy'', 10, ''The KID'')', 1000) as pldotnet,
    plbench('SELECT ageTestV8(''Billy'', 10, ''The KID'')', 1000) as plv8,
    plbench('SELECT ageTestPython(''Billy'', 10, ''The KID'')', 1000) as plpython,
    plbench('SELECT ageTestPg(''Billy'', 10, ''The KID'')', 1000) as plpgsql,
    plbench('SELECT ageTestJava(''Billy'', 10, ''The KID'')', 1000) as pljava,
    plbench('SELECT ageTestPerl(''Billy'', 10, ''The KID'')', 1000) as plperl,
    '-' as pllua,
    plbench('SELECT ageTestTcl(''Billy'', 10, ''The KID'')', 1000) as pltcl,
    plbench('SELECT ageTestR(''Billy'', 10, ''The KID'')', 1000) as plr,
    plbench('SELECT ageTestFSharp(''Billy'', 10, ''The KID'')', 1000) as plfsharp;

SELECT
    'ageTest(John)',
    plbench('SELECT ageTest(''John'', 33, ''Smith'')', 1000) as pldotnet,
    plbench('SELECT ageTestV8(''John'', 33, ''Smith'')', 1000) as plv8,
    plbench('SELECT ageTestPython(''John'', 33, ''Smith'')', 1000) as plpython,
    plbench('SELECT ageTestPg(''John'', 33, ''Smith'')', 1000) as plpgsql,
    plbench('SELECT ageTestJava(''John'', 33, ''Smith'')', 1000) as pljava,
    plbench('SELECT ageTestPerl(''John'', 33, ''Smith'')', 1000) as plperl,
    '-' as pllua,
    plbench('SELECT ageTestTcl(''John'', 33, ''Smith'')', 1000) as pltcl,
    plbench('SELECT ageTestR(''John'', 33, ''Smith'')', 1000) as plr,
    plbench('SELECT ageTestFSharp(''John'', 33, ''Smith'')', 1000) as plfsharp;

SELECT
    'ageTest(Robson)',
    plbench('SELECT ageTest(''Robson'', 41, ''Cruzoe'')', 1000) as pldotnet,
    plbench('SELECT ageTestV8(''Robson'', 41, ''Cruzoe'')', 1000) as plv8,
    plbench('SELECT ageTestPython(''Robson'', 41, ''Cruzoe'')', 1000) as plpython,
    plbench('SELECT ageTestPg(''Robson'', 41, ''Cruzoe'')', 1000) as plpgsql,
    plbench('SELECT ageTestJava(''Robson'', 41, ''Cruzoe'')', 1000) as pljava,
    plbench('SELECT ageTestPerl(''Robson'', 41, ''Cruzoe'')', 1000) as plperl,
    '-' as pllua,
    plbench('SELECT ageTestTcl(''Robson'', 41, ''Cruzoe'')', 1000) as pltcl,
    plbench('SELECT ageTestR(''Robson'', 41, ''Cruzoe'')', 1000) as plr,
    plbench('SELECT ageTestFSharp(''Robson'', 41, ''Cruzoe'')', 1000) as plfsharp;
