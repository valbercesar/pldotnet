\i sql/benchmark/includes.sql

\i sql/testarray.sql
\i sql/python/testarray.sql
\i sql/v8javascript/testarray.sql
\i sql/pgsql/testarray.sql
\i sql/java/testarray.sql
\i sql/perl/testarray.sql

SELECT
    'sumArrayInt',
    plbench('SELECT sumArrayInt( ARRAY[4,1,5] )', 1000) as pldotnet,
    plbench('SELECT sumArrayIntV8( ARRAY[4,1,5] )', 1000) as plv8,
    plbench('SELECT sumArrayIntPython( ARRAY[4,1,5] )', 1000) as plpython,
    plbench('SELECT sumArrayIntPg( ARRAY[4,1,5] )', 1000) as plpgsql,
    plbench('SELECT sumArrayIntJava( ARRAY[4,1,5] )', 1000) as pljava,
    plbench('SELECT sumArrayIntPerl( ARRAY[4,1,5] )', 1000) as plperl;

SELECT
    'sumArrayNum',
    plbench('SELECT sumArrayNum( ARRAY[1.00002, 1.00003, 1.00004] )', 1000) as pldotnet,
    plbench('SELECT sumArrayNumV8( ARRAY[1.00002, 1.00003, 1.00004] )', 1000) as plv8,
    plbench('SELECT sumArrayNumPython( ARRAY[1.00002, 1.00003, 1.00004] )', 1000) as plpython,
    plbench('SELECT sumArrayNumPg( ARRAY[1.00002, 1.00003, 1.00004] )', 1000) as plpgsql,
    plbench('SELECT sumArrayNumJava( ARRAY[1.00002, 1.00003, 1.00004] )', 1000) as pljava,
    plbench('SELECT sumArrayNumPerl( ARRAY[1.00002, 1.00003, 1.00004] )', 1000) as plperl;

SELECT
    'sumArrayText',
    plbench('SELECT sumArrayText( ARRAY[''Rodrigo'', ''Silva'', ''Lima'', ''Bahia''] )', 1000) as pldotnet,
    plbench('SELECT sumArrayTextV8( ARRAY[''Rodrigo'', ''Silva'', ''Lima'', ''Bahia''])', 1000) as plv8,
    plbench('SELECT sumArrayTextPython( ARRAY[''Rodrigo'', ''Silva'', ''Lima'', ''Bahia''])', 1000) as plpython,
    plbench('SELECT sumArrayTextPg( ARRAY[''Rodrigo'', ''Silva'', ''Lima'', ''Bahia''])', 1000) as plpgsql,
    plbench('SELECT sumArrayTextJava( ARRAY[''Rodrigo'', ''Silva'', ''Lima'', ''Bahia''] )', 1000) as pljava,
    plbench('SELECT sumArrayTextPerl( ARRAY[''Rodrigo'', ''Silva'', ''Lima'', ''Bahia''] )', 1000) as plperl;
