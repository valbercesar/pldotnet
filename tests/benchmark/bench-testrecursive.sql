\i tests/benchmark/includes.sql

\i tests/benchmark/csharp/testrecursive.sql
-- \i tests/benchmark/fsharp/testfsrecursive.sql
\i tests/benchmark/v8javascript/testrecursive.sql
\i tests/benchmark/python/testrecursive.sql
\i tests/benchmark/pgsql/testrecursive.sql
\i tests/benchmark/java/testrecursive.sql

\set runs 1000

SELECT
    'fibbb',
    plbench('SELECT fibbb(25)', 10) as plcsharp,
    -- plbench('SELECT fibbbFSharp(25)', 10) as plfsharp;
    plbench('SELECT fibbbV8(25)', 10) as plv8,
    plbench('SELECT fibbbPython(25)', 10) as plpython,
    plbench('SELECT fibbbPg(25)', 10) as plpgsql,
    plbench('SELECT fibbbJava(25)', 10) as pljava,
    '-' as plperl,
    '-' as pllua,
    '-' as pltcl,
    '-' as plr;

SELECT
    'fact',
    plbench('SELECT fact(12)', :runs) as plcsharp,
    -- plbench('SELECT factFSharp(12)', :runs) as plfsharp;
    plbench('SELECT factV8(12)', :runs) as plv8,
    plbench('SELECT factPython(12)', :runs) as plpython,
    plbench('SELECT factPg(12)', :runs) as plpgsql,
    plbench('SELECT factJava(12)', :runs) as pljava,
    '-' as plperl,
    '-' as pllua,
    '-' as pltcl,
    '-' as plr;

-- PL.NET doens't have support for numeric data type.
-- SELECT
--     'natural(10)',
--     plbench('SELECT natural(10)', :runs) as plcsharp,
--     -- plbench('SELECT naturalFSharp(10)', :runs) as plfsharp;
--     -- plbench('SELECT naturalV8(10)', :runs) as plv8,
--     plbench('SELECT naturalPython(10)', :runs) as plpython,
--     plbench('SELECT naturalPg(10)', :runs) as plpgsql,
--     plbench('SELECT naturalJava(10)', :runs) as pljava,
--     '-' as plperl,
--     '-' as pllua,
--     '-' as pltcl,
--     '-' as plr;

-- SELECT
--     'natural(10.5)',
--     plbench('SELECT natural(10.5)', :runs) as plcsharp,
--     -- plbench('SELECT naturalFSharp(10.5)', :runs) as plfsharp;
--     -- plbench('SELECT naturalV8(10.5)', :runs) as plv8,
--     plbench('SELECT naturalPython(10.5)', :runs) as plpython,
--     plbench('SELECT naturalPg(10.5)', :runs) as plpgsql,
--     plbench('SELECT naturalJava(10.5)', :runs) as pljava,
--     '-' as plperl,
--     '-' as pllua,
--     '-' as pltcl,
--     '-' as plr;
