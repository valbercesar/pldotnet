\i sql/benchmark/includes.sql

\i sql/testintegers.sql
\i sql/v8javascript/testintegers.sql

SELECT
    'maxSmallInt',
    plbench('SELECT maxSmallInt()', 100) as pldotnet,
    plbench('SELECT maxSmallIntV8()', 100) as plv8;

SELECT
    'sum2SmallInt',
    plbench('SELECT sum2SmallInt(CAST(100 AS smallint), CAST(101 AS smallint))', 100) as pldotnet,
    plbench('SELECT sum2SmallIntV8(CAST(100 AS smallint), CAST(101 AS smallint))', 100) as plv8;

SELECT
    'maxInteger',
    plbench('SELECT maxInteger()', 100) as pldotnet,
    plbench('SELECT maxIntegerV8()', 100) as plv8;

SELECT
    'sum2Integer',
    plbench('SELECT sum2Integer(32770, 100)', 100) as pldotnet,
    plbench('SELECT sum2IntegerV8(32770, 100)', 100) as plv8;

SELECT
    'maxBigInt',
    plbench('SELECT maxBigInt()', 100) as pldotnet,
    plbench('SELECT maxBigIntV8()', 100) as plv8;

SELECT
    'sum2BigInt',
    plbench('SELECT sum2BigInt(9223372036854775707, 100)', 100) as pldotnet,
    plbench('SELECT sum2BigIntV8(9223372036854775707, 100)', 100) as plv8;

SELECT
    'mixedBigInt',
    plbench('SELECT mixedBigInt(32767,  2147483647, 100)', 100) as pldotnet,
    plbench('SELECT mixedBigIntV8(32767,  2147483647, 100)', 100) as plv8;

SELECT
    'mixedInt',
    plbench('SELECT mixedInt(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', 300) as pldotnet,
    plbench('SELECT mixedIntV8(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', 300) as plv8
