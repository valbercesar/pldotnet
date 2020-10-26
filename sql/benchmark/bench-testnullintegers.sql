\i sql/benchmark/includes.sql

\i sql/testnullintegers.sql
\i sql/v8javascript/testnullintegers.sql
\i sql/python/testnullintegers.sql
\i sql/pgsql/testnullintegers.sql
\i sql/java/testnullintegers.sql
\i sql/perl/testnullintegers.sql

SELECT
    'returnNullInt',
    plbench('SELECT returnNullInt()', 100) as pldotnet,
    plbench('SELECT returnNullIntV8()', 100) as plv8,
    plbench('SELECT returnNullIntPython()', 100) as plpython,
    plbench('SELECT returnNullIntPg()', 100) as plpgsql,
    plbench('SELECT returnNullIntJava()', 100) as pljava,
    plbench('SELECT returnNullIntPerl()', 100) as plperl;

SELECT
    'returnNullSmallInt',
    plbench('SELECT returnNullSmallInt()', 100) as pldotnet,
    plbench('SELECT returnNullSmallIntV8()', 100) as plv8,
    plbench('SELECT returnNullSmallIntPython()', 100) as plpython,
    plbench('SELECT returnNullSmallIntPg()', 100) as plpgsql,
    plbench('SELECT returnNullSmallIntJava()', 100) as pljava,
    plbench('SELECT returnNullSmallIntPerl()', 100) as plperl;

SELECT
    'returnNullBigInt',
    plbench('SELECT returnNullBigInt()', 100) as pldotnet,
    plbench('SELECT returnNullBigIntV8()', 100) as plv8,
    plbench('SELECT returnNullBigIntPython()', 100) as plpython,
    plbench('SELECT returnNullBigIntPg()', 100) as plpgsql,
    plbench('SELECT returnNullBigIntJava()', 100) as pljava,
    plbench('SELECT returnNullBigIntPerl()', 100) as plperl;

SELECT
    'sumNullArgInt(null,null)',
    plbench('SELECT sumNullArgInt(null,null)', 100) as pldotnet,
    plbench('SELECT sumNullArgIntV8(null,null)', 100) as plv8,
    plbench('SELECT sumNullArgIntPython(null,null)', 100) as plpython,
    plbench('SELECT sumNullArgIntPg(null,null)', 100) as plpgsql,
    plbench('SELECT sumNullArgIntJava(null,null)', 100) as pljava,
    plbench('SELECT sumNullArgIntPerl(null,null)', 100) as plperl;

SELECT
    'sumNullArgInt(null,3)',
    plbench('SELECT sumNullArgInt(null,3)', 100) as pldotnet,
    plbench('SELECT sumNullArgIntV8(null,3)', 100) as plv8,
    plbench('SELECT sumNullArgIntPython(null,3)', 100) as plpython,
    plbench('SELECT sumNullArgIntPg(null,3)', 100) as plpgsql,
    plbench('SELECT sumNullArgIntJava(null,3)', 100) as pljava,
    plbench('SELECT sumNullArgIntPerl(null,3)', 100) as plperl;

SELECT
    'sumNullArgInt(3,null)',
    plbench('SELECT sumNullArgInt(3,null)', 100) as pldotnet,
    plbench('SELECT sumNullArgIntV8(3,null)', 100) as plv8,
    plbench('SELECT sumNullArgIntPython(3,null)', 100) as plpython,
    plbench('SELECT sumNullArgIntPg(3,null)', 100) as plpgsql,
    plbench('SELECT sumNullArgIntJava(3,null)', 100) as pljava,
    plbench('SELECT sumNullArgIntPerl(3,null)', 100) as plperl;

SELECT
    'sumNullArgInt(3,3)',
    plbench('SELECT sumNullArgInt(3,3)', 100) as pldotnet,
    plbench('SELECT sumNullArgIntV8(3,3)', 100) as plv8,
    plbench('SELECT sumNullArgIntPython(3,3)', 100) as plpython,
    plbench('SELECT sumNullArgIntPg(3,3)', 100) as plpgsql,
    plbench('SELECT sumNullArgIntJava(3,3)', 100) as pljava,
    plbench('SELECT sumNullArgIntPerl(3,3)', 100) as plperl;

