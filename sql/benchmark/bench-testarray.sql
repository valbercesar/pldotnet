\i sql/benchmark/includes.sql

\i sql/testarray.sql
\i sql/v8javascript/testarray.sql

SELECT
    'sumArrayInt',
    plbench('SELECT sumArrayInt( ARRAY[4,1,5] )', 200) as pldotnet,
    plbench('SELECT sumArrayIntV8( ARRAY[4,1,5] )', 200) as plv8;

SELECT 
    'sumArrayNum',
    plbench('SELECT sumArrayNum( ARRAY[1.00002, 1.00003, 1.00004] )', 200) as pldotnet,
    plbench('SELECT sumArrayNumV8( ARRAY[1.00002, 1.00003, 1.00004] )', 200) as plv8;

SELECT 
    'sumArrayText',
    plbench('SELECT sumArrayText( ARRAY[''Rodrigo'', ''Silva'', ''Lima'', ''Bahia''] )', 100) as pldotnet,
    plbench('SELECT sumArrayTextV8( ARRAY[''Rodrigo'', ''Silva'', ''Lima'', ''Bahia''])', 100) as plv8;
