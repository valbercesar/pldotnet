using System;

namespace PlDotNET.Tests.Helper
{
    public static class SqlHelperScript
    {
        public static string CommonTriggerTableSetup => @"
DROP TABLE IF EXISTS trigger_test_table;

CREATE TABLE trigger_test_table(
    id      INT,
    message TEXT
);
";
    }
}