\i sql/benchmark/includes.sql

\i sql/testnullbool.sql
\i sql/v8javascript/testnullbool.sql
\i sql/python/testnullbool.sql
\i sql/pgsql/testnullbool.sql
\i sql/java/testnullbool.sql
\i sql/perl/testnullbool.sql
\i sql/lua/testnullbool.sql
\i sql/tcl/testnullbool.sql
\i sql/r/testnullbool.sql
\i sql/testfsnullbool.sql

SELECT
    'returnNullBool',
    plbench('SELECT returnNullBool()', 200) as pldotnet,
    plbench('SELECT returnNullBoolV8()', 200) as plv8,
    plbench('SELECT returnNullBoolPython()', 200) as plpython,
    plbench('SELECT returnNullBoolPg()', 200) as plpgsql,
    plbench('SELECT returnNullBoolJava()', 200) as pljava,
    plbench('SELECT returnNullBoolPerl()', 200) as plperl,
    plbench('SELECT returnNullBoolLua()', 200) as pllua,
    plbench('SELECT returnNullBoolTcl()', 200) as pltcl,
    plbench('SELECT returnNullBoolR()', 200) as plr,
    plbench('SELECT returnNullBoolFSharp()', 200) as plfsharp;

SELECT
    'BooleanNullAnd(true, null)',
    plbench('SELECT BooleanNullAnd(true, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullAndV8(true, null)', 200) as plv8,
    plbench('SELECT BooleanNullAndPython(true, null)', 200) as plpython,
    plbench('SELECT BooleanNullAndPg(true, null)', 200) as plpgsql,
    plbench('SELECT BooleanNullAndJava(true, null)', 200) as pljava,
    plbench('SELECT BooleanNullAndPerl(true, null)', 200) as plperl,
    plbench('SELECT BooleanNullAndLua(true, null)', 200) as pllua,
    plbench('SELECT BooleanNullAndTcl(true, null)', 200) as pltcl,
    plbench('SELECT BooleanNullAndR(true, null)', 200) as plr,
    plbench('SELECT BooleanNullAndFSharp(true, null)', 200) as plfsharp;

SELECT
    'BooleanNullAnd(null, true)',
    plbench('SELECT BooleanNullAnd(null, true)', 200) as pldotnet,
    plbench('SELECT BooleanNullAndV8(null, true)', 200) as plv8,
    plbench('SELECT BooleanNullAndPython(null, true)', 200) as plpython,
    plbench('SELECT BooleanNullAndPg(null, true)', 200) as plpgsql,
    plbench('SELECT BooleanNullAndJava(null, true)', 200) as pljava,
    plbench('SELECT BooleanNullAndPerl(null, true)', 200) as plperl,
    plbench('SELECT BooleanNullAndLua(null, true)', 200) as pllua,
    plbench('SELECT BooleanNullAndTcl(null, true)', 200) as pltcl,
    plbench('SELECT BooleanNullAndR(null, true)', 200) as plr,
    plbench('SELECT BooleanNullAndFSharp(null, true)', 200) as plfsharp;

SELECT
    'BooleanNullAnd(false, null)',
    plbench('SELECT BooleanNullAnd(false, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullAndV8(false, null)', 200) as plv8,
    plbench('SELECT BooleanNullAndPython(false, null)', 200) as plpython,
    plbench('SELECT BooleanNullAndPg(false, null)', 200) as plpgsql,
    plbench('SELECT BooleanNullAndJava(false, null)', 200) as pljava,
    plbench('SELECT BooleanNullAndPerl(false, null)', 200) as plperl,
    plbench('SELECT BooleanNullAndLua(false, null)', 200) as pllua,
    plbench('SELECT BooleanNullAndTcl(false, null)', 200) as pltcl,
    plbench('SELECT BooleanNullAndR(false, null)', 200) as plr,
    plbench('SELECT BooleanNullAndFSharp(false, null)', 200) as plfsharp;

SELECT
    'BooleanNullAnd(null, false)',
    plbench('SELECT BooleanNullAnd(null, false)', 200) as pldotnet,
    plbench('SELECT BooleanNullAndV8(null, false)', 200) as plv8,
    plbench('SELECT BooleanNullAndPython(null, false)', 200) as plpython,
    plbench('SELECT BooleanNullAndPg(null, false)', 200) as plpgsql,
    plbench('SELECT BooleanNullAndJava(null, false)', 200) as pljava,
    plbench('SELECT BooleanNullAndPerl(null, false)', 200) as plperl,
    plbench('SELECT BooleanNullAndLua(null, false)', 200) as pllua,
    plbench('SELECT BooleanNullAndTcl(null, false)', 200) as pltcl,
    plbench('SELECT BooleanNullAndR(null, false)', 200) as plr,
    plbench('SELECT BooleanNullAndFSharp(null, false)', 200) as plfsharp;

