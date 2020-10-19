\i sql/benchmark/includes.sql

\i sql/testbool.sql
\i sql/v8javascript/testbool.sql

SELECT
    'returnBool',
    plbench('SELECT returnBool()', 1000) as pldotnet,
    plbench('SELECT returnBoolV8()', 1000) as plv8;

SELECT 
    'BooleanAnd',
    plbench('SELECT BooleanAnd(true, true)', 1000) as pldotnet,
    plbench('SELECT BooleanAndV8(true, true)', 1000) as plv8;

SELECT 
    'BooleanOr',
    plbench('SELECT BooleanOr(false, false)', 1000) as pldotnet,
    plbench('SELECT BooleanOrV8(false, false)', 1000) as plv8;

SELECT 
    'BooleanXor',
    plbench('SELECT BooleanXor(false, false)', 100) as pldotnet,
    plbench('SELECT BooleanXorV8(false, false)', 100) as plv8;
