\i sql/benchmark/includes.sql

\i sql/testbool.sql
\i sql/v8javascript/testbool.sql
\i sql/python/testbool.sql
\i sql/pgsql/testbool.sql
\i sql/java/testbool.sql

SELECT
    'returnBool',
    plbench('SELECT returnBool()', 1000) as pldotnet,
    plbench('SELECT returnBoolV8()', 1000) as plv8,
    plbench('SELECT returnBoolPython()', 1000) as plpython,
    plbench('SELECT returnBoolPg()', 1000) as plpgsql,
    plbench('SELECT returnBoolJava()', 1000) as pljava;

SELECT
    'BooleanAnd',
    plbench('SELECT BooleanAnd(true, true)', 1000) as pldotnet,
    plbench('SELECT BooleanAndV8(true, true)', 1000) as plv8,
    plbench('SELECT BooleanAndPython(true, true)', 1000) as plpython,
    plbench('SELECT BooleanAndPg(true, true)', 1000) as plpgsql,
    plbench('SELECT BooleanAndJava(true, true)', 1000) as pljava;

SELECT
    'BooleanOr',
    plbench('SELECT BooleanOr(false, false)', 1000) as pldotnet,
    plbench('SELECT BooleanOrV8(false, false)', 1000) as plv8,
    plbench('SELECT BooleanOrPython(false, false)', 1000) as plpython,
    plbench('SELECT BooleanOrPg(false, false)', 1000) as plpgsql,
    plbench('SELECT BooleanOrJava(false, false)', 1000) as pljava;

SELECT
    'BooleanXor',
    plbench('SELECT BooleanXor(false, false)', 100) as pldotnet,
    plbench('SELECT BooleanXorV8(false, false)', 100) as plv8,
    plbench('SELECT BooleanXorPython(false, false)', 100) as plpython,
    plbench('SELECT BooleanXorPg(false, false)', 100) as plpgsql,
    plbench('SELECT BooleanXorJava(false, false)', 100) as pljava;
