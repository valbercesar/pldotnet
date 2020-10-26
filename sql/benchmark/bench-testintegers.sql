\i sql/benchmark/includes.sql

\i sql/testintegers.sql
\i sql/v8javascript/testintegers.sql
\i sql/python/testintegers.sql
\i sql/pgsql/testintegers.sql
\i sql/java/testintegers.sql
\i sql/perl/testintegers.sql
\i sql/lua/testintegers.sql

SELECT
    'maxSmallInt',
    plbench('SELECT maxSmallInt()', 200) as pldotnet,
    plbench('SELECT maxSmallIntV8()', 200) as plv8,
    plbench('SELECT maxSmallIntPython()', 200) as plpython,
    plbench('SELECT maxSmallIntPg()', 200) as plpgsql,
    plbench('SELECT maxSmallIntJava()', 200) as pljava,
    plbench('SELECT maxSmallIntPerl()', 200) as plperl,
    plbench('SELECT maxSmallIntLua()', 200) as pllua;

SELECT
    'sum2SmallInt',
    plbench('SELECT sum2SmallInt(CAST(100 AS smallint), CAST(101 AS smallint))', 200) as pldotnet,
    plbench('SELECT sum2SmallIntV8(CAST(100 AS smallint), CAST(101 AS smallint))', 200) as plv8,
    plbench('SELECT sum2SmallIntPython(CAST(100 AS smallint), CAST(101 AS smallint))', 200) as plpython,
    plbench('SELECT sum2SmallIntPg(CAST(100 AS smallint), CAST(101 AS smallint))', 200) as plpgsql,
    plbench('SELECT sum2SmallIntJava(CAST(100 AS smallint), CAST(101 AS smallint))', 200) as pljava,
    plbench('SELECT sum2SmallIntPerl(CAST(100 AS smallint), CAST(101 AS smallint))', 200) as plperl,
    plbench('SELECT sum2SmallIntLua(CAST(100 AS smallint), CAST(101 AS smallint))', 200) as pllua;

SELECT
    'maxInteger',
    plbench('SELECT maxInteger()', 200) as pldotnet,
    plbench('SELECT maxIntegerV8()', 200) as plv8,
    plbench('SELECT maxIntegerPython()', 200) as plpython,
    plbench('SELECT maxIntegerPg()', 200) as plpgsql,
    plbench('SELECT maxIntegerJava()', 200) as pljava,
    plbench('SELECT maxIntegerPerl()', 200) as plperl,
    plbench('SELECT maxIntegerLua()', 200) as pllua;

SELECT
    'sum2Integer',
    plbench('SELECT sum2Integer(32770, 100)', 200) as pldotnet,
    plbench('SELECT sum2IntegerV8(32770, 100)', 200) as plv8,
    plbench('SELECT sum2IntegerPython(32770, 100)', 200) as plpython,
    plbench('SELECT sum2IntegerPg(32770, 100)', 200) as plpgsql,
    plbench('SELECT sum2IntegerJava(32770, 100)', 200) as pljava,
    plbench('SELECT sum2IntegerPerl(32770, 100)', 200) as plperl,
    plbench('SELECT sum2IntegerLua(32770, 100)', 200) as pllua;

SELECT
    'maxBigInt',
    plbench('SELECT maxBigInt()', 200) as pldotnet,
    plbench('SELECT maxBigIntV8()', 200) as plv8,
    plbench('SELECT maxBigIntPython()', 200) as plpython,
    plbench('SELECT maxBigIntPg()', 200) as plpgsql,
    plbench('SELECT maxBigIntJava()', 200) as pljava,
    plbench('SELECT maxBigIntPerl()', 200) as plperl,
    plbench('SELECT maxBigIntLua()', 200) as pllua;

SELECT
    'sum2BigInt',
    plbench('SELECT sum2BigInt(9223372036854775707, 100)', 200) as pldotnet,
    plbench('SELECT sum2BigIntV8(9223372036854775707, 100)', 200) as plv8,
    plbench('SELECT sum2BigIntPython(9223372036854775707, 100)', 200) as plpython,
    plbench('SELECT sum2BigIntPg(9223372036854775707, 100)', 200) as plpgsql,
    plbench('SELECT sum2BigIntJava(9223372036854775707, 100)', 200) as pljava,
    plbench('SELECT sum2BigIntPerl(9223372036854775707, 100)', 200) as plperl,
    plbench('SELECT sum2BigIntLua(9223372036854775707, 100)', 200) as pllua;

SELECT
    'mixedBigInt',
    plbench('SELECT mixedBigInt(32767,  2147483647, 100)', 200) as pldotnet,
    plbench('SELECT mixedBigIntV8(32767,  CAST(2147483647 as bigint), CAST(100 as bigint))', 200) as plv8,
    plbench('SELECT mixedBigIntPython(32767,  2147483647, 100)', 200) as plpython,
    plbench('SELECT mixedBigIntPg(32767,  CAST(2147483647 as bigint), CAST(100 as bigint))', 200) as plpgsql,
    plbench('SELECT mixedBigIntJava(32767,  2147483647, 100)', 200) as pljava,
    plbench('SELECT mixedBigIntPerl(32767,  2147483647, 100)', 200) as plperl,
    plbench('SELECT mixedBigIntLua(32767,  2147483647, 100)', 200) as pllua;

SELECT
    'mixedInt',
    plbench('SELECT mixedInt(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', 100) as pldotnet,
    plbench('SELECT mixedIntV8(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', 100) as plv8,
    plbench('SELECT mixedIntPython(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', 100) as ppython,
    plbench('SELECT mixedIntPg(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', 100) as plpgsql,
    plbench('SELECT mixedIntJava(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', 100) as pljava,
    plbench('SELECT mixedIntPerl(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', 100) as plperl,
    plbench('SELECT mixedIntLua(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', 100) as pllua;
