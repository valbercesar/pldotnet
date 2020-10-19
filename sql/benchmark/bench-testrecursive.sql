\i sql/benchmark/includes.sql

\i sql/testrecursive.sql
\i sql/v8javascript/testrecursive.sql

SELECT
    'fibbbV8',
    plbench('SELECT fibbb(30)', 10) as pldotnet,
    plbench('SELECT fibbbV8(30)', 10) as plv8;

SELECT
    'factV8',
    plbench('SELECT fact(5)', 10) as pldotnet,
    plbench('SELECT factV8(5)', 10) as plv8;

SELECT
    'naturalV8(10)',
    plbench('SELECT natural(10) ', 10) as pldotnet,
    plbench('SELECT naturalV8(10) ', 10) as plv8;

SELECT
    'naturalV8(10.5)',
    plbench('SELECT natural(10.5)', 10) as pldotnet,
    plbench('SELECT naturalV8(10.5)', 10) as plv8;
