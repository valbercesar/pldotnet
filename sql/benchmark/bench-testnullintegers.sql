\i sql/benchmark/includes.sql

\i sql/testnullintegers.sql
\i sql/v8javascript/testnullintegers.sql
\i sql/python/testnullintegers.sql

SELECT
    'returnNullInt',
    plbench('SELECT returnNullInt()', 100) as pldotnet,
    plbench('SELECT returnNullIntV8()', 100) as plv8,
    plbench('SELECT returnNullIntPython()', 100) as plpython;

SELECT
    'returnNullSmallInt',
    plbench('SELECT returnNullSmallInt()', 100) as pldotnet,
    plbench('SELECT returnNullSmallIntV8()', 100) as plv8,
    plbench('SELECT returnNullSmallIntPython()', 100) as plpython;

SELECT
    'returnNullBigInt',
    plbench('SELECT returnNullBigInt()', 100) as pldotnet,
    plbench('SELECT returnNullBigIntV8()', 100) as plv8,
    plbench('SELECT returnNullBigIntPython()', 100) as plpython;

SELECT
    'sumNullArgInt(null,null)',
    plbench('SELECT sumNullArgInt(null,null)', 100) as pldotnet,
    plbench('SELECT sumNullArgIntV8(null,null)', 100) as plv8,
    plbench('SELECT sumNullArgIntPython(null,null)', 100) as plpython;

SELECT
    'sumNullArgInt(null,3)',
    plbench('SELECT sumNullArgInt(null,3)', 100) as pldotnet,
    plbench('SELECT sumNullArgIntV8(null,3)', 100) as plv8,
    plbench('SELECT sumNullArgIntPython(null,3)', 100) as plpython;

SELECT
    'sumNullArgInt(3,null)',
    plbench('SELECT sumNullArgInt(3,null)', 100) as pldotnet,
    plbench('SELECT sumNullArgIntV8(3,null)', 100) as plv8,
    plbench('SELECT sumNullArgIntPython(3,null)', 100) as plpython;

SELECT
    'sumNullArgInt(3,3)',
    plbench('SELECT sumNullArgInt(3,3)', 100) as pldotnet,
    plbench('SELECT sumNullArgIntV8(3,3)', 100) as plv8,
    plbench('SELECT sumNullArgIntPython(3,3)', 100) as plpython;

SELECT
    'sumNullArgSmallInt(null,null)',
    plbench('SELECT sumNullArgSmallInt(null,null)', 100) as pldotnet,
    plbench('SELECT sumNullArgSmallIntV8(null,null)', 100) as plv8,
    plbench('SELECT sumNullArgSmallIntPython(null,null)', 100) as plpython;

SELECT
    'sumNullArgSmallInt(null,CAST(101 AS smallint))',
    plbench('SELECT sumNullArgSmallInt(null,CAST(101 AS smallint))', 100) as pldotnet,
    plbench('SELECT sumNullArgSmallIntV8(null,CAST(101 AS smallint))', 100) as plv8,
    plbench('SELECT sumNullArgSmallIntPython(null,CAST(101 AS smallint))', 100) as plpython;

SELECT
    'sumNullArgSmallIntCAST(101 AS smallint),null)',
    plbench('SELECT sumNullArgSmallInt(CAST(101 AS smallint),null)', 100) as pldotnet,
    plbench('SELECT sumNullArgSmallIntV8(CAST(101 AS smallint),null)', 100) as plv8,
    plbench('SELECT sumNullArgSmallIntPython(CAST(101 AS smallint),null)', 100) as plpython;

SELECT
    'sumNullArgSmallInt(CAST(101 AS smallint),CAST(101 AS smallint))',
    plbench('SELECT sumNullArgSmallInt(CAST(101 AS smallint),CAST(101 AS smallint))', 100) as pldotnet,
    plbench('SELECT sumNullArgSmallIntV8(CAST(101 AS smallint),CAST(101 AS smallint))', 100) as plv8,
    plbench('SELECT sumNullArgSmallIntPython(CAST(101 AS smallint),CAST(101 AS smallint))', 100) as plpython;

SELECT
    'sumNullArgBigInt(null,null)',
    plbench('SELECT sumNullArgBigInt(null,null)', 100) as pldotnet,
    plbench('SELECT sumNullArgBigIntV8(null,null)', 100) as plv8,
    plbench('SELECT sumNullArgBigIntPython(null,null)', 100) as plpython;

SELECT
    'sumNullArgBigInt(null,100)',
    plbench('SELECT sumNullArgBigInt(null,100)', 100) as pldotnet,
    plbench('SELECT sumNullArgBigIntV8(null,100)', 100) as plv8,
    plbench('SELECT sumNullArgBigIntPython(null,100)', 100) as plpython;

SELECT
    'sumNullArgBigInt(9223372036854775707,null)',
    plbench('SELECT sumNullArgBigInt(9223372036854775707,null)', 100) as pldotnet,
    plbench('SELECT sumNullArgBigIntV8(9223372036854775707,null)', 100) as plv8,
    plbench('SELECT sumNullArgBigIntPython(9223372036854775707,null)', 100) as plpython;

SELECT
    'sumNullArgBigInt(9223372036854775707,100)',
    plbench('SELECT sumNullArgBigInt(9223372036854775707,100)', 100) as pldotnet,
    plbench('SELECT sumNullArgBigIntV8(9223372036854775707,100)', 100) as plv8,
    plbench('SELECT sumNullArgBigIntPython(9223372036854775707,100)', 100) as plpython;

