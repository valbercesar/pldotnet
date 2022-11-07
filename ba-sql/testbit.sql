-- BIT

CREATE OR REPLACE FUNCTION modifybit(a BIT(10)) RETURNS BIT(10) AS $$
    a[0] = a[0] ? false : true;
    a[a.Length-1] = a[a.Length-1] ? false : true;
    return a;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BIT', 'modifybit', modifybit('10101'::BIT(10)) = '0010100001'::BIT(10);

-- VARBIT

CREATE OR REPLACE FUNCTION modifyvarbit(a BIT VARYING) RETURNS BIT VARYING AS $$
    a[0] = a[0] ? false : true;
    a[a.Length-1] = a[a.Length-1] ? false : true;
    return a;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'VARBIT', 'modifyvarbit', modifyvarbit('1001110001000'::BIT VARYING) = '0001110001001'::BIT VARYING;

CREATE OR REPLACE FUNCTION concatenatevarbit(a BIT VARYING, b BIT VARYING) RETURNS BIT VARYING AS $$
    BitArray c = new BitArray(a.Length+b.Length);
    for(int i = 0; i < a.Length;i++)
        c[i] = a[i];
    for(int i = 0, cont = a.Length; i < b.Length;i++)
        c[cont++] = b[i];
    return c;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'VARBIT', 'concatenatevarbit1', concatenatevarbit('1001110001000'::BIT VARYING, '111010111101111000'::BIT VARYING) = '1001110001000111010111101111000'::BIT VARYING;
SELECT 'VARBIT', 'concatenatevarbit2', concatenatevarbit('1001110001000'::BIT(10), '111010111101111000'::BIT VARYING) = '1001110001111010111101111000'::BIT VARYING;
