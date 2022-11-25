\i sql/benchmark/includes.sql

\i sql/testarray.sql
\i sql/python/testarray.sql
--- \i sql/java/testarray.sql

\set runs 1000

SELECT
    'sumArrayInt',
    plbench('SELECT sumArrayInt( ARRAY[4,1,5] )', :runs) as plcsharp,
    plbench('SELECT sumArrayIntPython( ARRAY[4,1,5] )', :runs) as plpython;
    --- plbench('SELECT sumArrayIntJava( ARRAY[4,1,5] )', :runs) as pljava;

SELECT
    'sumArrayText',
    plbench('SELECT sumArrayText( ARRAY[''Rodrigo'', ''Silva'', ''Lima'', ''Bahia''] )', :runs) as plcsharp,
    plbench('SELECT sumArrayTextPython( ARRAY[''Rodrigo'', ''Silva'', ''Lima'', ''Bahia''])', :runs) as plpython;
    --- plbench('SELECT sumArrayTextJava( ARRAY[''Rodrigo'', ''Silva'', ''Lima'', ''Bahia''] )', :runs) as pljava;
