
\i sql/benchmark/includes.sql

\i sql/testnullbool.sql
\i sql/v8javascript/testnullbool.sql
\i sql/python/testnullbool.sql

SELECT
    'returnNullBool',
    plbench('SELECT returnNullBool()', 200) as pldotnet,
    plbench('SELECT returnNullBoolV8()', 200) as plv8,
    plbench('SELECT returnNullBoolPython()', 200) as plpython;

SELECT
    'SELECT BooleanNullAnd(true, null)',
    plbench('SELECT BooleanNullAnd(true, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullAndV8(true, null)', 200) as plv8,
    plbench('SELECT BooleanNullAndPython(true, null)', 200) as plpython;

SELECT
    'BooleanNullAnd(null, true)',
    plbench('SELECT BooleanNullAnd(null, true)', 200) as pldotnet,
    plbench('SELECT BooleanNullAndV8(null, true)', 200) as plv8,
    plbench('SELECT BooleanNullAndPython(null, true)', 200) as plpython;

SELECT
    'BooleanNullAnd(false, null)',
    plbench('SELECT BooleanNullAnd(false, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullAndV8(false, null)', 200) as plv8,
    plbench('SELECT BooleanNullAndPython(false, null)', 200) as plpython;

SELECT
    'BooleanNullAnd(null, false)',
    plbench('SELECT BooleanNullAnd(null, false)', 200) as pldotnet,
    plbench('SELECT BooleanNullAndV8(null, false)', 200) as plv8,
    plbench('SELECT BooleanNullAndPython(null, false)', 200) as plpython;

SELECT
    'BooleanNullAnd(null, null)',
    plbench('SELECT BooleanNullAnd(null, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullAndV8(null, null)', 200) as plv8,
    plbench('SELECT BooleanNullAndPython(null, null)', 200) as plpython;

SELECT
    'BooleanNullOr(true, null)',
    plbench('SELECT BooleanNullOr(true, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullOrV8(true, null)', 200) as plv8,
    plbench('SELECT BooleanNullOrPython(true, null)', 200) as plpython;

SELECT
    'retuBooleanNullOr(null, true)rnBool',
    plbench('SELECT BooleanNullOr(null, true)', 200) as pldotnet,
    plbench('SELECT BooleanNullOrV8(null, true)', 200) as plv8,
    plbench('SELECT BooleanNullOrPython(null, true)', 200) as plpython;

SELECT
    'BooleanNullOr(false, null)',
    plbench('SELECT BooleanNullOr(false, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullOrV8(false, null)', 200) as plv8,
    plbench('SELECT BooleanNullOrPython(false, null)', 200) as plpython;

SELECT
    'BooleanNullOr(null, false)',
    plbench('SELECT BooleanNullOr(null, false)', 200) as pldotnet,
    plbench('SELECT BooleanNullOrV8(null, false)', 200) as plv8,
    plbench('SELECT BooleanNullOrPython(null, false)', 200) as plpython;

SELECT
    'BooleanNullOr(null, null)',
    plbench('SELECT BooleanNullOr(null, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullOrV8(null, null)', 200) as plv8,
    plbench('SELECT BooleanNullOrPython(null, null)', 200) as plpython;

SELECT
    'SELECT BooleanNullXor(true, null)',
    plbench('SELECT BooleanNullXor(true, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullXorV8(true, null)', 200) as plv8,
    plbench('SELECT BooleanNullXorPython(true, null)', 200) as plpython;

SELECT
    'BooleanNullXor(null, true)',
    plbench('SELECT BooleanNullXor(null, true)', 200) as pldotnet,
    plbench('SELECT BooleanNullXorV8(null, true)', 200) as plv8,
    plbench('SELECT BooleanNullXorPython(null, true)', 200) as plpython;

SELECT
    'BooleanNullXor(false, null)',
    plbench('SELECT BooleanNullXor(false, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullXorV8(false, null)', 200) as plv8,
    plbench('SELECT BooleanNullXorPython(false, null)', 200) as plpython;

SELECT
    'BooleanNullXor(null, false)',
    plbench('SELECT BooleanNullXor(null, false)', 200) as pldotnet,
    plbench('SELECT BooleanNullXorV8(null, false)', 200) as plv8,
    plbench('SELECT BooleanNullXorPython(null, false)', 200) as plpython;

SELECT
    'BooleanNullXor(null, null)',
    plbench('SELECT BooleanNullXor(null, null)', 200) as pldotnet,
    plbench('SELECT BooleanNullXorV8(null, null)', 200) as plv8,
    plbench('SELECT BooleanNullXorPython(null, null)', 200) as plpython;
