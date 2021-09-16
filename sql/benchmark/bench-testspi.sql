\i sql/benchmark/includes.sql

\i sql/testspi.sql
\i sql/testfsspi.sql
\i sql/v8javascript/testspi.sql
\i sql/python/testspi.sql
\i sql/pgsql/testspi.sql
\i sql/java/testspi.sql
\i sql/perl/testspi.sql
\i sql/tcl/testspi.sql
\i sql/r/testspi.sql

\set runs 100

SELECT
    'returnCompositeSum',
    plbench('SELECT returnCompositeSum()', :runs) as pldotnet,
    plbench('SELECT returnCompositeSumV8()', :runs) as plv8,
    plbench('SELECT returnCompositeSumPython()', :runs) as plpython,
    plbench('SELECT returnCompositeSumPg()', :runs) as plpgsql,
    plbench('SELECT returnCompositeSumJava()', :runs) as pljava,
    plbench('SELECT returnCompositeSumPerl()', :runs) as plperl,
    '-' as pllua,
    plbench('SELECT returnCompositeSumTcl()', :runs) as pltcl,
    plbench('SELECT returnCompositeSumR()', :runs) as plr,
    plbench('SELECT returnCompositeSumFSharp()', :runs) as plfsharp;

SELECT
    'checkTypes',
    plbench('SELECT checkTypes()', :runs) as pldotnet,
    plbench('SELECT checkTypesV8()', :runs) as plv8,
    plbench('SELECT checkTypesPython()', :runs) as plpython,
    plbench('SELECT checkTypesPg()', :runs) as plpgsql,
    plbench('SELECT checkTypesJava()', :runs) as pljava,
    '-' as plperl,
    '-' as pllua,
    plbench('SELECT checkTypesTcl()', :runs) as pltcl,
    plbench('SELECT checkTypesR()', :runs) as plr,
    plbench('SELECT checkTypesFSharp()', :runs) as plfsharp;

SELECT
    'getUsersWithBalance',
    plbench('SELECT getUsersWithBalance(2304.55)', :runs) as pldotnet,
    plbench('SELECT getUsersWithBalanceV8(2304.55)', :runs) as plv8,
    plbench('SELECT getUsersWithBalancePython(2304.55)', :runs) as plpython,
    plbench('SELECT getUsersWithBalancePg(2304.55)', :runs) as plpgsql,
    plbench('SELECT getUsersWithBalanceJava(2304.55)', :runs) as pljava,
    plbench('SELECT getUsersWithBalancePerl(2304.55)', :runs) as plperl,
    '-' as pllua,
    plbench('SELECT getUsersWithBalanceTcl(2304.55)', :runs) as pltcl,
    plbench('SELECT getUsersWithBalanceR(2304.55)', :runs) as plr,
    plbench('SELECT getUsersWithBalanceFSharp(2304.55)', :runs) as plfsharp;

SELECT
    'getUserDescription(123456789)',
    plbench('SELECT getUserDescription(123456789)', :runs) as pldotnet,
    plbench('SELECT getUserDescriptionV8(123456789)', :runs) as plv8,
    plbench('SELECT getUserDescriptionPython(123456789)', :runs) as plpython,
    plbench('SELECT getUserDescriptionPg(123456789)', :runs) as plpgsql,
    plbench('SELECT getUserDescriptionJava(123456789)', :runs) as pljava,
    plbench('SELECT getUserDescriptionPerl(123456789)', :runs) as plperl,
    '-' as pllua,
    plbench('SELECT getUserDescriptionTcl(123456789)', :runs) as pltcl,
    plbench('SELECT getUserDescriptionR(123456789)', :runs) as plr,
    plbench('SELECT getUserDescriptionFSharp(123456789)', :runs) as plfsharp;

SELECT
    'getUserDescriptionV8(987654321)',
    plbench('SELECT getUserDescription(987654321)', :runs) as pldotnet,
    plbench('SELECT getUserDescriptionV8(987654321)', :runs) as plv8,
    plbench('SELECT getUserDescriptionPython(987654321)', :runs) as plpython,
    plbench('SELECT getUserDescriptionPg(987654321)', :runs) as plpgsql,
    plbench('SELECT getUserDescriptionJava(987654321)', :runs) as pljava,
    plbench('SELECT getUserDescriptionPerl(987654321)', :runs) as plperl,
    '-' as pllua,
    plbench('SELECT getUserDescriptionTcl(987654321)', :runs) as pltcl,
    plbench('SELECT getUserDescriptionR(987654321)', :runs) as plr,
    plbench('SELECT getUserDescriptionFSharp(987654321)', :runs) as plfsharp;
