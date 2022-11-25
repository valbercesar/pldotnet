\i sql/benchmark/includes.sql

\i sql/testnullintegers.sql
\i sql/python/testnullintegers.sql
--- \i sql/java/testnullintegers.sql

\set runs 1000

SELECT
    'returnNullInt',
    plbench('SELECT returnNullInt()', :runs) as plcsharp,
    plbench('SELECT returnNullIntPython()', :runs) as plpython;
    --- plbench('SELECT returnNullIntJava()', :runs) as pljava,

SELECT
    'returnNullSmallInt',
    plbench('SELECT returnNullSmallInt()', :runs) as plcsharp,
    plbench('SELECT returnNullSmallIntPython()', :runs) as plpython;
    --- plbench('SELECT returnNullSmallInt1()', :runs) as pljava,


SELECT
    'returnNullBigInt',
    plbench('SELECT returnNullBigInt()', :runs) as plcsharp,
    plbench('SELECT returnNullBigIntPython()', :runs) as plpython;
    --- plbench('SELECT returnNullBigInt1()', :runs) as pljava;

SELECT
    'sumNullArgInt(null,null)',
    plbench('SELECT sumNullArgInt(null,null)', :runs) as plcsharp,
    plbench('SELECT sumNullArgIntPython(null,null)', :runs) as plpython;
    --- plbench('SELECT sumNullArgIntJava(null,null)', :runs) as pljava;

SELECT
    'sumNullArgInt(null,3)',
    plbench('SELECT sumNullArgInt(null,3)', :runs) as plcsharp,
    plbench('SELECT sumNullArgIntPython(null,3)', :runs) as plpython;
    --- plbench('SELECT sumNullArgIntJava(null,3)', :runs) as pljava;

SELECT
    'sumNullArgInt(3,null)',
    plbench('SELECT sumNullArgInt(3,null)', :runs) as plcsharp,
    plbench('SELECT sumNullArgIntPython(3,null)', :runs) as plpython;
    --- plbench('SELECT sumNullArgIntJava(3,null)', :runs) as pljava;

SELECT
    'sumNullArgInt(3,3)',
    plbench('SELECT sumNullArgInt(3,3)', :runs) as plcsharp,
    plbench('SELECT sumNullArgIntPython(3,3)', :runs) as plpython;
    --- plbench('SELECT sumNullArgIntJava(3,3)', :runs) as pljava;

SELECT
    'sumNullArgSmallInt(null,null)',
    plbench('SELECT sumNullArgSmallInt(null,null)', :runs) as plcsharp,
    plbench('SELECT sumNullArgSmallIntPython(null,null)', :runs) as plpython;
    --- plbench('SELECT sumNullArgSmallIntJava(null,null)', :runs) as pljava;

SELECT
    'sumNullArgSmallInt(null,CAST(101 AS smallint))',
    plbench('SELECT sumNullArgSmallInt(null,CAST(101 AS smallint))', :runs) as plcsharp,
    plbench('SELECT sumNullArgSmallIntPython(null,CAST(101 AS smallint))', :runs) as plpython;
    --- plbench('SELECT sumNullArgSmallIntJava(null,CAST(101 AS smallint))', :runs) as pljava;

SELECT
    'sumNullArgSmallInt(CAST(101 AS smallint),null)',
    plbench('SELECT sumNullArgSmallInt(CAST(101 AS smallint),null)', :runs) as plcsharp,
    plbench('SELECT sumNullArgSmallIntPython(CAST(101 AS smallint),null)', :runs) as plpython;
    --- plbench('SELECT sumNullArgSmallIntJava(CAST(101 AS smallint),null)', :runs) as pljava;

SELECT
    'sumNullArgSmallInt(CAST(101 AS smallint),CAST(101 AS smallint))',
    plbench('SELECT sumNullArgSmallInt(CAST(101 AS smallint),CAST(101 AS smallint))', :runs) as plcsharp,
    plbench('SELECT sumNullArgSmallIntPython(CAST(101 AS smallint),CAST(101 AS smallint))', :runs) as plpython;
    --- plbench('SELECT sumNullArgSmallIntJava(CAST(101 AS smallint),CAST(101 AS smallint))', :runs) as pljava;

