\i sql/benchmark/includes.sql

\i sql/testfloats.sql
\i sql/v8javascript/testfloats.sql

SELECT
    'returnReal',
    plbench('SELECT returnReal()', 500) as pldotnet,
    plbench('SELECT returnRealV8()', 500) as plv8;

SELECT
    'sumReal',
    plbench('SELECT sumReal(1.50055, 1.50054)', 500) as pldotnet,
    plbench('SELECT sumRealV8(1.50055, 1.50054)', 500) as plv8;

SELECT
    'returnDouble',
    plbench('SELECT returnDouble()', 500) as pldotnet,
    plbench('SELECT returnDoubleV8()', 500) as plv8;

SELECT
    'sumDouble',
    plbench('SELECT sumDouble(10.5000000000055, 10.5000000000054)', 500) as pldotnet,
    plbench('SELECT sumDoubleV8(10.5000000000055, 10.5000000000054)', 500) as plv8;
