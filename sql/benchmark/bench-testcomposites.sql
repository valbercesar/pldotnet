\i sql/benchmark/includes.sql

\i sql/testcomposites.sql
\i sql/v8javascript/testcomposites.sql

SELECT
    'helloPersonAge',
    plbench('SELECT helloPersonAge((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as pldotnet,
    plbench('SELECT helloPersonAgeV8((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as plv8;


SELECT
    'helloPerson',
    plbench('SELECT helloPerson((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as pldotnet,
    plbench('SELECT helloPersonV8((''John Smith'', 38, 85.5, 1.71, 999.999, true))', 700) as plv8;