SELECT
    'sumNullArgBigInt(null,null)',
    plbench('SELECT sumNullArgBigInt(null,null)', :runs) as plcsharp,
    plbench('SELECT sumNullArgBigIntPython(null,null)', :runs) as plpython;
    --- plbench('SELECT sumNullArgBigIntJava(null,null)', :runs) as pljava;

SELECT
    'sumNullArgBigInt(null,100)',
    plbench('SELECT sumNullArgBigInt(null,100)', :runs) as plcsharp,
    plbench('SELECT sumNullArgBigIntPython(null,100)', :runs) as plpython;
    --- plbench('SELECT sumNullArgBigIntJava(null,100)', :runs) as pljava;

SELECT
    'sumNullArgBigInt(9223372036854775707,null)',
    plbench('SELECT sumNullArgBigInt(9223372036854775707,null)', :runs) as plcsharp,
    plbench('SELECT sumNullArgBigIntPython(9223372036854775707,null)', :runs) as plpython;
    --- plbench('SELECT sumNullArgBigIntJava(9223372036854775707,null)', :runs) as pljava;

SELECT
    'sumNullArgBigInt(9223372036854775707,100)',
    plbench('SELECT sumNullArgBigInt(9223372036854775707,100)', :runs) as plcsharp,
    plbench('SELECT sumNullArgBigIntPython(9223372036854775707,100)', :runs) as plpython;
    --- plbench('SELECT sumNullArgBigIntJava(9223372036854775707,100)', :runs) as pljava;

SELECT
    'checkedSumNullArgInt(null,null)',
    plbench('SELECT checkedSumNullArgInt(null,null)', :runs) as plcsharp,
    plbench('SELECT checkedSumNullArgIntPython(null,null)', :runs) as plpython;
    --- plbench('SELECT checkedSumNullArgIntJava(null,null)', :runs) as pljava;

SELECT
    'checkedSumNullArgInt(null,3)',
    plbench('SELECT checkedSumNullArgInt(null,3)', :runs) as plcsharp,
    plbench('SELECT checkedSumNullArgIntPython(null,3)', :runs) as plpython;
    --- plbench('SELECT checkedSumNullArgIntJava(null,3)', :runs) as pljava;

SELECT
    'checkedSumNullArgInt(3,null)',
    plbench('SELECT checkedSumNullArgInt(3,null)', :runs) as plcsharp,
    plbench('SELECT checkedSumNullArgIntPython(3,null)', :runs) as plpython;
    --- plbench('SELECT checkedSumNullArgIntJava(3,null)', :runs) as pljava;

SELECT
    'checkedSumNullArgInt(3,3)',
    plbench('SELECT checkedSumNullArgInt(3,3)', :runs) as plcsharp,
    plbench('SELECT checkedSumNullArgIntPython(3,3)', :runs) as plpython;
    --- plbench('SELECT checkedSumNullArgIntJava(3,3)', :runs) as pljava;

SELECT
    'checkedSumNullArgSmallInt(null,null)',
    plbench('SELECT checkedSumNullArgSmallInt(null,null)', :runs) as plcsharp,
    plbench('SELECT checkedSumNullArgSmallIntPython(null,null)', :runs) as plpython;
    --- plbench('SELECT checkedSumNullArgSmallIntJava(null,null)', :runs) as pljava;

SELECT  
    'checkedSumNullArgSmallInt(null,CAST(133 AS smallint))',
    plbench('SELECT checkedSumNullArgSmallInt(null,CAST(133 AS smallint))', :runs) as plcsharp,
    plbench('SELECT checkedSumNullArgSmallIntPython(null,CAST(133 AS smallint))', :runs) as plpython;
    --- plbench('SELECT checkedSumNullArgSmallIntJava(null,CAST(133 AS smallint))', :runs) as pljava;

SELECT
    'checkedSumNullArgSmallInt(CAST(133 AS smallint),null)',
    plbench('SELECT checkedSumNullArgSmallInt(CAST(133 AS smallint),null)', :runs) as plcsharp,
    plbench('SELECT checkedSumNullArgSmallIntPython(CAST(133 AS smallint),null)', :runs) as plpython;
    --- plbench('SELECT checkedSumNullArgSmallIntJava(CAST(133 AS smallint),null)', :runs) as pljava;