SELECT
    'sumNullArgSmallInt(null,null)',
    plbench('SELECT sumNullArgSmallInt(null,null)', 100) as pldotnet,
    plbench('SELECT sumNullArgSmallIntV8(null,null)', 100) as plv8,
    plbench('SELECT sumNullArgSmallIntPython(null,null)', 100) as plpython,
    plbench('SELECT sumNullArgSmallIntPg(null,null)', 100) as plpgsql,
    plbench('SELECT sumNullArgSmallIntJava(null,null)', 100) as pljava,
    plbench('SELECT sumNullArgSmallIntPerl(null,null)', 100) as plperl;

SELECT
    'sumNullArgSmallInt(null,CAST(101 AS smallint))',
    plbench('SELECT sumNullArgSmallInt(null,CAST(101 AS smallint))', 100) as pldotnet,
    plbench('SELECT sumNullArgSmallIntV8(null,CAST(101 AS smallint))', 100) as plv8,
    plbench('SELECT sumNullArgSmallIntPython(null,CAST(101 AS smallint))', 100) as plpython,
    plbench('SELECT sumNullArgSmallIntPg(null,CAST(101 AS smallint))', 100) as plpgsql,
    plbench('SELECT sumNullArgSmallIntJava(null,CAST(101 AS smallint))', 100) as pljava,
    plbench('SELECT sumNullArgSmallIntPerl(null,CAST(101 AS smallint))', 100) as plperl;

SELECT
    'sumNullArgSmallInt(CAST(101 AS smallint),null)',
    plbench('SELECT sumNullArgSmallInt(CAST(101 AS smallint),null)', 100) as pldotnet,
    plbench('SELECT sumNullArgSmallIntV8(CAST(101 AS smallint),null)', 100) as plv8,
    plbench('SELECT sumNullArgSmallIntPython(CAST(101 AS smallint),null)', 100) as plpython,
    plbench('SELECT sumNullArgSmallIntPg(CAST(101 AS smallint),null)', 100) as plpgsql,
    plbench('SELECT sumNullArgSmallIntJava(CAST(101 AS smallint),null)', 100) as pljava,
    plbench('SELECT sumNullArgSmallIntPerl(CAST(101 AS smallint),null)', 100) as plperl;

SELECT
    'sumNullArgSmallInt(CAST(101 AS smallint),CAST(101 AS smallint))',
    plbench('SELECT sumNullArgSmallInt(CAST(101 AS smallint),CAST(101 AS smallint))', 100) as pldotnet,
    plbench('SELECT sumNullArgSmallIntV8(CAST(101 AS smallint),CAST(101 AS smallint))', 100) as plv8,
    plbench('SELECT sumNullArgSmallIntPython(CAST(101 AS smallint),CAST(101 AS smallint))', 100) as plpython,
    plbench('SELECT sumNullArgSmallIntPg(CAST(101 AS smallint),CAST(101 AS smallint))', 100) as plpgsql,
    plbench('SELECT sumNullArgSmallIntJava(CAST(101 AS smallint),CAST(101 AS smallint))', 100) as pljava,
    plbench('SELECT sumNullArgSmallIntPerl(CAST(101 AS smallint),CAST(101 AS smallint))', 100) as plperl;

SELECT
    'sumNullArgBigInt(null,null)',
    plbench('SELECT sumNullArgBigInt(null,null)', 100) as pldotnet,
    plbench('SELECT sumNullArgBigIntV8(null,null)', 100) as plv8,
    plbench('SELECT sumNullArgBigIntPython(null,null)', 100) as plpython,
    plbench('SELECT sumNullArgBigIntPg(null,null)', 100) as plpgsql,
    plbench('SELECT sumNullArgBigIntJava(null,null)', 100) as pljava,
    plbench('SELECT sumNullArgBigIntPerl(null,null)', 100) as plperl;

SELECT
    'sumNullArgBigInt(null,100)',
    plbench('SELECT sumNullArgBigInt(null,100)', 100) as pldotnet,
    plbench('SELECT sumNullArgBigIntV8(null,100)', 100) as plv8,
    plbench('SELECT sumNullArgBigIntPython(null,100)', 100) as plpython,
    plbench('SELECT sumNullArgBigIntPg(null,100)', 100) as plpgsql,
    plbench('SELECT sumNullArgBigIntJava(null,100)', 100) as pljava,
    plbench('SELECT sumNullArgBigIntPerl(null,100)', 100) as plperl;

