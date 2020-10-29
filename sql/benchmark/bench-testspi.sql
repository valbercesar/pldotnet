\i sql/benchmark/includes.sql

\i sql/testspi.sql
\i sql/v8javascript/testspi.sql
\i sql/python/testspi.sql
\i sql/pgsql/testspi.sql
\i sql/java/testspi.sql
\i sql/perl/testspi.sql
\i sql/tcl/testspi.sql

SELECT
    'returnCompositeSum',
    plbench('SELECT returnCompositeSum()', 100) as pldotnet,
    plbench('SELECT returnCompositeSumV8()', 100) as plv8,
    plbench('SELECT returnCompositeSumPython()', 100) as plpython,
    plbench('SELECT returnCompositeSumPg()', 100) as plpgsql,
    plbench('SELECT returnCompositeSumJava()', 100) as pljava,
    plbench('SELECT returnCompositeSumPerl()', 100) as plperl,
    '-' as pllua,
    plbench('SELECT returnCompositeSumTcl()', 100) as pltcl;

SELECT
    'checkTypes',
    plbench('SELECT checkTypes()', 100) as pldotnet,
    plbench('SELECT checkTypesV8()', 100) as plv8,
    plbench('SELECT checkTypesPython()', 100) as plpython,
    plbench('SELECT checkTypesPg()', 100) as plpgsql,
    plbench('SELECT checkTypesJava()', 100) as pljava,
    '-' as plperl,
    '-' as pllua,
    plbench('SELECT checkTypesTcl()', 100) as pltcl;

SELECT
    'getUsersWithBalance',
    plbench('SELECT getUsersWithBalance(2304.55)', 100) as pldotnet,
    plbench('SELECT getUsersWithBalanceV8(2304.55)', 100) as plv8,
    plbench('SELECT getUsersWithBalancePython(2304.55)', 100) as plpython,
    plbench('SELECT getUsersWithBalancePg(2304.55)', 100) as plpgsql,
    plbench('SELECT getUsersWithBalanceJava(2304.55)', 100) as pljava,
    plbench('SELECT getUsersWithBalancePerl(2304.55)', 100) as plperl,
    '-' as pllua,
    plbench('SELECT getUsersWithBalanceTcl(2304.55)', 100) as pltcl;

SELECT
    'getUserDescription(123456789)',
    plbench('SELECT getUserDescription(123456789)', 100) as pldotnet,
    plbench('SELECT getUserDescriptionV8(123456789)', 100) as plv8,
    plbench('SELECT getUserDescriptionPython(123456789)', 100) as plpython,
    plbench('SELECT getUserDescriptionPg(123456789)', 100) as plpgsql,
    plbench('SELECT getUserDescriptionJava(123456789)', 100) as pljava,
    plbench('SELECT getUserDescriptionPerl(123456789)', 100) as plperl,
    '-' as pllua,
    plbench('SELECT getUserDescriptionTcl(123456789)', 100) as pltcl;

SELECT
    'getUserDescriptionV8(987654321)',
    plbench('SELECT getUserDescription(987654321)', 100) as pldotnet,
    plbench('SELECT getUserDescriptionV8(987654321)', 100) as plv8,
    plbench('SELECT getUserDescriptionPython(987654321)', 100) as plpython,
    plbench('SELECT getUserDescriptionPg(987654321)', 100) as plpgsql,
    plbench('SELECT getUserDescriptionJava(987654321)', 100) as pljava,
    plbench('SELECT getUserDescriptionPerl(987654321)', 100) as plperl,
    '-' as pllua,
    plbench('SELECT getUserDescriptionTcl(987654321)', 100) as pltcl;
