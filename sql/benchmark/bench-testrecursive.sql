\i sql/benchmark/includes.sql

\i sql/testrecursive.sql
\i sql/v8javascript/testrecursive.sql
\i sql/python/testrecursive.sql
\i sql/pgsql/testrecursive.sql
\i sql/java/testrecursive.sql

SELECT
    'fibbb',
    plbench('SELECT fibbb(30)', 10) as pldotnet,
    plbench('SELECT fibbbV8(30)', 10) as plv8,
    plbench('SELECT fibbbPython(30)', 10) as plpython,
    plbench('SELECT fibbbPg(30)', 10) as plpgsql,
    plbench('SELECT fibbbJava(30)', 10) as pljava,
    '-' as plperl,
    '-' as pllua,
    '-' as pltcl;

SELECT
    'fact',
    plbench('SELECT fact(5)', 10) as pldotnet,
    plbench('SELECT factV8(5)', 10) as plv8,
    plbench('SELECT factPython(5)', 10) as plpython,
    plbench('SELECT factPg(5)', 10) as plpgsql,
    plbench('SELECT factJava(5)', 10) as pljava,
    '-' as plperl,
    '-' as pllua,
    '-' as pltcl;

SELECT
    'natural(10)',
    plbench('SELECT natural(10)', 10) as pldotnet,
    plbench('SELECT naturalV8(10)', 10) as plv8,
    plbench('SELECT naturalPython(10)', 10) as plpython,
    plbench('SELECT naturalPg(10)', 10) as plpgsql,
    plbench('SELECT naturalJava(10)', 10) as pljava,
    '-' as plperl,
    '-' as pllua,
    '-' as pltcl;

SELECT
    'natural(10.5)',
    plbench('SELECT natural(10.5)', 10) as pldotnet,
    plbench('SELECT naturalV8(10.5)', 10) as plv8,
    plbench('SELECT naturalPython(10.5)', 10) as plpython,
    plbench('SELECT naturalPg(10.5)', 10) as plpgsql,
    plbench('SELECT naturalJava(10.5)', 10) as pljava,
    '-' as plperl,
    '-' as pllua,
    '-' as pltcl;
