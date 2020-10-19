\i sql/benchmark/includes.sql

\i sql/testspi.sql
\i sql/v8javascript/testspi.sql

SELECT
    'returnCompositeSum',
    plbench('SELECT returnCompositeSum()', 100) as pldotnet,
    plbench('SELECT returnCompositeSumV8()', 100) as plv8;

SELECT
    'checkTypes',
    plbench('SELECT checkTypes()', 100) as pldotnet,
    plbench('SELECT checkTypesV8()', 100) as plv8;

SELECT
    'getUsersWithBalance',
    plbench('SELECT getUsersWithBalance(2304.55)', 100) as pldotnet,
    plbench('SELECT getUsersWithBalanceV8(2304.55)', 100) as plv8;

SELECT
    'getUserDescription(123456789)',
    plbench('SELECT getUserDescription(123456789)', 100) as pldotnet,
    plbench('SELECT getUserDescriptionV8(123456789)', 100) as plv8;

SELECT
    'getUserDescriptionV8(987654321)',
    plbench('SELECT getUserDescription(987654321)', 100) as pldotnet,
    plbench('SELECT getUserDescriptionV8(987654321)', 100) as plv8;
