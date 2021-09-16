\i sql/benchmark/includes.sql

\i sql/testfunc.sql
\i sql/v8javascript/testfunc.sql
\i sql/python/testfunc.sql
\i sql/pgsql/testfunc.sql
\i sql/java/testfunc.sql
\i sql/perl/testfunc.sql
\i sql/lua/testfunc.sql
\i sql/tcl/testfunc.sql
\i sql/r/testfunc.sql
\i sql/testfsfunc.sql

\set runs 1000

SELECT
    'returnX',
    plbench('SELECT returnX()', :runs) as pldotnet,
    plbench('SELECT returnXV8()', :runs) as plv8,
    plbench('SELECT returnXPython()', :runs) as plpython,
    plbench('SELECT returnXPg()', :runs) as plpgsql,
    plbench('SELECT returnXJava()', :runs) as pljava,
    plbench('SELECT returnXPerl()', :runs) as plperl,
    plbench('SELECT returnXLua()', :runs) as pllua,
    plbench('SELECT returnXTcl()', :runs) as pltcl,
    plbench('SELECT returnXR()', :runs) as plr,
    plbench('SELECT returnXFSharp()', :runs) as plfsharp;

SELECT
    'inc2',
    plbench('SELECT inc2(8)', :runs) as pldotnet,
    plbench('SELECT inc2V8(8)', :runs) as plv8,
    plbench('SELECT inc2Python(8)', :runs) as plpython,
    plbench('SELECT inc2Pg(8)', :runs) as plpgsql,
    plbench('SELECT inc2Java(8)', :runs) as pljava,
    plbench('SELECT inc2Perl(8)', :runs) as plperl,
    plbench('SELECT inc2Lua(8)', :runs) as pllua,
    plbench('SELECT inc2Tcl(8)', :runs) as pltcl,
    plbench('SELECT inc2R(8)', :runs) as plr,
    plbench('SELECT inc2FSharp(8)', :runs) as plfsharp;

SELECT
    'sum2',
    plbench('SELECT sum2(3,2)', :runs) as pldotnet,
    plbench('SELECT sum2V8(3,2)', :runs) as plv8,
    plbench('SELECT sum2Python(3,2)', :runs) as plpython,
    plbench('SELECT sum2Pg(3,2)', :runs) as plpgsql,
    plbench('SELECT sum2Java(3,2)', :runs) as pljava,
    plbench('SELECT sum2Perl(3,2)', :runs) as plperl,
    plbench('SELECT sum2Lua(3,2)', :runs) as pllua,
    plbench('SELECT sum2Tcl(3,2)', :runs) as pltcl,
    plbench('SELECT sum2R(3,2)', :runs) as plr,
    plbench('SELECT sum2FSharp(3,2)', :runs) as plfsharp;

SELECT
    'sum3',
    plbench('SELECT sum3(3,2,1)', :runs) as pldotnet,
    plbench('SELECT sum3V8(3,2,1)', :runs) as plv8,
    plbench('SELECT sum3Python(3,2,1)', :runs) as plpython,
    plbench('SELECT sum3Pg(3,2,1)', :runs) as plpgsql,
    plbench('SELECT sum3Java(3,2,1)', :runs) as pljava,
    plbench('SELECT sum3Perl(3,2,1)', :runs) as plperl,
    plbench('SELECT sum3Lua(3,2,1)', :runs) as pllua,
    plbench('SELECT sum3Tcl(3,2,1)', :runs) as pltcl,
    plbench('SELECT sum3R(3,2,1)', :runs) as plr,
    plbench('SELECT sum3FSharp(3,2,1)', :runs) as plfsharp;

SELECT
    'sum4',
    plbench('SELECT sum4(4,3,2,1)', :runs) as pldotnet,
    plbench('SELECT sum4V8(4,3,2,1)', :runs) as plv8,
    plbench('SELECT sum4Python(4,3,2,1)', :runs) as plpython,
    plbench('SELECT sum4Pg(4,3,2,1)', :runs) as plpgsql,
    plbench('SELECT sum4Java(4,3,2,1)', :runs) as pljava,
    plbench('SELECT sum4Perl(4,3,2,1)', :runs) as plperl,
    plbench('SELECT sum4Lua(4,3,2,1)', :runs) as pllua,
    plbench('SELECT sum4Tcl(4,3,2,1)', :runs) as pltcl,
    plbench('SELECT sum4R(4,3,2,1)', :runs) as plr,
    plbench('SELECT sum4FSharp(4,3,2,1)', :runs) as plfsharp;
