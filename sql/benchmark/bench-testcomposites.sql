\i sql/benchmark/includes.sql

\i sql/testcomposites.sql
\i sql/v8javascript/testcomposites.sql
\i sql/python/testcomposites.sql
\i sql/pgsql/testcomposites.sql
\i sql/java/testcomposites.sql
\i sql/perl/testcomposites.sql
\i sql/lua/testcomposites.sql
\i sql/tcl/testcomposites.sql
\i sql/r/testcomposites.sql
\i sql/testfscomposites.sql

SELECT
    'helloPersonAge',
    plbench('SELECT helloPersonAge((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as pldotnet,
    plbench('SELECT helloPersonAgeV8((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as plv8,
    plbench('SELECT helloPersonAgePython((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as plpython,
    plbench('SELECT helloPersonAgePg((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as plpgsql,
    plbench('SELECT helloPersonAgeJava((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as pljava,
    plbench('SELECT helloPersonAgePerl((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as plperl,
    plbench('SELECT helloPersonAgeLua((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as pllua,
    plbench('SELECT helloPersonAgeTcl((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as pltcl,
    plbench('SELECT helloPersonAgeR((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as plr,
    plbench('SELECT helloPersonAgeFSharp((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as plfsharp;

SELECT
    'helloPerson',
    plbench('SELECT helloPerson((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as pldotnet,
    plbench('SELECT helloPersonV8((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as plv8,
    plbench('SELECT helloPersonPython((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as plpython,
    plbench('SELECT helloPersonPg((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as plpgsql,
    plbench('SELECT helloPersonJava((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as pljava,
    plbench('SELECT helloPersonPerl((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as plperl,
    plbench('SELECT helloPersonLua((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as pllua,
    plbench('SELECT helloPersonTcl((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as pltcl,
    plbench('SELECT helloPersonR((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as plr,
    plbench('SELECT helloPersonFSharp((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as plfsharp;