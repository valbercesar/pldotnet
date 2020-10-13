\i sql/benchmark/includes.sql

\i sql/testfloats.sql
\i sql/v8javascript/testfloats.sql
\i sql/python/testfloats.sql

SELECT
    'returnReal',
    plbench('SELECT returnReal()', 800) as pldotnet,
    plbench('SELECT returnRealV8()', 800) as plv8,
    plbench('SELECT returnRealPython()', 800) as plpython;

SELECT
    'sumReal',
    plbench('SELECT sumReal(1.50055, 1.50054)', 800) as pldotnet,
    plbench('SELECT sumRealV8(1.50055, 1.50054)', 800) as plv8,
    plbench('SELECT sumRealPython(1.50055, 1.50054)', 800) as plpython;

SELECT
    'returnDouble',
    plbench('SELECT returnDouble()', 800) as pldotnet,
    plbench('SELECT returnDoubleV8()', 800) as plv8,
    plbench('SELECT returnDoublePython()', 800) as plpython;

SELECT
    'sumDouble',
    plbench('SELECT sumDouble(10.5000000000055, 10.5000000000054)', 800) as pldotnet,
    plbench('SELECT sumDoubleV8(10.5000000000055, 10.5000000000054)', 800) as plv8,
    plbench('SELECT sumDoublePython(10.5000000000055, 10.5000000000054)', 800) as plpython;
