\i sql/benchmark/includes.sql

\i sql/testfunc.sql
\i sql/v8javascript/testfunc.sql
\i sql/python/testfunc.sql
\i sql/pgsql/testfunc.sql
\i sql/java/testfunc.sql

SELECT
    'returnX',
    plbench('SELECT returnX()', 300) as pldotnet,
    plbench('SELECT returnXV8()', 300) as plv8,
    plbench('SELECT returnXPython()', 300) as plpython,
    plbench('SELECT returnXPg()', 300) as plpgsql,
    plbench('SELECT returnXJava()', 300) as pljava;

SELECT
    'inc2',
    plbench('SELECT inc2(8)', 300) as pldotnet,
    plbench('SELECT inc2V8(8)', 300) as plv8,
    plbench('SELECT inc2Python(8)', 300) as plpython,
    plbench('SELECT inc2Pg(8)', 300) as plpgsql,
    plbench('SELECT inc2Java(8)', 300) as pljava;

SELECT
    'sum2',
    plbench('SELECT sum2(3,2)', 300) as pldotnet,
    plbench('SELECT sum2V8(3,2)', 300) as plv8,
    plbench('SELECT sum2Python(3,2)', 300) as plpython,
    plbench('SELECT sum2Pg(3,2)', 300) as plpgsql,
    plbench('SELECT sum2Java(3,2)', 300) as pljava;

SELECT
    'sum3',
    plbench('SELECT sum3(3,2,1)', 300) as pldotnet,
    plbench('SELECT sum3V8(3,2,1)', 300) as plv8,
    plbench('SELECT sum3Python(3,2,1)', 300) as plpython,
    plbench('SELECT sum3Pg(3,2,1)', 300) as plpgsql,
    plbench('SELECT sum3Java(3,2,1)', 300) as pljava;

SELECT
    'sum4',
    plbench('SELECT sum4(4,3,2,1)', 300) as pldotnet,
    plbench('SELECT sum4V8(4,3,2,1)', 300) as plv8,
    plbench('SELECT sum4Python(4,3,2,1)', 300) as plpython,
    plbench('SELECT sum4Pg(4,3,2,1)', 300) as plpgsql,
    plbench('SELECT sum4Java(4,3,2,1)', 300) as pljava;
