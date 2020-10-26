
\i sql/benchmark/includes.sql

\i sql/testnullbool.sql
\i sql/v8javascript/testnullbool.sql
\i sql/python/testnullbool.sql
\i sql/pgsql/testnullbool.sql
\i sql/java/testnullbool.sql
\i sql/perl/testnullbool.sql
\i sql/lua/testnullbool.sql

SELECT
    'returnNullBool',
    plbench('SELECT returnNullBool()', 200) as pldotnet,
    plbench('SELECT returnNullBoolV8()', 200) as plv8,
    plbench('SELECT returnNullBoolPython()', 200) as plpython,
    plbench('SELECT returnNullBoolPg()', 200) as plpgsql,
    plbench('SELECT returnNullBoolJava()', 200) as pljava,
    plbench('SELECT returnNullBoolPerl()', 200) as plperl,
    plbench('SELECT returnNullBoolLua()', 200) as pllua;

SELECT
    'BooleanNullAnd(true, null)',
    plbench('SELECT BooleanNullAnd(true, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullAndV8(true, null)', 200) as plv8,
    plbench('SELECT BooleanNullAndPython(true, null)', 200) as plpython,
    plbench('SELECT BooleanNullAndPg(true, null)', 200) as plpgsql,
    plbench('SELECT BooleanNullAndJava(true, null)', 200) as pljava,
    plbench('SELECT BooleanNullAndPerl(true, null)', 200) as plperl,
    plbench('SELECT BooleanNullAndLua(true, null)', 200) as pllua;

SELECT
    'BooleanNullAnd(null, true)',
    plbench('SELECT BooleanNullAnd(null, true)', 200) as pldotnet,
    plbench('SELECT BooleanNullAndV8(null, true)', 200) as plv8,
    plbench('SELECT BooleanNullAndPython(null, true)', 200) as plpython,
    plbench('SELECT BooleanNullAndPg(null, true)', 200) as plpgsql,
    plbench('SELECT BooleanNullAndJava(null, true)', 200) as pljava,
    plbench('SELECT BooleanNullAndPerl(null, true)', 200) as plperl,
    plbench('SELECT BooleanNullAndLua(null, true)', 200) as pllua;

SELECT
    'BooleanNullAnd(false, null)',
    plbench('SELECT BooleanNullAnd(false, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullAndV8(false, null)', 200) as plv8,
    plbench('SELECT BooleanNullAndPython(false, null)', 200) as plpython,
    plbench('SELECT BooleanNullAndPg(false, null)', 200) as plpgsql,
    plbench('SELECT BooleanNullAndJava(false, null)', 200) as pljava,
    plbench('SELECT BooleanNullAndPerl(false, null)', 200) as plperl,
    plbench('SELECT BooleanNullAndLua(false, null)', 200) as pllua;

SELECT
    'BooleanNullAnd(null, false)',
    plbench('SELECT BooleanNullAnd(null, false)', 200) as pldotnet,
    plbench('SELECT BooleanNullAndV8(null, false)', 200) as plv8,
    plbench('SELECT BooleanNullAndPython(null, false)', 200) as plpython,
    plbench('SELECT BooleanNullAndPg(null, false)', 200) as plpgsql,
    plbench('SELECT BooleanNullAndJava(null, false)', 200) as pljava,
    plbench('SELECT BooleanNullAndPerl(null, false)', 200) as plperl,
    plbench('SELECT BooleanNullAndLua(null, false)', 200) as pllua;

SELECT
    'BooleanNullAnd(null, null)',
    plbench('SELECT BooleanNullAnd(null, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullAndV8(null, null)', 200) as plv8,
    plbench('SELECT BooleanNullAndPython(null, null)', 200) as plpython,
    plbench('SELECT BooleanNullAndPg(null, null)', 200) as plpgsql,
    plbench('SELECT BooleanNullAndJava(null, null)', 200) as pljava,
    plbench('SELECT BooleanNullAndPerl(null, null)', 200) as plperl,
    plbench('SELECT BooleanNullAndLua(null, null)', 200) as pllua;

SELECT
    'BooleanNullOr(true, null)',
    plbench('SELECT BooleanNullOr(true, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullOrV8(true, null)', 200) as plv8,
    plbench('SELECT BooleanNullOrPython(true, null)', 200) as plpython,
    plbench('SELECT BooleanNullOrPg(true, null)', 200) as plpgsql,
    plbench('SELECT BooleanNullOrJava(true, null)', 200) as pljava,
    plbench('SELECT BooleanNullOrPerl(true, null)', 200) as plperl,
    plbench('SELECT BooleanNullOrLua(true, null)', 200) as pllua;

SELECT
    'retuBooleanNullOr(null, true)rnBool',
    plbench('SELECT BooleanNullOr(null, true)', 200) as pldotnet,
    plbench('SELECT BooleanNullOrV8(null, true)', 200) as plv8,
    plbench('SELECT BooleanNullOrPython(null, true)', 200) as plpython,
    plbench('SELECT BooleanNullOrPg(null, true)', 200) as plpgsql,
    plbench('SELECT BooleanNullOrJava(null, true)', 200) as pljava,
    plbench('SELECT BooleanNullOrPerl(null, true)', 200) as plperl,
    plbench('SELECT BooleanNullOrLua(null, true)', 200) as pllua;

