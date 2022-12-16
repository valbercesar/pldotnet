\i sql/benchmark/includes.sql

\i sql/testintegers.sql
-- \i sql/v8javascript/testintegers.sql
\i sql/python/testintegers.sql
\i sql/pgsql/testintegers.sql
\i sql/java/testintegers.sql
\i sql/perl/testintegers.sql
\i sql/lua/testintegers.sql
\i sql/tcl/testintegers.sql
\i sql/r/testintegers.sql
-- \i sql/testfsintegers.sql

\set runs 1000

SELECT
    'maxSmallInt',
    plbench('SELECT maxSmallInt()', :runs) as plcsharp,
    -- plbench('SELECT maxSmallIntFSharp()', :runs) as plfsharp;
    -- plbench('SELECT maxSmallIntV8()', :runs) as plv8,
    plbench('SELECT maxSmallIntPython()', :runs) as plpython,
    plbench('SELECT maxSmallIntPg()', :runs) as plpgsql,
    plbench('SELECT maxSmallIntJava()', :runs) as pljava,
    plbench('SELECT maxSmallIntPerl()', :runs) as plperl,
    plbench('SELECT maxSmallIntLua()', :runs) as pllua,
    plbench('SELECT maxSmallIntTcl()', :runs) as pltcl,
    plbench('SELECT maxSmallIntR()', :runs) as plr;

SELECT
    'sum2SmallInt',
    plbench('SELECT sum2SmallInt(CAST(100 AS smallint), CAST(101 AS smallint))', :runs) as plcsharp,
    -- plbench('SELECT sum2SmallIntFSharp(CAST(100 AS smallint), CAST(101 AS smallint))', :runs) as plfsharp;
    -- plbench('SELECT sum2SmallIntV8(CAST(100 AS smallint), CAST(101 AS smallint))', :runs) as plv8,
    plbench('SELECT sum2SmallIntPython(CAST(100 AS smallint), CAST(101 AS smallint))', :runs) as plpython,
    plbench('SELECT sum2SmallIntPg(CAST(100 AS smallint), CAST(101 AS smallint))', :runs) as plpgsql,
    plbench('SELECT sum2SmallIntJava(CAST(100 AS smallint), CAST(101 AS smallint))', :runs) as pljava,
    plbench('SELECT sum2SmallIntPerl(CAST(100 AS smallint), CAST(101 AS smallint))', :runs) as plperl,
    plbench('SELECT sum2SmallIntLua(CAST(100 AS smallint), CAST(101 AS smallint))', :runs) as pllua,
    plbench('SELECT sum2SmallIntTcl(CAST(100 AS smallint), CAST(101 AS smallint))', :runs) as pltcl,
    plbench('SELECT sum2SmallIntR(CAST(100 AS smallint), CAST(101 AS smallint))', :runs) as plr;

SELECT
    'maxInteger',
    plbench('SELECT maxInteger()', :runs) as plcsharp,
    -- plbench('SELECT maxIntegerFSharp()', :runs) as plfsharp;
    -- plbench('SELECT maxIntegerV8()', :runs) as plv8,
    plbench('SELECT maxIntegerPython()', :runs) as plpython,
    plbench('SELECT maxIntegerPg()', :runs) as plpgsql,
    plbench('SELECT maxIntegerJava()', :runs) as pljava,
    plbench('SELECT maxIntegerPerl()', :runs) as plperl,
    plbench('SELECT maxIntegerLua()', :runs) as pllua,
    plbench('SELECT maxIntegerTcl()', :runs) as pltcl,
    plbench('SELECT maxIntegerR()', :runs) as plr;

SELECT
    'sum2Integer',
    plbench('SELECT sum2Integer(32770, 100)', :runs) as plcsharp,
    -- plbench('SELECT sum2IntegerFSharp(32770, 100)', :runs) as plfsharp;
    -- plbench('SELECT sum2IntegerV8(32770, 100)', :runs) as plv8,
    plbench('SELECT sum2IntegerPython(32770, 100)', :runs) as plpython,
    plbench('SELECT sum2IntegerPg(32770, 100)', :runs) as plpgsql,
    plbench('SELECT sum2IntegerJava(32770, 100)', :runs) as pljava,
    plbench('SELECT sum2IntegerPerl(32770, 100)', :runs) as plperl,
    plbench('SELECT sum2IntegerLua(32770, 100)', :runs) as pllua,
    plbench('SELECT sum2IntegerTcl(32770, 100)', :runs) as pltcl,
    plbench('SELECT sum2IntegerR(32770, 100)', :runs) as plr;

