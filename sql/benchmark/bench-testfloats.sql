\i sql/benchmark/includes.sql

\i sql/testfloats.sql
\i sql/v8javascript/testfloats.sql
\i sql/python/testfloats.sql
\i sql/pgsql/testfloats.sql
\i sql/java/testfloats.sql
\i sql/perl/testfloats.sql
\i sql/lua/testfloats.sql

SELECT
    'returnReal',
    plbench('SELECT returnReal()', 500) as pldotnet,
    plbench('SELECT returnRealV8()', 500) as plv8,
    plbench('SELECT returnRealPython()', 500) as plpython,
    plbench('SELECT returnRealPg()', 500) as plpgsql,
    plbench('SELECT returnRealJava()', 500) as pljava,
    plbench('SELECT returnRealPerl()', 500) as plperl,
    plbench('SELECT returnRealLua()', 500) as pllua;

SELECT
    'sumReal',
    plbench('SELECT sumReal(1.50055, 1.50054)', 500) as pldotnet,
    plbench('SELECT sumRealV8(1.50055, 1.50054)', 500) as plv8,
    plbench('SELECT sumRealPython(1.50055, 1.50054)', 500) as plpython,
    plbench('SELECT sumRealPg(1.50055, 1.50054)', 500) as plpgsql,
    plbench('SELECT sumRealJava(1.50055, 1.50054)', 500) as pljava,
    plbench('SELECT sumRealPerl(1.50055, 1.50054)', 500) as plperl,
    plbench('SELECT sumRealLua(1.50055, 1.50054)', 500) as pllua;

SELECT
    'returnDouble',
    plbench('SELECT returnDouble()', 500) as pldotnet,
    plbench('SELECT returnDoubleV8()', 500) as plv8,
    plbench('SELECT returnDoublePython()', 500) as plpython,
    plbench('SELECT returnDoublePg()', 500) as plpgsql,
    plbench('SELECT returnDoubleJava()', 500) as pljava,
    plbench('SELECT returnDoublePerl()', 500) as plperl,
    plbench('SELECT returnDoubleLua()', 500) as pllua;

SELECT
    'sumDouble',
    plbench('SELECT sumDouble(10.5000000000055, 10.5000000000054)', 500) as pldotnet,
    plbench('SELECT sumDoubleV8(10.5000000000055, 10.5000000000054)', 500) as plv8,
    plbench('SELECT sumDoublePython(10.5000000000055, 10.5000000000054)', 500) as plpython,
    plbench('SELECT sumDoublePg(10.5000000000055, 10.5000000000054)', 500) as plpgsql,
    plbench('SELECT sumDoubleJava(10.5000000000055, 10.5000000000054)', 500) as pljava,
    plbench('SELECT sumDoublePerl(10.5000000000055, 10.5000000000054)', 500) as plperl,
    plbench('SELECT sumDoubleLua(10.5000000000055, 10.5000000000054)', 500) as pllua;
