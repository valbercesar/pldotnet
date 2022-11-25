\i sql/benchmark/includes.sql

\i sql/testintegers.sql
\i sql/python/testintegers.sql
--- \i sql/java/testintegers.sql

\set runs 1000

SELECT
    'maxSmallInt',
    plbench('SELECT maxSmallInt()', :runs) as plcsharp,
    plbench('SELECT maxSmallIntPython()', :runs) as plpython;
    --- plbench('SELECT maxSmallIntJava()', :runs) as pljava;

SELECT
    'sum2SmallInt',
    plbench('SELECT sum2SmallInt(CAST(100 AS smallint), CAST(101 AS smallint))', :runs) as plcsharp,
    plbench('SELECT sum2SmallIntPython(CAST(100 AS smallint), CAST(101 AS smallint))', :runs) as plpython;
    --- plbench('SELECT sum2SmallIntJava(CAST(100 AS smallint), CAST(101 AS smallint))', :runs) as pljava;

SELECT
    'maxInteger',
    plbench('SELECT maxInteger()', :runs) as plcsharp,
    plbench('SELECT maxIntegerPython()', :runs) as plpython;
    --- plbench('SELECT maxIntegerJava()', :runs) as pljava;

SELECT
    'sum2Integer',
    plbench('SELECT sum2Integer(32770, 100)', :runs) as plcsharp,
    plbench('SELECT sum2IntegerPython(32770, 100)', :runs) as plpython;
    --- plbench('SELECT sum2IntegerJava(32770, 100)', :runs) as pljava;

SELECT
    'maxBigInt',
    plbench('SELECT maxBigInt()', :runs) as plcsharp,
    plbench('SELECT maxBigIntPython()', :runs) as plpython;
    --- plbench('SELECT maxBigIntJava()', :runs) as pljava;

SELECT
    'sum2BigInt',
    plbench('SELECT sum2BigInt(9223372036854775707, 100)', :runs) as plcsharp,
    plbench('SELECT sum2BigIntPython(9223372036854775707, 100)', :runs) as plpython;
    --- plbench('SELECT sum2BigIntJava(9223372036854775707, 100)', :runs) as pljava;

SELECT
    'mixedBigInt',
    plbench('SELECT mixedBigInt(32767,  2147483647, 100)', :runs) as plcsharp,
    plbench('SELECT mixedBigIntPython(32767,  2147483647, 100)', :runs) as plpython;
    --- plbench('SELECT mixedBigIntJava(32767,  2147483647, 100)', :runs) as pljava;

SELECT
    'mixedInt',
    plbench('SELECT mixedInt(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', :runs) as plcsharp,
    plbench('SELECT mixedIntPython(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', :runs) as ppython;
    --- plbench('SELECT mixedIntJava(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', :runs) as pljava;