SELECT
    'BooleanNullAnd(null, null)',
    plbench('SELECT BooleanNullAnd(null, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullAndV8(null, null)', 200) as plv8,
    plbench('SELECT BooleanNullAndPython(null, null)', 200) as plpython,
    plbench('SELECT BooleanNullAndPg(null, null)', 200) as plpgsql,
    plbench('SELECT BooleanNullAndJava(null, null)', 200) as pljava,
    plbench('SELECT BooleanNullAndPerl(null, null)', 200) as plperl,
    plbench('SELECT BooleanNullAndLua(null, null)', 200) as pllua,
    plbench('SELECT BooleanNullAndTcl(null, null)', 200) as pltcl,
    plbench('SELECT BooleanNullAndR(null, null)', 200) as plr,
    plbench('SELECT BooleanNullAndFSharp(null, null)', 200) as plfsharp;

SELECT
    'BooleanNullOr(true, null)',
    plbench('SELECT BooleanNullOr(true, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullOrV8(true, null)', 200) as plv8,
    plbench('SELECT BooleanNullOrPython(true, null)', 200) as plpython,
    plbench('SELECT BooleanNullOrPg(true, null)', 200) as plpgsql,
    plbench('SELECT BooleanNullOrJava(true, null)', 200) as pljava,
    plbench('SELECT BooleanNullOrPerl(true, null)', 200) as plperl,
    plbench('SELECT BooleanNullOrLua(true, null)', 200) as pllua,
    plbench('SELECT BooleanNullOrTcl(true, null)', 200) as pltcl,
    plbench('SELECT BooleanNullOrR(true, null)', 200) as plr,
    plbench('SELECT BooleanNullOrFSharp(true, null)', 200) as plfsharp;

SELECT
    'retuBooleanNullOr(null, true)rnBool',
    plbench('SELECT BooleanNullOr(null, true)', 200) as pldotnet,
    plbench('SELECT BooleanNullOrV8(null, true)', 200) as plv8,
    plbench('SELECT BooleanNullOrPython(null, true)', 200) as plpython,
    plbench('SELECT BooleanNullOrPg(null, true)', 200) as plpgsql,
    plbench('SELECT BooleanNullOrJava(null, true)', 200) as pljava,
    plbench('SELECT BooleanNullOrPerl(null, true)', 200) as plperl,
    plbench('SELECT BooleanNullOrLua(null, true)', 200) as pllua,
    plbench('SELECT BooleanNullOrTcl(null, true)', 200) as pltcl,
    plbench('SELECT BooleanNullOrR(null, true)', 200) as plr,
    plbench('SELECT BooleanNullOrFSharp(null, true)', 200) as plfsharp;

SELECT
    'BooleanNullOr(false, null)',
    plbench('SELECT BooleanNullOr(false, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullOrV8(false, null)', 200) as plv8,
    plbench('SELECT BooleanNullOrPython(false, null)', 200) as plpython,
    plbench('SELECT BooleanNullOrPg(false, null)', 200) as plpgsql,
    plbench('SELECT BooleanNullOrJava(false, null)', 200) as pljava,
    plbench('SELECT BooleanNullOrPerl(false, null)', 200) as plperl,
    plbench('SELECT BooleanNullOrLua(false, null)', 200) as pllua,
    plbench('SELECT BooleanNullOrTcl(false, null)', 200) as pltcl,
    plbench('SELECT BooleanNullOrR(false, null)', 200) as plr,
    plbench('SELECT BooleanNullOrFSharp(false, null)', 200) as plfsharp;

SELECT
    'BooleanNullOr(null, false)',
    plbench('SELECT BooleanNullOr(null, false)', 200) as pldotnet,
    plbench('SELECT BooleanNullOrV8(null, false)', 200) as plv8,
    plbench('SELECT BooleanNullOrPython(null, false)', 200) as plpython,
    plbench('SELECT BooleanNullOrPg(null, false)', 200) as plpgsql,
    plbench('SELECT BooleanNullOrJava(null, false)', 200) as pljava,
    plbench('SELECT BooleanNullOrPerl(null, false)', 200) as plperl,
    plbench('SELECT BooleanNullOrLua(null, false)', 200) as pllua,
    plbench('SELECT BooleanNullOrTcl(null, false)', 200) as pltcl,
    plbench('SELECT BooleanNullOrR(null, false)', 200) as plr,
    plbench('SELECT BooleanNullOrFSharp(null, false)', 200) as plfsharp;

SELECT
    'BooleanNullOr(null, null)',
    plbench('SELECT BooleanNullOr(null, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullOrV8(null, null)', 200) as plv8,
    plbench('SELECT BooleanNullOrPython(null, null)', 200) as plpython,
    plbench('SELECT BooleanNullOrPg(null, null)', 200) as plpgsql,
    plbench('SELECT BooleanNullOrJava(null, null)', 200) as pljava,
    plbench('SELECT BooleanNullOrPerl(null, null)', 200) as plperl,
    plbench('SELECT BooleanNullOrLua(null, null)', 200) as pllua,
    plbench('SELECT BooleanNullOrTcl(null, null)', 200) as pltcl,
    plbench('SELECT BooleanNullOrR(null, null)', 200) as plr,
    plbench('SELECT BooleanNullOrFSharp(null, null)', 200) as plfsharp;