SELECT
    'sumNullArgBigInt(9223372036854775707,null)',
    plbench('SELECT sumNullArgBigInt(9223372036854775707,null)', 100) as pldotnet,
    plbench('SELECT sumNullArgBigIntV8(9223372036854775707,null)', 100) as plv8,
    plbench('SELECT sumNullArgBigIntPython(9223372036854775707,null)', 100) as plpython,
    plbench('SELECT sumNullArgBigIntPg(9223372036854775707,null)', 100) as plpgsql,
    plbench('SELECT sumNullArgBigIntJava(9223372036854775707,null)', 100) as pljava;
    /*plbench('SELECT sumNullArgBigIntPerl(9223372036854775707,null)', 100) as plperl;*/

SELECT
    'sumNullArgBigInt(9223372036854775707,100)',
    plbench('SELECT sumNullArgBigInt(9223372036854775707,100)', 100) as pldotnet,
    plbench('SELECT sumNullArgBigIntV8(9223372036854775707,100)', 100) as plv8,
    plbench('SELECT sumNullArgBigIntPython(9223372036854775707,100)', 100) as plpython,
    plbench('SELECT sumNullArgBigIntPg(9223372036854775707,100)', 100) as plpgsql,
    plbench('SELECT sumNullArgBigIntJava(9223372036854775707,100)', 100) as pljava,
    plbench('SELECT sumNullArgBigIntPerl(9223372036854775707,100)', 100) as plperl;

SELECT
    'checkedSumNullArgInt(null,null)',
    plbench('SELECT checkedSumNullArgInt(null,null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgIntV8(null,null)', 100) as plv8,
    plbench('SELECT checkedSumNullArgIntPython(null,null)', 100) as plpython,
    plbench('SELECT checkedSumNullArgIntPg(null,null)', 100) as plpgsql,
    plbench('SELECT checkedSumNullArgIntJava(null,null)', 100) as pljava,
    plbench('SELECT checkedSumNullArgIntPerl(null,null)', 100) as plperl;

SELECT
    'checkedSumNullArgInt(null,3)',
    plbench('SELECT checkedSumNullArgInt(null,3)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgIntV8(null,3)', 100) as plv8,
    plbench('SELECT checkedSumNullArgIntPython(null,3)', 100) as plpython,
    plbench('SELECT checkedSumNullArgIntPg(null,3)', 100) as plpgsql,
    plbench('SELECT checkedSumNullArgIntJava(null,3)', 100) as pljava,
    plbench('SELECT checkedSumNullArgIntPerl(null,3)', 100) as plperl;

SELECT
    'checkedSumNullArgInt(3,null)',
    plbench('SELECT checkedSumNullArgInt(3,null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgIntV8(3,null)', 100) as plv8,
    plbench('SELECT checkedSumNullArgIntPython(3,null)', 100) as plpython,
    plbench('SELECT checkedSumNullArgIntPg(3,null)', 100) as plpgsql,
    plbench('SELECT checkedSumNullArgIntJava(3,null)', 100) as pljava,
    plbench('SELECT checkedSumNullArgIntPerl(3,null)', 100) as plperl;

SELECT
    'checkedSumNullArgInt(3,3)',
    plbench('SELECT checkedSumNullArgInt(3,3)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgIntV8(3,3)', 100) as plv8,
    plbench('SELECT checkedSumNullArgIntPython(3,3)', 100) as plpython,
    plbench('SELECT checkedSumNullArgIntPg(3,3)', 100) as plpgsql,
    plbench('SELECT checkedSumNullArgIntJava(3,3)', 100) as pljava,
    plbench('SELECT checkedSumNullArgIntPerl(3,3)', 100) as plperl;

SELECT
    'checkedSumNullArgSmallInt(null,null)',
    plbench('SELECT checkedSumNullArgSmallInt(null,null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgSmallIntV8(null,null)', 100) as plv8,
    plbench('SELECT checkedSumNullArgSmallIntPython(null,null)', 100) as plpython,
    plbench('SELECT checkedSumNullArgSmallIntPg(null,null)', 100) as plpgsql,
    plbench('SELECT checkedSumNullArgSmallIntJava(null,null)', 100) as pljava,
    plbench('SELECT checkedSumNullArgSmallIntPerl(null,null)', 100) as plperl;