SELECT
    'checkedSumNullArgSmallInt(CAST(133 AS smallint),CAST(133 AS smallint)',
    plbench('SELECT checkedSumNullArgSmallInt(CAST(133 AS smallint),CAST(133 AS smallint))', :runs) as plcsharp,
    plbench('SELECT checkedSumNullArgSmallIntPython(CAST(133 AS smallint),CAST(133 AS smallint))', :runs) as plpython;
    --- plbench('SELECT checkedSumNullArgSmallIntJava(CAST(133 AS smallint),CAST(133 AS smallint))', :runs) as pljava;

SELECT
    'checkedSumNullArgBigInt(null,null)',
    plbench('SELECT checkedSumNullArgBigInt(null,null)', :runs) as plcsharp,
    plbench('SELECT checkedSumNullArgBigIntPython(null,null)', :runs) as plpython;
    --- plbench('SELECT checkedSumNullArgBigIntJava(null,null)', :runs) as pljava;

SELECT
    'checkedSumNullArgBigInt(null,100)',
    plbench('SELECT checkedSumNullArgBigInt(null,100)', :runs) as plcsharp,
    plbench('SELECT checkedSumNullArgBigIntPython(null,100)', :runs) as plpython;
    --- plbench('SELECT checkedSumNullArgBigIntJava(null,100)', :runs) as pljava;

SELECT
    'checkedSumNullArgBigInt(9223372036854775707,null)',
    plbench('SELECT checkedSumNullArgBigInt(9223372036854775707,null)', :runs) as plcsharp,
    plbench('SELECT checkedSumNullArgBigIntPython(9223372036854775707,null)', :runs) as plpython;
    --- plbench('SELECT checkedSumNullArgBigIntJava(9223372036854775707,null)', :runs) as pljava;

SELECT
    'checkedSumNullArgBigInt(9223372036854775707,100)',
    plbench('SELECT checkedSumNullArgBigInt(9223372036854775707,100)', :runs) as plcsharp,
    plbench('SELECT checkedSumNullArgBigIntPython(9223372036854775707,100)', :runs) as plpython;
    --- plbench('SELECT checkedSumNullArgBigIntJava(9223372036854775707,100)', :runs) as pljava;

SELECT
    'checkedSumNullArgMixed(null,null,null)',
    plbench('SELECT checkedSumNullArgMixed(null,null,null)', :runs) as plcsharp,
    plbench('SELECT checkedSumNullArgMixedPython(null,null,null)', :runs) as plpython;
    --- plbench('SELECT checkedSumNullArgMixedJava(null,null,null)', :runs) as pljava;

SELECT
    'checkedSumNullArgMixed(null,CAST(1313 as smallint),null)',
    plbench('SELECT checkedSumNullArgMixed(null,CAST(1313 as smallint),null)', :runs) as plcsharp,
    plbench('SELECT checkedSumNullArgMixedPython(null,CAST(1313 as smallint),null)', :runs) as plpython;
    --- plbench('SELECT checkedSumNullArgMixedJava(null,CAST(1313 as smallint),null)', :runs) as pljava;

SELECT
    'checkedSumNullArgMixed(1313,null,null)',
    plbench('SELECT checkedSumNullArgMixed(1313,null,null)', :runs) as plcsharp,
    plbench('SELECT checkedSumNullArgMixedPython(1313,null,null)', :runs) as plpython;
    --- plbench('SELECT checkedSumNullArgMixedJava(1313,null,null)', :runs) as pljava;

SELECT
    'checkedSumNullArgMixed(null,null,3)',
    plbench('SELECT checkedSumNullArgMixed(null,null,3)', :runs) as plcsharp,
    plbench('SELECT checkedSumNullArgMixedPython(null,null,3)', :runs) as plpython;
    --- plbench('SELECT checkedSumNullArgMixedJava(null,null,3)', :runs) as pljava;
SELECT
    'checkedSumNullArgMixed(1313,CAST(1313 as smallint), 1313)',
    plbench('SELECT checkedSumNullArgMixed(1313,CAST(1313 as smallint), 1313)', :runs) as plcsharp,
    plbench('SELECT checkedSumNullArgMixedPython(1313,CAST(1313 as smallint), 1313)', :runs) as plpython;
    --- plbench('SELECT checkedSumNullArgMixedJava(1313,CAST(1313 as smallint), 1313)', :runs) as pljava;