SELECT
    'BooleanNullXor(true, null)',
    plbench('SELECT BooleanNullXor(true, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullXorV8(true, null)', 200) as plv8,
    plbench('SELECT BooleanNullXorPython(true, null)', 200) as plpython,
    plbench('SELECT BooleanNullXorPg(true, null)', 200) as plpgsql,
    plbench('SELECT BooleanNullXorJava(true, null)', 200) as pljava,
    plbench('SELECT BooleanNullXorPerl(true, null)', 200) as plperl,
    plbench('SELECT BooleanNullXorLua(true, null)', 200) as pllua,
    plbench('SELECT BooleanNullXorTcl(true, null)', 200) as pltcl,
    plbench('SELECT BooleanNullXorR(true, null)', 200) as plr,
    plbench('SELECT BooleanNullXorFSharp(true, null)', 200) as plfsharp;

SELECT
    'BooleanNullXor(null, true)',
    plbench('SELECT BooleanNullXor(null, true)', 200) as pldotnet,
    plbench('SELECT BooleanNullXorV8(null, true)', 200) as plv8,
    plbench('SELECT BooleanNullXorPython(null, true)', 200) as plpython,
    plbench('SELECT BooleanNullXorPg(null, true)', 200) as plpgsql,
    plbench('SELECT BooleanNullXorJava(null, true)', 200) as pljava,
    plbench('SELECT BooleanNullXorPerl(null, true)', 200) as plperl,
    plbench('SELECT BooleanNullXorLua(null, true)', 200) as pllua,
    plbench('SELECT BooleanNullXorTcl(null, true)', 200) as pltcl,
    plbench('SELECT BooleanNullXorR(null, true)', 200) as plr,
    plbench('SELECT BooleanNullXorFSharp(null, true)', 200) as plfsharp;

SELECT
    'BooleanNullXor(false, null)',
    plbench('SELECT BooleanNullXor(false, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullXorV8(false, null)', 200) as plv8,
    plbench('SELECT BooleanNullXorPython(false, null)', 200) as plpython,
    plbench('SELECT BooleanNullXorPg(false, null)', 200) as plpgsql,
    plbench('SELECT BooleanNullXorJava(false, null)', 200) as pljava,
    plbench('SELECT BooleanNullXorPerl(false, null)', 200) as plperl,
    plbench('SELECT BooleanNullXorLua(false, null)', 200) as pllua,
    plbench('SELECT BooleanNullXorTcl(false, null)', 200) as pltcl,
    plbench('SELECT BooleanNullXorR(false, null)', 200) as plr,
    plbench('SELECT BooleanNullXorFSharp(false, null)', 200) as plfsharp;

SELECT
    'BooleanNullXor(null, false)',
    plbench('SELECT BooleanNullXor(null, false)', 200) as pldotnet,
    plbench('SELECT BooleanNullXorV8(null, false)', 200) as plv8,
    plbench('SELECT BooleanNullXorPython(null, false)', 200) as plpython,
    plbench('SELECT BooleanNullXorPg(null, false)', 200) as plpgsql,
    plbench('SELECT BooleanNullXorJava(null, false)', 200) as pljava,
    plbench('SELECT BooleanNullXorPerl(null, false)', 200) as plperl,
    plbench('SELECT BooleanNullXorLua(null, false)', 200) as pllua,
    plbench('SELECT BooleanNullXorTcl(null, false)', 200) as pltcl,
    plbench('SELECT BooleanNullXorR(null, false)', 200) as plr,
    plbench('SELECT BooleanNullXorFSharp(null, false)', 200) as plfsharp;

SELECT
    'BooleanNullXor(null, null)',
    plbench('SELECT BooleanNullXor(null, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullXorV8(null, null)', 200) as plv8,
    plbench('SELECT BooleanNullXorPython(null, null)', 200) as plpython,
    plbench('SELECT BooleanNullXorPg(null, null)', 200) as plpgsql,
    plbench('SELECT BooleanNullXorJava(null, null)', 200) as pljava,
    plbench('SELECT BooleanNullXorPerl(null, null)', 200) as plperl,
    plbench('SELECT BooleanNullXorLua(null, null)', 200) as pllua,
    plbench('SELECT BooleanNullXorTcl(null, null)', 200) as pltcl,
    plbench('SELECT BooleanNullXorR(null, null)', 200) as plr,
    plbench('SELECT BooleanNullXorFSharp(null, null)', 200) as plfsharp;