SELECT  
    'checkedSumNullArgSmallInt(null,CAST(133 AS smallint))',
    plbench('SELECT checkedSumNullArgSmallInt(null,CAST(133 AS smallint))', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgSmallIntV8(null,CAST(133 AS smallint))', 100) as plv8,
    plbench('SELECT checkedSumNullArgSmallIntPython(null,CAST(133 AS smallint))', 100) as plpython,
    plbench('SELECT checkedSumNullArgSmallIntPg(null,CAST(133 AS smallint))', 100) as plpgsql,
    plbench('SELECT checkedSumNullArgSmallIntJava(null,CAST(133 AS smallint))', 100) as pljava,
    plbench('SELECT checkedSumNullArgSmallIntPerl(null,CAST(133 AS smallint))', 100) as plperl;

SELECT
    'checkedSumNullArgSmallInt(CAST(133 AS smallint),null)',
    plbench('SELECT checkedSumNullArgSmallInt(CAST(133 AS smallint),null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgSmallIntV8(CAST(133 AS smallint),null)', 100) as plv8,
    plbench('SELECT checkedSumNullArgSmallIntPython(CAST(133 AS smallint),null)', 100) as plpython,
    plbench('SELECT checkedSumNullArgSmallIntPg(CAST(133 AS smallint),null)', 100) as plpgsql,
    plbench('SELECT checkedSumNullArgSmallIntJava(CAST(133 AS smallint),null)', 100) as pljava,
    plbench('SELECT checkedSumNullArgSmallIntPerl(CAST(133 AS smallint),null)', 100) as plperl;

SELECT
    'checkedSumNullArgSmallInt(CAST(133 AS smallint),CAST(133 AS smallint)',
    plbench('SELECT checkedSumNullArgSmallInt(CAST(133 AS smallint),CAST(133 AS smallint))', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgSmallIntV8(CAST(133 AS smallint),CAST(133 AS smallint))', 100) as plv8,
    plbench('SELECT checkedSumNullArgSmallIntPython(CAST(133 AS smallint),CAST(133 AS smallint))', 100) as plpython,
    plbench('SELECT checkedSumNullArgSmallIntPg(CAST(133 AS smallint),CAST(133 AS smallint))', 100) as plpgsql,
    plbench('SELECT checkedSumNullArgSmallIntJava(CAST(133 AS smallint),CAST(133 AS smallint))', 100) as pljava,
    plbench('SELECT checkedSumNullArgSmallIntPerl(CAST(133 AS smallint),CAST(133 AS smallint))', 100) as plperl;

SELECT
    'checkedSumNullArgBigInt(null,null)',
    plbench('SELECT checkedSumNullArgBigInt(null,null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgBigIntV8(null,null)', 100) as plv8,
    plbench('SELECT checkedSumNullArgBigIntPython(null,null)', 100) as plpython,
    plbench('SELECT checkedSumNullArgBigIntPg(null,null)', 100) as plpgsql,
    plbench('SELECT checkedSumNullArgBigIntJava(null,null)', 100) as pljava,
    plbench('SELECT checkedSumNullArgBigIntPerl(null,null)', 100) as plperl;

SELECT
    'checkedSumNullArgBigInt(null,100)',
    plbench('SELECT checkedSumNullArgBigInt(null,100)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgBigIntV8(null,100)', 100) as plv8,
    plbench('SELECT checkedSumNullArgBigIntPython(null,100)', 100) as plpython,
    plbench('SELECT checkedSumNullArgBigIntPg(null,100)', 100) as plpgsql,
    plbench('SELECT checkedSumNullArgBigIntJava(null,100)', 100) as pljava,
    plbench('SELECT checkedSumNullArgBigIntPerl(null,100)', 100) as plperl;

SELECT
    'checkedSumNullArgBigInt(9223372036854775707,null)',
    plbench('SELECT checkedSumNullArgBigInt(9223372036854775707,null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgBigIntV8(9223372036854775707,null)', 100) as plv8,
    plbench('SELECT checkedSumNullArgBigIntPython(9223372036854775707,null)', 100) as plpython,
    plbench('SELECT checkedSumNullArgBigIntPg(9223372036854775707,null)', 100) as plpgsql,
    plbench('SELECT checkedSumNullArgBigIntJava(9223372036854775707,null)', 100) as pljava,
    plbench('SELECT checkedSumNullArgBigIntPerl(9223372036854775707,null)', 100) as plperl;

SELECT
    'checkedSumNullArgBigInt(9223372036854775707,100)',
    plbench('SELECT checkedSumNullArgBigInt(9223372036854775707,100)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgBigIntV8(9223372036854775707,100)', 100) as plv8,
    plbench('SELECT checkedSumNullArgBigIntPython(9223372036854775707,100)', 100) as plpython,
    plbench('SELECT checkedSumNullArgBigIntPg(9223372036854775707,100)', 100) as plpgsql,
    plbench('SELECT checkedSumNullArgBigIntJava(9223372036854775707,100)', 100) as pljava,
    plbench('SELECT checkedSumNullArgBigIntPerl(9223372036854775707,100)', 100) as plperl;

SELECT
    'checkedSumNullArgMixed(null,null,null)',
    plbench('SELECT checkedSumNullArgMixed(null,null,null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgMixedV8(null,null,null)', 100) as plv8,
    plbench('SELECT checkedSumNullArgMixedPython(null,null,null)', 100) as plpython,
    plbench('SELECT checkedSumNullArgMixedPg(null,null,null)', 100) as plpgsql,
    plbench('SELECT checkedSumNullArgMixedJava(null,null,null)', 100) as pljava,
    plbench('SELECT checkedSumNullArgMixedPerl(null,null,null)', 100) as plperl;

SELECT
    'checkedSumNullArgMixed(null,CAST(1313 as smallint),null)',
    plbench('SELECT checkedSumNullArgMixed(null,CAST(1313 as smallint),null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgMixedV8(null,CAST(1313 as smallint),null)', 100) as plv8,
    plbench('SELECT checkedSumNullArgMixedPython(null,CAST(1313 as smallint),null)', 100) as plpython,
    plbench('SELECT checkedSumNullArgMixedPg(null,CAST(1313 as smallint),null)', 100) as plpgsql,
    plbench('SELECT checkedSumNullArgMixedJava(null,CAST(1313 as smallint),null)', 100) as pljava,
    plbench('SELECT checkedSumNullArgMixedPerl(null,CAST(1313 as smallint),null)', 100) as plperl;

SELECT
    'checkedSumNullArgMixed(1313,null,null)',
    plbench('SELECT checkedSumNullArgMixed(1313,null,null)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgMixedV8(1313,null,null)', 100) as plv8,
    plbench('SELECT checkedSumNullArgMixedPython(1313,null,null)', 100) as plpython,
    plbench('SELECT checkedSumNullArgMixedPg(1313,null,null)', 100) as plpgsql,
    plbench('SELECT checkedSumNullArgMixedJava(1313,null,null)', 100) as pljava,
    plbench('SELECT checkedSumNullArgMixedPerl(1313,null,null)', 100) as plperl;

SELECT
    'checkedSumNullArgMixed(null,null,3)',
    plbench('SELECT checkedSumNullArgMixed(null,null,3)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgMixedV8(null,null,3)', 100) as plv8,
    plbench('SELECT checkedSumNullArgMixedPython(null,null,3)', 100) as plpython,
    plbench('SELECT checkedSumNullArgMixedPg(null,null,3)', 100) as plpgsql,
    plbench('SELECT checkedSumNullArgMixedJava(null,null,3)', 100) as pljava,
    plbench('SELECT checkedSumNullArgMixedPerl(null,null,3)', 100) as plperl;

SELECT
    'checkedSumNullArgMixed(1313,CAST(1313 as smallint), 1313)',
    plbench('SELECT checkedSumNullArgMixed(1313,CAST(1313 as smallint), 1313)', 100) as pldotnet,
    plbench('SELECT checkedSumNullArgMixedV8(1313,CAST(1313 as smallint), 1313)', 100) as plv8,
    plbench('SELECT checkedSumNullArgMixedPython(1313,CAST(1313 as smallint), 1313)', 100) as plpython,
    plbench('SELECT checkedSumNullArgMixedPg(1313,CAST(1313 as smallint), 1313)', 100) as plpgsql,
    plbench('SELECT checkedSumNullArgMixedJava(1313,CAST(1313 as smallint), 1313)', 100) as pljava,
    plbench('SELECT checkedSumNullArgMixedPerl(1313,CAST(1313 as smallint), 1313)', 100) as plperl;
