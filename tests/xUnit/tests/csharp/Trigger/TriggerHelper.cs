using System;

namespace PlDotNET.Tests.Helper
{
    public static class SqlHelperScript
    {
        public static string CommonTriggerTableSetup => @"
DROP TABLE IF EXISTS trigger_test_table;
DROP TRIGGER IF EXISTS test_trigger_BIR_1 ON trigger_test_table;
DROP FUNCTION IF EXISTS trigger_test_function_modify();

CREATE TABLE trigger_test_table(
    id      INT,
    textcol TEXT
);
";
    }
}