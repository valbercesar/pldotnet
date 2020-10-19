\i sql/benchmark/includes.sql

\i sql/testfunc.sql
\i sql/v8javascript/testfunc.sql

SELECT
    'returnX',
    plbench('SELECT returnX()', 600) as pldotnet,
    plbench('SELECT returnXV8()', 600) as plv8;

SELECT
    'inc2',
    plbench('SELECT inc2(8)', 600) as pldotnet,
    plbench('SELECT inc2V8(8)', 600) as plv8;

SELECT
    'sum2',
    plbench('SELECT sum2(3,2)', 600) as pldotnet,
    plbench('SELECT sum2V8(3,2)', 600) as plv8;

SELECT
    'sum3',
    plbench('SELECT sum3(3,2,1)', 600) as pldotnet,
    plbench('SELECT sum3V8(3,2,1)', 600) as plv8;

SELECT
    'sum4',
    plbench('SELECT sum4(4,3,2,1)', 600) as pldotnet,
    plbench('SELECT sum4V8(4,3,2,1)', 600) as plv8;