SELECT
    'maxBigInt',
    plbench('SELECT maxBigInt()', :runs) as plcsharp,
    -- plbench('SELECT maxBigIntFSharp()', :runs) as plfsharp;
    -- plbench('SELECT maxBigIntV8()', :runs) as plv8,
    plbench('SELECT maxBigIntPython()', :runs) as plpython,
    plbench('SELECT maxBigIntPg()', :runs) as plpgsql,
    plbench('SELECT maxBigIntJava()', :runs) as pljava,
    plbench('SELECT maxBigIntPerl()', :runs) as plperl,
    plbench('SELECT maxBigIntLua()', :runs) as pllua,
    plbench('SELECT maxBigIntTcl()', :runs) as pltcl,
    plbench('SELECT maxBigIntR()', :runs) as plr;

SELECT
    'sum2BigInt',
    plbench('SELECT sum2BigInt(9223372036854775707, 100)', :runs) as plcsharp,
    -- plbench('SELECT sum2BigIntFSharp(9223372036854775707, 100)', :runs) as plfsharp;
    -- plbench('SELECT sum2BigIntV8(9223372036854775707, 100)', :runs) as plv8,
    plbench('SELECT sum2BigIntPython(9223372036854775707, 100)', :runs) as plpython,
    plbench('SELECT sum2BigIntPg(9223372036854775707, 100)', :runs) as plpgsql,
    plbench('SELECT sum2BigIntJava(9223372036854775707, 100)', :runs) as pljava,
    plbench('SELECT sum2BigIntPerl(9223372036854775707, 100)', :runs) as plperl,
    plbench('SELECT sum2BigIntLua(9223372036854775707, 100)', :runs) as pllua,
    plbench('SELECT sum2BigIntTcl(9223372036854775707, 100)', :runs) as pltcl,
    '-' as plr;

SELECT
    'mixedBigInt',
    plbench('SELECT mixedBigInt(32767,  2147483647, 100)', :runs) as plcsharp,
    -- plbench('SELECT mixedBigIntFSharp(32767,  2147483647, 100)', :runs) as plfsharp;
    -- plbench('SELECT mixedBigIntV8(32767,  CAST(2147483647 as bigint), CAST(100 as bigint))', :runs) as plv8,
    plbench('SELECT mixedBigIntPython(32767,  2147483647, 100)', :runs) as plpython,
    plbench('SELECT mixedBigIntPg(32767,  CAST(2147483647 as bigint), CAST(100 as bigint))', :runs) as plpgsql,
    plbench('SELECT mixedBigIntJava(32767,  2147483647, 100)', :runs) as pljava,
    plbench('SELECT mixedBigIntPerl(32767,  2147483647, 100)', :runs) as plperl,
    plbench('SELECT mixedBigIntLua(32767,  2147483647, 100)', :runs) as pllua,
    plbench('SELECT mixedBigIntTcl(32767,  2147483647, 100)', :runs) as pltcl,
    '-' as plr;

SELECT
    'mixedInt',
    plbench('SELECT mixedInt(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', :runs) as plcsharp,
    -- plbench('SELECT mixedIntFSharp(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', :runs) as plfsharp;
    -- plbench('SELECT mixedIntV8(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', :runs) as plv8,
    plbench('SELECT mixedIntPython(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', :runs) as ppython,
    plbench('SELECT mixedIntPg(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', :runs) as plpgsql,
    plbench('SELECT mixedIntJava(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', :runs) as pljava,
    plbench('SELECT mixedIntPerl(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', :runs) as plperl,
    plbench('SELECT mixedIntLua(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', :runs) as pllua,
    plbench('SELECT mixedIntTcl(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', :runs) as pltcl,
    plbench('SELECT mixedIntR(CAST(32767 AS smallint),  CAST(32767 AS smallint), 100)', :runs) as plr;