SELECT
    'checkedSumNullArgInt(null,null)',
    plbench('SELECT checkedSumNullArgInt(null,null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgIntV8(null,null)', 100) as plv8,
    plbench('SELECT checkedSumNullArgIntPython(null,null)', 100) as plpython;

SELECT
    'checkedSumNullArgInt(null,3)',
    plbench('SELECT checkedSumNullArgInt(null,3)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgIntV8(null,3)', 100) as plv8,
    plbench('SELECT checkedSumNullArgIntPython(null,3)', 100) as plpython;

SELECT
    'checkedSumNullArgInt(3,null)',
    plbench('SELECT checkedSumNullArgInt(3,null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgIntV8(3,null)', 100) as plv8,
    plbench('SELECT checkedSumNullArgIntPython(3,null)', 100) as plpython;

SELECT
    'checkedSumNullArgInt(3,3)',
    plbench('SELECT checkedSumNullArgInt(3,3)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgIntV8(3,3)', 100) as plv8,
    plbench('SELECT checkedSumNullArgIntPython(3,3)', 100) as plpython;

SELECT
    'checkedSumNullArgSmallInt(null,null)',
    plbench('SELECT checkedSumNullArgSmallInt(null,null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgSmallIntV8(null,null)', 100) as plv8,
    plbench('SELECT checkedSumNullArgSmallIntPython(null,null)', 100) as plpython;

SELECT
    'checkedSumNullArgSmallInt(null,CAST(133 AS smallint))',
    plbench('SELECT checkedSumNullArgSmallInt(null,CAST(133 AS smallint))', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgSmallIntV8(null,CAST(133 AS smallint))', 100) as plv8,
    plbench('SELECT checkedSumNullArgSmallIntPython(null,CAST(133 AS smallint))', 100) as plpython;

SELECT
    'checkedSumNullArgSmallInt(CAST(133 AS smallint),null)',
    plbench('SELECT checkedSumNullArgSmallInt(CAST(133 AS smallint),null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgSmallIntV8(CAST(133 AS smallint),null)', 100) as plv8,
    plbench('SELECT checkedSumNullArgSmallIntPython(CAST(133 AS smallint),null)', 100) as plpython;

SELECT
    'checkedSumNullArgSmallInt(CAST(133 AS smallint),CAST(133 AS smallint)',
    plbench('SELECT checkedSumNullArgSmallInt(CAST(133 AS smallint),CAST(133 AS smallint))', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgSmallIntV8(CAST(133 AS smallint),CAST(133 AS smallint))', 100) as plv8,
    plbench('SELECT checkedSumNullArgSmallIntPython(CAST(133 AS smallint),CAST(133 AS smallint))', 100) as plpython;

SELECT
    'checkedSumNullArgBigInt(null,null)',
    plbench('SELECT checkedSumNullArgBigInt(null,null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgBigIntV8(null,null)', 100) as plv8,
    plbench('SELECT checkedSumNullArgBigIntPython(null,null)', 100) as plpython;

SELECT
    'checkedSumNullArgBigInt(null,100)',
    plbench('SELECT checkedSumNullArgBigInt(null,100)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgBigIntV8(null,100)', 100) as plv8,
    plbench('SELECT checkedSumNullArgBigIntPython(null,100)', 100) as plpython;

SELECT
    'checkedSumNullArgBigInt(9223372036854775707,null)',
    plbench('SELECT checkedSumNullArgBigInt(9223372036854775707,null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgBigIntV8(9223372036854775707,null)', 100) as plv8,
    plbench('SELECT checkedSumNullArgBigIntPython(9223372036854775707,null)', 100) as plpython;

SELECT
    'checkedSumNullArgBigInt(9223372036854775707,100)',
    plbench('SELECT checkedSumNullArgBigInt(9223372036854775707,100)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgBigIntV8(9223372036854775707,100)', 100) as plv8,
    plbench('SELECT checkedSumNullArgBigIntPython(9223372036854775707,100)', 100) as plpython;

SELECT
    'checkedSumNullArgMixed(null,null,null)',
    plbench('SELECT checkedSumNullArgMixed(null,null,null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgMixedV8(null,null,null)', 100) as plv8,
    plbench('SELECT checkedSumNullArgMixedPython(null,null,null)', 100) as plpython;

SELECT
    'checkedSumNullArgMixed(null,CAST(1313 as smallint),null)',
    plbench('SELECT checkedSumNullArgMixed(null,CAST(1313 as smallint),null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgMixedV8(null,CAST(1313 as smallint),null)', 100) as plv8,
    plbench('SELECT checkedSumNullArgMixedPython(null,CAST(1313 as smallint),null)', 100) as plpython;

SELECT
    'checkedSumNullArgMixed(1313,null,null)',
    plbench('SELECT checkedSumNullArgMixed(1313,null,null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgMixedV8(1313,null,null)', 100) as plv8,
    plbench('SELECT checkedSumNullArgMixedPython(1313,null,null)', 100) as plpython;

SELECT
    'checkedSumNullArgMixed(null,null,3)',
    plbench('SELECT checkedSumNullArgMixed(null,null,3)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgMixedV8(null,null,3)', 100) as plv8,
    plbench('SELECT checkedSumNullArgMixedPython(null,null,3)', 100) as plpython;

SELECT
    'checkedSumNullArgMixed(1313,CAST(1313 as smallint), 1313',
    plbench('SELECT checkedSumNullArgMixed(1313,CAST(1313 as smallint), 1313)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgMixedV8(1313,CAST(1313 as smallint), 1313)', 100) as plv8,
    plbench('SELECT checkedSumNullArgMixedPython(1313,CAST(1313 as smallint), 1313)', 100) as plpython;
