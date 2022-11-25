\i sql/benchmark/includes.sql

\i sql/testbool.sql
\i sql/python/testbool.sql
--- \i sql/java/testbool.sql

\set runs 1000

SELECT
    'returnBool',
    plbench('SELECT returnBool()', :runs) as plcsharp,
    plbench('SELECT returnBoolPython()', :runs) as plpython
    ;--- plbench('SELECT returnBoolJava()', :runs) as pljava;

SELECT
    'BooleanAnd',
    plbench('SELECT BooleanAnd(true, true)', :runs) as plcsharp,
    plbench('SELECT BooleanAndPython(true, true)', :runs) as plpython
    ;--- plbench('SELECT BooleanAndJava(true, true)', :runs) as pljava;

SELECT
    'BooleanOr',
    plbench('SELECT BooleanOr(false, false)', :runs) as plcsharp,
    plbench('SELECT BooleanOrPython(false, false)', :runs) as plpython
    ;--- plbench('SELECT BooleanOrJava(false, false)', :runs) as pljava

SELECT
    'BooleanXor',
    plbench('SELECT BooleanXor(false, false)', :runs) as plcsharp,
    plbench('SELECT BooleanXorPython(false, false)', :runs) as plpython
    ;--- plbench('SELECT BooleanXorJava(false, false)', :runs) as pljava;