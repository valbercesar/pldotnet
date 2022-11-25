\i sql/benchmark/includes.sql

\i sql/testfunc.sql
\i sql/python/testfunc.sql
--- \i sql/java/testfunc.sql

\set runs 1000

SELECT
    'returnX',
    plbench('SELECT returnX()', :runs) as plcsharp,
    plbench('SELECT returnXPython()', :runs) as plpython;
    --- plbench('SELECT returnXJava()', :runs) as pljava;

SELECT
    'inc2',
    plbench('SELECT inc2(8)', :runs) as plcsharp,
    plbench('SELECT inc2Python(8)', :runs) as plpython;
    --- plbench('SELECT inc2Java(8)', :runs) as pljava;

SELECT
    'sum2',
    plbench('SELECT sum2(3,2)', :runs) as plcsharp,
    plbench('SELECT sum2Python(3,2)', :runs) as plpython;
    --- plbench('SELECT sum2Java(3,2)', :runs) as pljava;

SELECT
    'sum3',
    plbench('SELECT sum3(3,2,1)', :runs) as plcsharp,
    plbench('SELECT sum3Python(3,2,1)', :runs) as plpython;
    --- plbench('SELECT sum3Java(3,2,1)', :runs) as pljava;

SELECT
    'sum4',
    plbench('SELECT sum4(4,3,2,1)', :runs) as plcsharp,
    plbench('SELECT sum4Python(4,3,2,1)', :runs) as plpython;
    --- plbench('SELECT sum4Java(4,3,2,1)', :runs) as pljava;