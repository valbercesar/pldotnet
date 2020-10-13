\i sql/benchmark/includes.sql

\i sql/testintegers.sql
\i sql/v8javascript/testintegers.sql
\i sql/python/testintegers.sql

SELECT
    'maxSmallInt',
    plbench('SELECT maxSmallInt()', 500) as pldotnet,
    plbench('SELECT maxSmallIntV8()', 500) as plv8,
    plbench('SELECT maxSmallIntPython()', 500) as plpython;

SELECT
    'sum2SmallInt',
    plbench('SELECT sum2SmallInt(CAST(100 AS smallint), CAST(101 AS smallint))', 500) as pldotnet,
    plbench('SELECT sum2SmallIntV8(CAST(100 AS smallint), CAST(101 AS smallint))', 500) as plv8,
    plbench('SELECT sum2SmallIntPython(CAST(100 AS smallint), CAST(101 AS smallint))', 500) as plpython;

SELECT
    'maxInteger',
    plbench('SELECT maxInteger()', 500) as pldotnet,
    plbench('SELECT maxIntegerV8()', 500) as plv8,
    plbench('SELECT maxIntegerPython()', 500) as plpython;

SELECT
    'sum2Integer',
    plbench('SELECT sum2Integer(32770, 100)', 500) as pldotnet,
    plbench('SELECT sum2IntegerV8(32770, 100)', 500) as plv8,
    plbench('SELECT sum2IntegerPython(32770, 100)', 500) as plpython;

SELECT
    'maxBigInt',
    plbench('SELECT maxBigInt()', 500) as pldotnet,
    plbench('SELECT maxBigIntV8()', 500) as plv8,
    plbench('SELECT maxBigIntPython()', 500) as plpython;

SELECT
    'sum2BigInt',
    plbench('SELECT sum2BigInt(9223372036854775707, 100)', 500) as pldotnet,
    plbench('SELECT sum2BigIntV8(9223372036854775707, 100)', 500) as plv8,
    plbench('SELECT sum2BigIntPython(9223372036854775707, 100)', 500) as plpython;

SELECT
    'mixedBigInt',
    plbench('SELECT mixedBigInt(32767,  2147483647, 100)', 500) as pldotnet,
    plbench('SELECT mixedBigIntV8(32767,  2147483647, 100)', 500) as plv8,
    plbench('SELECT mixedBigIntPython(32767,  2147483647, 100)', 500) as plpython;

SELECT
    'mixedInt',
    plbench('SELECT mixedInt(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', 300) as pldotnet,
    plbench('SELECT mixedIntV8(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', 300) as plv8,
    plbench('SELECT mixedIntPython(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', 300) as plpython;
