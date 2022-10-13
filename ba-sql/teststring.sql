CREATE OR REPLACE FUNCTION identityStr(a text) RETURNS text AS $$
    System.Console.WriteLine("Got string: {0}", a);
    return a;
$$ LANGUAGE plcsharp;
INSERT INTO results (testName, result)
SELECT 'TEST STRING: identityStr', identityStr('dog') = 'dog';

CREATE OR REPLACE FUNCTION concatenateText(a text, b text) RETURNS text AS $$
    String c = a + " " + b;
    return c;
$$ LANGUAGE plcsharp;
INSERT INTO results (testName, result)
SELECT 'TEST STRING: concatenateText', concatenateText('red', 'blue') = 'red blue';

CREATE OR REPLACE FUNCTION multiplyText(a text, b int) RETURNS text AS $$
    int i;
    String c = "";
    for(i=0;i<b;i++){ c = c + a; }
    return c;
$$ LANGUAGE plcsharp;
INSERT INTO results (testName, result)
SELECT 'TEST STRING: multiplyText', multiplyText('dog ', 3) = 'dog dog dog ';

