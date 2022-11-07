-- BYTEA

CREATE OR REPLACE FUNCTION byteaConversions(a BYTEA, b BYTEA) RETURNS BYTEA AS $$
    UTF8Encoding utf8_e = new UTF8Encoding();
    string s1 = utf8_e.GetString(a, 0, a.Length);
    string s2 = utf8_e.GetString(b, 0, b.Length);
    string result = s1 + " " + s2;
    return utf8_e.GetBytes(result);
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BYTEA', 'byteaConversions', byteaConversions('Brick Abode is nice!'::BYTEA, 'Thank you very much...'::BYTEA) = '\x427269636b2041626f6465206973206e69636521205468616e6b20796f752076657279206d7563682e2e2e'::BYTEA;

CREATE OR REPLACE FUNCTION concatenateBytea(a BYTEA, b TEXT) RETURNS BYTEA AS $$
    UTF8Encoding utf8_e = new UTF8Encoding();
    byte[] b_bytes = new byte[b.Length];
    utf8_e.GetBytes(b, 0, b.Length, b_bytes, 0);
    byte[] c = new byte[a.Length + b_bytes.Length];
    a.CopyTo(c, 0);
    b_bytes.CopyTo(c, a.Length);
    return c;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BYTEA', 'concatenateBytea', concatenateBytea('\x427269636b2041626f6465206973206e69636521'::BYTEA, ' Thank you very much...'::TEXT) = '\x427269636b2041626f6465206973206e69636521205468616e6b20796f752076657279206d7563682e2e2e'::BYTEA;