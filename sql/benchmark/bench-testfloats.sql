\i sql/benchmark/includes.sql

\i sql/testfloats.sql
\i sql/v8javascript/testfloats.sql
\i sql/python/testfloats.sql
\i sql/pgsql/testfloats.sql

SELECT
    'returnReal',
    plbench('SELECT returnReal()', 500) as pldotnet,
    plbench('SELECT returnRealV8()', 500) as plv8,
    plbench('SELECT returnRealPython()', 500) as plpython,
    plbench('SELECT returnRealPg()', 500) as plpgsql;

SELECT
    'sumReal',
    plbench('SELECT sumReal(1.50055, 1.50054)', 500) as pldotnet,
    plbench('SELECT sumRealV8(1.50055, 1.50054)', 500) as plv8,
    plbench('SELECT sumRealPython(1.50055, 1.50054)', 500) as plpython,
    plbench('SELECT sumRealPg(1.50055, 1.50054)', 500) as plpgsql;

SELECT
    'returnDouble',
    plbench('SELECT returnDouble()', 500) as pldotnet,
    plbench('SELECT returnDoubleV8()', 500) as plv8,
    plbench('SELECT returnDoublePython()', 500) as plpython,
    plbench('SELECT returnDoublePg()', 500) as plpgsql;

SELECT
    'sumDouble',
    plbench('SELECT sumDouble(10.5000000000055, 10.5000000000054)', 500) as pldotnet,
    plbench('SELECT sumDoubleV8(10.5000000000055, 10.5000000000054)', 500) as plv8,
    plbench('SELECT sumDoublePython(10.5000000000055, 10.5000000000054)', 500) as plpython,
    plbench('SELECT sumDoublePg(10.5000000000055, 10.5000000000054)', 500) as plpgsql;