SELECT
    'BooleanNullOr(false, null)',
    plbench('SELECT BooleanNullOr(false, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullOrV8(false, null)', 200) as plv8,
    plbench('SELECT BooleanNullOrPython(false, null)', 200) as plpython,
    plbench('SELECT BooleanNullOrPg(false, null)', 200) as plpgsql,
    plbench('SELECT BooleanNullOrJava(false, null)', 200) as pljava,
    plbench('SELECT BooleanNullOrPerl(false, null)', 200) as plperl,
    plbench('SELECT BooleanNullOrLua(false, null)', 200) as pllua;

SELECT
    'BooleanNullOr(null, false)',
    plbench('SELECT BooleanNullOr(null, false)', 200) as pldotnet,
    plbench('SELECT BooleanNullOrV8(null, false)', 200) as plv8,
    plbench('SELECT BooleanNullOrPython(null, false)', 200) as plpython,
    plbench('SELECT BooleanNullOrPg(null, false)', 200) as plpgsql,
    plbench('SELECT BooleanNullOrJava(null, false)', 200) as pljava,
    plbench('SELECT BooleanNullOrPerl(null, false)', 200) as plperl,
    plbench('SELECT BooleanNullOrLua(null, false)', 200) as pllua;

SELECT
    'BooleanNullOr(null, null)',
    plbench('SELECT BooleanNullOr(null, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullOrV8(null, null)', 200) as plv8,
    plbench('SELECT BooleanNullOrPython(null, null)', 200) as plpython,
    plbench('SELECT BooleanNullOrPg(null, null)', 200) as plpgsql,
    plbench('SELECT BooleanNullOrJava(null, null)', 200) as pljava,
    plbench('SELECT BooleanNullOrPerl(null, null)', 200) as plperl,
    plbench('SELECT BooleanNullOrLua(null, null)', 200) as pllua;

SELECT
    'BooleanNullXor(true, null)',
    plbench('SELECT BooleanNullXor(true, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullXorV8(true, null)', 200) as plv8,
    plbench('SELECT BooleanNullXorPython(true, null)', 200) as plpython,
    plbench('SELECT BooleanNullXorPg(true, null)', 200) as plpgsql,
    plbench('SELECT BooleanNullXorJava(true, null)', 200) as pljava,
    plbench('SELECT BooleanNullXorPerl(true, null)', 200) as plperl,
    plbench('SELECT BooleanNullXorLua(true, null)', 200) as pllua;

SELECT
    'BooleanNullXor(null, true)',
    plbench('SELECT BooleanNullXor(null, true)', 200) as pldotnet,
    plbench('SELECT BooleanNullXorV8(null, true)', 200) as plv8,
    plbench('SELECT BooleanNullXorPython(null, true)', 200) as plpython,
    plbench('SELECT BooleanNullXorPg(null, true)', 200) as plpgsql,
    plbench('SELECT BooleanNullXorJava(null, true)', 200) as pljava,
    plbench('SELECT BooleanNullXorPerl(null, true)', 200) as plperl,
    plbench('SELECT BooleanNullXorLua(null, true)', 200) as pllua;

SELECT
    'BooleanNullXor(false, null)',
    plbench('SELECT BooleanNullXor(false, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullXorV8(false, null)', 200) as plv8,
    plbench('SELECT BooleanNullXorPython(false, null)', 200) as plpython,
    plbench('SELECT BooleanNullXorPg(false, null)', 200) as plpgsql,
    plbench('SELECT BooleanNullXorJava(false, null)', 200) as pljava,
    plbench('SELECT BooleanNullXorPerl(false, null)', 200) as plperl,
    plbench('SELECT BooleanNullXorLua(false, null)', 200) as pllua;

SELECT
    'BooleanNullXor(null, false)',
    plbench('SELECT BooleanNullXor(null, false)', 200) as pldotnet,
    plbench('SELECT BooleanNullXorV8(null, false)', 200) as plv8,
    plbench('SELECT BooleanNullXorPython(null, false)', 200) as plpython,
    plbench('SELECT BooleanNullXorPg(null, false)', 200) as plpgsql,
    plbench('SELECT BooleanNullXorJava(null, false)', 200) as pljava,
    plbench('SELECT BooleanNullXorPerl(null, false)', 200) as plperl,
    plbench('SELECT BooleanNullXorLua(null, false)', 200) as pllua;

SELECT
    'BooleanNullXor(null, null)',
    plbench('SELECT BooleanNullXor(null, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullXorV8(null, null)', 200) as plv8,
    plbench('SELECT BooleanNullXorPython(null, null)', 200) as plpython,
    plbench('SELECT BooleanNullXorPg(null, null)', 200) as plpgsql,
    plbench('SELECT BooleanNullXorJava(null, null)', 200) as pljava,
    plbench('SELECT BooleanNullXorPerl(null, null)', 200) as plperl,
    plbench('SELECT BooleanNullXorLua(null, null)', 200) as pllua;
