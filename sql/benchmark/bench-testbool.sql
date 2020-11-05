\i sql/benchmark/includes.sql

\i sql/testbool.sql
\i sql/v8javascript/testbool.sql
\i sql/python/testbool.sql
\i sql/pgsql/testbool.sql
\i sql/java/testbool.sql
\i sql/perl/testbool.sql
\i sql/lua/testbool.sql
\i sql/tcl/testbool.sql
\i sql/r/testbool.sql
\i sql/testfsbool.sql

SELECT
    'returnBool',
    plbench('SELECT returnBool()', 1000) as pldotnet,
    plbench('SELECT returnBoolV8()', 1000) as plv8,
    plbench('SELECT returnBoolPython()', 1000) as plpython,
    plbench('SELECT returnBoolPg()', 1000) as plpgsql,
    plbench('SELECT returnBoolJava()', 1000) as pljava,
    plbench('SELECT returnBoolPerl()', 1000) as plperl,
    plbench('SELECT returnBoolLua()', 1000) as pllua,
    plbench('SELECT returnBoolTcl()', 1000) as pltcl,
    plbench('SELECT returnBoolR()', 1000) as plr,
    plbench('SELECT returnBoolFSharp()', 1000) as plfsharp;

SELECT
    'BooleanAnd',
    plbench('SELECT BooleanAnd(true, true)', 1000) as pldotnet,
    plbench('SELECT BooleanAndV8(true, true)', 1000) as plv8,
    plbench('SELECT BooleanAndPython(true, true)', 1000) as plpython,
    plbench('SELECT BooleanAndPg(true, true)', 1000) as plpgsql,
    plbench('SELECT BooleanAndJava(true, true)', 1000) as pljava,
    plbench('SELECT BooleanAndPerl(true, true)', 1000) as plperl,
    plbench('SELECT BooleanAndLua(true, true)', 1000) as pllua,
    plbench('SELECT BooleanAndTcl(true, true)', 1000) as pltcl,
    plbench('SELECT BooleanAndR(true, true)', 1000) as plr,
    plbench('SELECT BooleanAndFSharp(true, true)', 1000) as plfsharp;

SELECT
    'BooleanOr',
    plbench('SELECT BooleanOr(false, false)', 1000) as pldotnet,
    plbench('SELECT BooleanOrV8(false, false)', 1000) as plv8,
    plbench('SELECT BooleanOrPython(false, false)', 1000) as plpython,
    plbench('SELECT BooleanOrPg(false, false)', 1000) as plpgsql,
    plbench('SELECT BooleanOrJava(false, false)', 1000) as pljava,
    plbench('SELECT BooleanOrPerl(false, false)', 1000) as plperl,
    plbench('SELECT BooleanOrLua(false, false)', 1000) as pllua,
    plbench('SELECT BooleanOrTcl(false, false)', 1000) as pltcl,
    plbench('SELECT BooleanOrR(false, false)', 1000) as plr,
    plbench('SELECT BooleanOrFSharp(false, false)', 1000) as plfsharp;

SELECT
    'BooleanXor',
    plbench('SELECT BooleanXor(false, false)', 100) as pldotnet,
    plbench('SELECT BooleanXorV8(false, false)', 100) as plv8,
    plbench('SELECT BooleanXorPython(false, false)', 100) as plpython,
    plbench('SELECT BooleanXorPg(false, false)', 100) as plpgsql,
    plbench('SELECT BooleanXorJava(false, false)', 100) as pljava,
    plbench('SELECT BooleanXorPerl(false, false)', 100) as plperl,
    plbench('SELECT BooleanXorLua(false, false)', 100) as pllua,
    plbench('SELECT BooleanXorTcl(false, false)', 100) as pltcl,
    plbench('SELECT BooleanXorR(false, false)', 100) as plr,
    plbench('SELECT BooleanXorFSharp(false, false)', 100) as plfsharp;
