\i sql/benchmark/includes.sql

\i sql/testarray.sql
\i sql/python/testarray.sql
\i sql/v8javascript/testarray.sql
\i sql/pgsql/testarray.sql
\i sql/java/testarray.sql
\i sql/perl/testarray.sql
\i sql/lua/testarray.sql
\i sql/tcl/testarray.sql
\i sql/r/testarray.sql
\i sql/testfsarray.sql

\set runs 1000

SELECT
    'sumArrayInt',
    plbench('SELECT sumArrayInt( ARRAY[4,1,5] )', :runs) as pldotnet,
    plbench('SELECT sumArrayIntV8( ARRAY[4,1,5] )', :runs) as plv8,
    plbench('SELECT sumArrayIntPython( ARRAY[4,1,5] )', :runs) as plpython,
    plbench('SELECT sumArrayIntPg( ARRAY[4,1,5] )', :runs) as plpgsql,
    plbench('SELECT sumArrayIntJava( ARRAY[4,1,5] )', :runs) as pljava,
    plbench('SELECT sumArrayIntPerl( ARRAY[4,1,5] )', :runs) as plperl,
    plbench('SELECT sumArrayIntLua( ARRAY[4,1,5] )', :runs) as pllua,
    plbench('SELECT sumArrayIntTcl( ARRAY[4,1,5] )', :runs) as pltcl,
    plbench('SELECT sumArrayIntR( ARRAY[4,1,5] )', :runs) as plr,
    plbench('SELECT sumArrayIntFSharp( ARRAY[4,1,5] )', :runs) as plfsharp;

SELECT
    'sumArrayNum',
    plbench('SELECT sumArrayNum( ARRAY[1.00002, 1.00003, 1.00004] )', :runs) as pldotnet,
    plbench('SELECT sumArrayNumV8( ARRAY[1.00002, 1.00003, 1.00004] )', :runs) as plv8,
    plbench('SELECT sumArrayNumPython( ARRAY[1.00002, 1.00003, 1.00004] )', :runs) as plpython,
    plbench('SELECT sumArrayNumPg( ARRAY[1.00002, 1.00003, 1.00004] )', :runs) as plpgsql,
    plbench('SELECT sumArrayNumJava( ARRAY[1.00002, 1.00003, 1.00004] )', :runs) as pljava,
    plbench('SELECT sumArrayNumPerl( ARRAY[1.00002, 1.00003, 1.00004] )', :runs) as plperl,
    '-' as pllua,
    plbench('SELECT sumArrayNumTcl( ARRAY[1.00002, 1.00003, 1.00004] )', :runs) as pltcl,
    plbench('SELECT sumArrayNumR( ARRAY[1.00002, 1.00003, 1.00004] )', :runs) as plr,
    plbench('SELECT sumArrayNumFSharp( ARRAY[1.00002, 1.00003, 1.00004] )', :runs) as plfsharp;
    /* FIXME: Not working:
    plbench('SELECT sumArrayNumLua( ARRAY[1.00002, 1.00003, 1.00004] )', :runs) as pllua.
    */

SELECT
    'sumArrayText',
    plbench('SELECT sumArrayText( ARRAY[''Rodrigo'', ''Silva'', ''Lima'', ''Bahia''] )', :runs) as pldotnet,
    plbench('SELECT sumArrayTextV8( ARRAY[''Rodrigo'', ''Silva'', ''Lima'', ''Bahia''])', :runs) as plv8,
    plbench('SELECT sumArrayTextPython( ARRAY[''Rodrigo'', ''Silva'', ''Lima'', ''Bahia''])', :runs) as plpython,
    plbench('SELECT sumArrayTextPg( ARRAY[''Rodrigo'', ''Silva'', ''Lima'', ''Bahia''])', :runs) as plpgsql,
    plbench('SELECT sumArrayTextJava( ARRAY[''Rodrigo'', ''Silva'', ''Lima'', ''Bahia''] )', :runs) as pljava,
    plbench('SELECT sumArrayTextPerl( ARRAY[''Rodrigo'', ''Silva'', ''Lima'', ''Bahia''] )', :runs) as plperl,
    plbench('SELECT sumArrayTextLua( ARRAY[''Rodrigo'', ''Silva'', ''Lima'', ''Bahia''] )', :runs) as pllua,
    plbench('SELECT sumArrayTextTcl( ARRAY[''Rodrigo'', ''Silva'', ''Lima'', ''Bahia''] )', :runs) as pltcl,
    plbench('SELECT sumArrayTextR( ARRAY[''Rodrigo'', ''Silva'', ''Lima'', ''Bahia''] )', :runs) as plr,
    plbench('SELECT sumArrayTextFSharp( ARRAY[''Rodrigo'', ''Silva'', ''Lima'', ''Bahia''] )', :runs) as plfsharp;
