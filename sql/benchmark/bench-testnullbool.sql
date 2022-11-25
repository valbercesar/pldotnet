\i sql/benchmark/includes.sql

\i sql/testnullbool.sql
\i sql/python/testnullbool.sql
--- \i sql/java/testnullbool.sql

\set runs 1000

SELECT
    'returnNullBool',
    plbench('SELECT returnNullBool()', :runs) as plcsharp,
    plbench('SELECT returnNullBoolPython()', :runs) as plpython;
    --- plbench('SELECT returnNullBoolJava()', :runs) as pljava;

SELECT
    'BooleanNullAnd(true, null)',
    plbench('SELECT BooleanNullAnd(true, null)', :runs) as plcsharp,
    plbench('SELECT BooleanNullAndPython(true, null)', :runs) as plpython;
    --- plbench('SELECT BooleanNullAndJava(true, null)', :runs) as pljava;

SELECT
    'BooleanNullAnd(null, true)',
    plbench('SELECT BooleanNullAnd(null, true)', :runs) as plcsharp,
    plbench('SELECT BooleanNullAndPython(null, true)', :runs) as plpython;
    --- plbench('SELECT BooleanNullAndJava(null, true)', :runs) as pljava;

SELECT
    'BooleanNullAnd(false, null)',
    plbench('SELECT BooleanNullAnd(false, null)', :runs) as plcsharp,
    plbench('SELECT BooleanNullAndPython(false, null)', :runs) as plpython;
    --- plbench('SELECT BooleanNullAndJava(false, null)', :runs) as pljava;

SELECT
    'BooleanNullAnd(null, false)',
    plbench('SELECT BooleanNullAnd(null, false)', :runs) as plcsharp,
    plbench('SELECT BooleanNullAndPython(null, false)', :runs) as plpython;
    --- plbench('SELECT BooleanNullAndJava(null, false)', :runs) as pljava;

SELECT
    'BooleanNullAnd(null, null)',
    plbench('SELECT BooleanNullAnd(null, null)', :runs) as plcsharp,
    plbench('SELECT BooleanNullAndPython(null, null)', :runs) as plpython;
    --- plbench('SELECT BooleanNullAndJava(null, null)', :runs) as pljava;

SELECT
    'BooleanNullOr(true, null)',
    plbench('SELECT BooleanNullOr(true, null)', :runs) as plcsharp,
    plbench('SELECT BooleanNullOrPython(true, null)', :runs) as plpython;
    --- plbench('SELECT BooleanNullOrJava(true, null)', :runs) as pljava;

SELECT
    'retuBooleanNullOr(null, true)rnBool',
    plbench('SELECT BooleanNullOr(null, true)', :runs) as plcsharp,
    plbench('SELECT BooleanNullOrPython(null, true)', :runs) as plpython;
    --- plbench('SELECT BooleanNullOrJava(null, true)', :runs) as pljava;

SELECT
    'BooleanNullOr(false, null)',
    plbench('SELECT BooleanNullOr(false, null)', :runs) as plcsharp,
    plbench('SELECT BooleanNullOrPython(false, null)', :runs) as plpython;
    --- plbench('SELECT BooleanNullOrJava(false, null)', :runs) as pljava;

SELECT
    'BooleanNullOr(null, false)',
    plbench('SELECT BooleanNullOr(null, false)', :runs) as plcsharp,
    plbench('SELECT BooleanNullOrPython(null, false)', :runs) as plpython;
    --- plbench('SELECT BooleanNullOrJava(null, false)', :runs) as pljava;

SELECT
    'BooleanNullOr(null, null)',
    plbench('SELECT BooleanNullOr(null, null)', :runs) as plcsharp,
    plbench('SELECT BooleanNullOrPython(null, null)', :runs) as plpython;
    --- plbench('SELECT BooleanNullOrJava(null, null)', :runs) as pljava;

SELECT
    'BooleanNullXor(true, null)',
    plbench('SELECT BooleanNullXor(true, null)', :runs) as plcsharp,
    plbench('SELECT BooleanNullXorPython(true, null)', :runs) as plpython;
    --- plbench('SELECT BooleanNullXorJava(true, null)', :runs) as pljava;

SELECT
    'BooleanNullXor(null, true)',
    plbench('SELECT BooleanNullXor(null, true)', :runs) as plcsharp,
    plbench('SELECT BooleanNullXorPython(null, true)', :runs) as plpython;
    --- plbench('SELECT BooleanNullXorJava(null, true)', :runs) as pljava;

SELECT
    'BooleanNullXor(false, null)',
    plbench('SELECT BooleanNullXor(false, null)', :runs) as plcsharp,
    plbench('SELECT BooleanNullXorPython(false, null)', :runs) as plpython;
    --- plbench('SELECT BooleanNullXorJava(false, null)', :runs) as pljava;

SELECT
    'BooleanNullXor(null, false)',
    plbench('SELECT BooleanNullXor(null, false)', :runs) as plcsharp,
    plbench('SELECT BooleanNullXorPython(null, false)', :runs) as plpython;
    --- plbench('SELECT BooleanNullXorJava(null, false)', :runs) as pljava;

SELECT
    'BooleanNullXor(null, null)',
    plbench('SELECT BooleanNullXor(null, null)', :runs) as plcsharp,
    plbench('SELECT BooleanNullXorPython(null, null)', :runs) as plpython;
    --- plbench('SELECT BooleanNullXorJava(null, null)', :runs) as pljava;