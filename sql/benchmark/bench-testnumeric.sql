\i sql/benchmark/includes.sql

\i sql/testnumeric.sql
\i sql/v8javascript/testnumeric.sql
\i sql/python/testnumeric.sql
\i sql/pgsql/testnumeric.sql
\i sql/java/testnumeric.sql
\i sql/perl/testnumeric.sql
\i sql/lua/testnumeric.sql
\i sql/tcl/testnumeric.sql
\i sql/r/testnumeric.sql

SELECT
    'get_sum(1.3333333, 10)',
    plbench('SELECT get_sum(1.3333333, 10)', 500) as pldotnet,
    plbench('SELECT get_sumV8(1.3333333, 10)', 500) as plv8,
    plbench('SELECT get_sumPython(1.3333333, 10)', 500) as plpython,
    plbench('SELECT get_sumPg(1.3333333, 10)', 500) as plpgsql,
    plbench('SELECT get_sumJava(1.3333333, 10)', 500) as pljava,
    plbench('SELECT get_sumPerl(1.3333333, 10)', 500) as plperl,
    '-' as pllua,
    plbench('SELECT get_sumTcl(1.3333333, 10)', 500) as pltcl,
    plbench('SELECT get_sumR(1.3333333, 10)', 500) as plr;

SELECT
    'get_sum(1.33333333, -10.99999999)',
    plbench('SELECT get_sum(1.33333333, -10.99999999)', 500) as pldotnet,
    plbench('SELECT get_sumV8(1.33333333, -10.99999999)', 500) as plv8,
    plbench('SELECT get_sumPython(1.33333333, -10.99999999)', 500) as plpython,
    plbench('SELECT get_sumPg(1.33333333, -10.99999999)', 500) as plpgsql,
    plbench('SELECT get_sumJava(1.33333333, -10.99999999)', 500) as pljava,
    plbench('SELECT get_sumPerl(1.33333333, -10.99999999)', 500) as plperl,
    '-' as pllua,
    plbench('SELECT get_sumTcl(1.33333333, -10.99999999)', 500) as pltcl,
    plbench('SELECT get_sumR(1.33333333, -10.99999999)', 500) as plr;

SELECT
    'get_sum(1999999999999.555555555555555, -10.99999999)',
    plbench('SELECT get_sum(1999999999999.555555555555555, -10.99999999)', 500) as pldotnet,
    plbench('SELECT get_sumV8(1999999999999.555555555555555, -10.99999999)', 500) as plv8,
    plbench('SELECT get_sumPython(1999999999999.555555555555555, -10.99999999)', 500) as plpython,
    plbench('SELECT get_sumPg(1999999999999.555555555555555, -10.99999999)', 500) as plpgsql,
    plbench('SELECT get_sumJava(1999999999999.555555555555555, -10.99999999)', 500) as pljava,
    plbench('SELECT get_sumPerl(1999999999999.555555555555555, -10.99999999)', 500) as plperl,
    '-' as pllua,
    plbench('SELECT get_sumTcl(1999999999999.555555555555555, -10.99999999)', 500) as pltcl,
    plbench('SELECT get_sumR(1999999999999.555555555555555, -10.99999999)', 500) as plr;

SELECT
    'getbigNum(999999999999999999991.9999991)',
    plbench('SELECT getbigNum(999999999999999999991.9999991)', 500) as pldotnet,
    plbench('SELECT getbigNumV8(999999999999999999991.9999991)', 500) as plv8,
    plbench('SELECT getbigNumPython(999999999999999999991.9999991)', 500) as plpython,
    plbench('SELECT getbigNumPg(999999999999999999991.9999991)', 500) as plpgsql,
    plbench('SELECT getbigNumJava(999999999999999999991.9999991)', 500) as pljava,
    plbench('SELECT getbigNumPerl(999999999999999999991.9999991)', 500) as plperl,
    plbench('SELECT getbigNumLua(999999999999999999991.9999991)', 500) as pllua,
    plbench('SELECT getbigNumTcl(999999999999999999991.9999991)', 500) as pltcl,
    plbench('SELECT getbigNumR(999999999999999999991.9999991)', 500) as plr;

SELECT
    'getbigNum(999999999999999999991.99999999)',
    plbench('SELECT getbigNum(999999999999999999991.99999999)', 500) as pldotnet,
    plbench('SELECT getbigNumV8(999999999999999999991.99999999)', 500) as plv8,
    plbench('SELECT getbigNumPython(999999999999999999991.99999999)', 500) as plpython,
    plbench('SELECT getbigNumPg(999999999999999999991.99999999)', 500) as plpgsql,
    plbench('SELECT getbigNumJava(999999999999999999991.99999999)', 500) as pljava,
    plbench('SELECT getbigNumPerl(999999999999999999991.99999999)', 500) as plperl,
    plbench('SELECT getbigNumLua(999999999999999999991.99999999)', 500) as pllua,
    plbench('SELECT getbigNumTcl(999999999999999999991.99999999)', 500) as pltcl,
    plbench('SELECT getbigNumR(999999999999999999991.99999999)', 500) as plr;
