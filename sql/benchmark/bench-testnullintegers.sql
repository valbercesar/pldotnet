\i sql/benchmark/includes.sql

\i sql/testnullintegers.sql
\i sql/v8javascript/testnullintegers.sql

SELECT
    'returnNullInt',
    plbench('SELECT returnNullInt()', 100) as pldotnet,
    plbench('SELECT returnNullIntV8()', 100) as plv8;

SELECT
    'returnNullSmallInt',
    plbench('SELECT returnNullSmallInt()', 100) as pldotnet,
    plbench('SELECT returnNullSmallIntV8()', 100) as plv8;

SELECT
    'returnNullBigInt',
    plbench('SELECT returnNullBigInt()', 100) as pldotnet,
    plbench('SELECT returnNullBigIntV8()', 100) as plv8;

SELECT
    'sumNullArgInt(null,null)',
    plbench('SELECT sumNullArgInt(null,null)', 100) as pldotnet,
    plbench('SELECT sumNullArgIntV8(null,null)', 100) as plv8;

SELECT
    'sumNullArgInt(null,3)',
    plbench('SELECT sumNullArgInt(null,3)', 100) as pldotnet,
    plbench('SELECT sumNullArgIntV8(null,3)', 100) as plv8;

SELECT
    'sumNullArgInt(3,null)',
    plbench('SELECT sumNullArgInt(3,null)', 100) as pldotnet,
    plbench('SELECT sumNullArgIntV8(3,null)', 100) as plv8;

SELECT
    'sumNullArgInt(3,3)',
    plbench('SELECT sumNullArgInt(3,3)', 100) as pldotnet,
    plbench('SELECT sumNullArgIntV8(3,3)', 100) as plv8;

SELECT
    'sumNullArgSmallInt(null,null)',
    plbench('SELECT sumNullArgSmallInt(null,null)', 100) as pldotnet,
    plbench('SELECT sumNullArgSmallIntV8(null,null)', 100) as plv8;

SELECT
    'sumNullArgSmallInt(null,CAST(101 AS smallint))',
    plbench('SELECT sumNullArgSmallInt(null,CAST(101 AS smallint))', 100) as pldotnet,
    plbench('SELECT sumNullArgSmallIntV8(null,CAST(101 AS smallint))', 100) as plv8;

SELECT
    'sumNullArgSmallIntCAST(101 AS smallint),null)',
    plbench('SELECT sumNullArgSmallInt(CAST(101 AS smallint),null)', 100) as pldotnet,
    plbench('SELECT sumNullArgSmallIntV8(CAST(101 AS smallint),null)', 100) as plv8;

SELECT
    'sumNullArgSmallInt(CAST(101 AS smallint),CAST(101 AS smallint))',
    plbench('SELECT sumNullArgSmallInt(CAST(101 AS smallint),CAST(101 AS smallint))', 100) as pldotnet,
    plbench('SELECT sumNullArgSmallIntV8(CAST(101 AS smallint),CAST(101 AS smallint))', 100) as plv8;

SELECT
    'sumNullArgBigInt(null,null)',
    plbench('SELECT sumNullArgBigInt(null,null)', 100) as pldotnet,
    plbench('SELECT sumNullArgBigIntV8(null,null)', 100) as plv8;

SELECT
    'sumNullArgBigInt(null,100)',
    plbench('SELECT sumNullArgBigInt(null,100)', 100) as pldotnet,
    plbench('SELECT sumNullArgBigIntV8(null,100)', 100) as plv8;

SELECT
    'sumNullArgBigInt(9223372036854775707,null)',
    plbench('SELECT sumNullArgBigInt(9223372036854775707,null)', 100) as pldotnet,
    plbench('SELECT sumNullArgBigIntV8(9223372036854775707,null)', 100) as plv8;

SELECT
    'sumNullArgBigInt(9223372036854775707,100)',
    plbench('SELECT sumNullArgBigInt(9223372036854775707,100)', 100) as pldotnet,
    plbench('SELECT sumNullArgBigIntV8(9223372036854775707,100)', 100) as plv8;

