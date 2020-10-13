
\i sql/benchmark/includes.sql

\i sql/testnumeric.sql
\i sql/v8javascript/testnumeric.sql
\i sql/python/testnumeric.sql

SELECT
    'get_sum(1.3333333, 10)',
    plbench('SELECT get_sum(1.3333333, 10)', 500) as pldotnet,
    plbench('SELECT get_sumV8(1.3333333, 10)', 500) as plv8,
    plbench('SELECT get_sumPython(1.3333333, 10)', 500) as plpython;

SELECT
    'get_sum(1.33333333, -10.99999999)',
    plbench('SELECT get_sum(1.33333333, -10.99999999)', 500) as pldotnet,
    plbench('SELECT get_sumV8(1.33333333, -10.99999999)', 500) as plv8,
    plbench('SELECT get_sumPython(1.33333333, -10.99999999)', 500) as plpython;

SELECT
    'get_sum(1999999999999.555555555555555, -10.99999999)',
    plbench('SELECT get_sum(1999999999999.555555555555555, -10.99999999)', 500) as pldotnet,
    plbench('SELECT get_sumV8(1999999999999.555555555555555, -10.99999999)', 500) as plv8,
    plbench('SELECT get_sumPython(1999999999999.555555555555555, -10.99999999)', 500) as plpython;

SELECT
    'getbigNum(999999999999999999991.9999991)',
    plbench('SELECT getbigNum(999999999999999999991.9999991)', 500) as pldotnet,
    plbench('SELECT getbigNumV8(999999999999999999991.9999991)', 500) as plv8,
    plbench('SELECT getbigNumPython(999999999999999999991.9999991)', 500) as plpython;

SELECT
    'getbigNum(999999999999999999991.99999999)',
    plbench('SELECT getbigNum(999999999999999999991.99999999)', 500) as pldotnet,
    plbench('SELECT getbigNumV8(999999999999999999991.99999999)', 500) as plv8,
    plbench('SELECT getbigNumPython(999999999999999999991.99999999)', 500) as plpython;
