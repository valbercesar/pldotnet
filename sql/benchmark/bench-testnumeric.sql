
\i sql/benchmark/includes.sql

\i sql/testnumeric.sql
\i sql/v8javascript/testnumeric.sql

SELECT
    'get_sum(1.3333333, 10)',
    plbench('SELECT get_sum(1.3333333, 10)', 200) as pldotnet,
    plbench('SELECT get_sumV8(1.3333333, 10)', 200) as plv8;

SELECT
    'get_sum(1.33333333, -10.99999999)',
    plbench('SELECT get_sum(1.33333333, -10.99999999)', 200) as pldotnet,
    plbench('SELECT get_sumV8(1.33333333, -10.99999999)', 200) as plv8;

SELECT
    'get_sum(1999999999999.555555555555555, -10.99999999)',
    plbench('SELECT get_sum(1999999999999.555555555555555, -10.99999999)', 200) as pldotnet,
    plbench('SELECT get_sumV8(1999999999999.555555555555555, -10.99999999)', 200) as plv8;

SELECT
    'getbigNum(999999999999999999991.9999991)',
    plbench('SELECT getbigNum(999999999999999999991.9999991)', 200) as pldotnet,
    plbench('SELECT getbigNumV8(999999999999999999991.9999991)', 200) as plv8;

SELECT
    'getbigNum(999999999999999999991.99999999)',
    plbench('SELECT getbigNum(999999999999999999991.99999999)', 200) as pldotnet,
    plbench('SELECT getbigNumV8(999999999999999999991.99999999)', 200) as plv8;