SELECT
    'checkedSumNullArgInt(null,null)',
    plbench('SELECT checkedSumNullArgInt(null,null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgIntV8(null,null)', 100) as plv8;

SELECT
    'checkedSumNullArgInt(null,3)',
    plbench('SELECT checkedSumNullArgInt(null,3)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgIntV8(null,3)', 100) as plv8;

SELECT
    'checkedSumNullArgInt(3,null)',
    plbench('SELECT checkedSumNullArgInt(3,null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgIntV8(3,null)', 100) as plv8;

SELECT
    'checkedSumNullArgInt(3,3)',
    plbench('SELECT checkedSumNullArgInt(3,3)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgIntV8(3,3)', 100) as plv8;

SELECT
    'checkedSumNullArgSmallInt(null,null)',
    plbench('SELECT checkedSumNullArgSmallInt(null,null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgSmallIntV8(null,null)', 100) as plv8;

SELECT
    'checkedSumNullArgSmallInt(null,CAST(133 AS smallint))',
    plbench('SELECT checkedSumNullArgSmallInt(null,CAST(133 AS smallint))', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgSmallIntV8(null,CAST(133 AS smallint))', 100) as plv8;

SELECT
    'checkedSumNullArgSmallInt(CAST(133 AS smallint),null)',
    plbench('SELECT checkedSumNullArgSmallInt(CAST(133 AS smallint),null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgSmallIntV8(CAST(133 AS smallint),null)', 100) as plv8;

SELECT
    'checkedSumNullArgSmallInt(CAST(133 AS smallint),CAST(133 AS smallint)',
    plbench('SELECT checkedSumNullArgSmallInt(CAST(133 AS smallint),CAST(133 AS smallint))', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgSmallIntV8(CAST(133 AS smallint),CAST(133 AS smallint))', 100) as plv8;

SELECT
    'checkedSumNullArgBigInt(null,null)',
    plbench('SELECT checkedSumNullArgBigInt(null,null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgBigIntV8(null,null)', 100) as plv8;

SELECT
    'checkedSumNullArgBigInt(null,100)',
    plbench('SELECT checkedSumNullArgBigInt(null,100)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgBigIntV8(null,100)', 100) as plv8;

SELECT
    'checkedSumNullArgBigInt(9223372036854775707,null)',
    plbench('SELECT checkedSumNullArgBigInt(9223372036854775707,null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgBigIntV8(9223372036854775707,null)', 100) as plv8;

SELECT
    'checkedSumNullArgBigInt(9223372036854775707,100)',
    plbench('SELECT checkedSumNullArgBigInt(9223372036854775707,100)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgBigIntV8(9223372036854775707,100)', 100) as plv8;

SELECT
    'checkedSumNullArgMixed(null,null,null)',
    plbench('SELECT checkedSumNullArgMixed(null,null,null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgMixedV8(null,null,null)', 100) as plv8;

SELECT
    'checkedSumNullArgMixed(null,CAST(1313 as smallint),null)',
    plbench('SELECT checkedSumNullArgMixed(null,CAST(1313 as smallint),null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgMixedV8(null,CAST(1313 as smallint),null)', 100) as plv8;

SELECT
    'checkedSumNullArgMixed(1313,null,null)',
    plbench('SELECT checkedSumNullArgMixed(1313,null,null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgMixedV8(1313,null,null)', 100) as plv8;

SELECT
    'checkedSumNullArgMixed(null,null,3)',
    plbench('SELECT checkedSumNullArgMixed(null,null,3)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgMixedV8(null,null,3)', 100) as plv8;

SELECT
    'checkedSumNullArgMixed(1313,CAST(1313 as smallint), 1313',
    plbench('SELECT checkedSumNullArgMixed(1313,CAST(1313 as smallint), 1313)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgMixedV8(1313,CAST(1313 as smallint), 1313)', 100) as plv8;
