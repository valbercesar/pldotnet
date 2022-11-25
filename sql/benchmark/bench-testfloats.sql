\i sql/benchmark/includes.sql

\i sql/testfloats.sql
\i sql/python/testfloats.sql
--- \i sql/java/testfloats.sql

\set runs 1000

SELECT
    'returnReal',
    plbench('SELECT returnReal()', :runs) as plcsharp,
    plbench('SELECT returnRealPython()', :runs) as plpython;
    --- plbench('SELECT returnRealJava()', :runs) as pljava;

SELECT
    'sumReal',
    plbench('SELECT sumReal(1.10055, 1.10054)', :runs) as plcsharp,
    plbench('SELECT sumRealPython(1.10055, 1.10054)', :runs) as plpython;
    --- plbench('SELECT sumRealJava(1.10055, 1.10054)', :runs) as pljava;

SELECT
    'returnDouble',
    plbench('SELECT returnDouble()', :runs) as plcsharp,
    plbench('SELECT returnDoublePython()', :runs) as plpython;
    --- plbench('SELECT returnDoubleJava()', :runs) as pljava;

SELECT
    'sumDouble',
    plbench('SELECT sumDouble(10.1000000000055, 10.1000000000054)', :runs) as plcsharp,
    plbench('SELECT sumDoublePython(10.1000000000055, 10.1000000000054)', :runs) as plpython;
    --- plbench('SELECT sumDoubleJava(10.1000000000055, 10.1000000000054)', :runs) as pljava;
