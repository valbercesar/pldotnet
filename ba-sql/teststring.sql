-- TEXT

CREATE OR REPLACE FUNCTION identityStr(a text) RETURNS text AS $$
    System.Console.WriteLine("Got string: {0}", a);
    return a;
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TEXT', 'identityStr', identityStr('dog') = 'dog';

CREATE OR REPLACE FUNCTION concatenateText(a text, b text) RETURNS text AS $$
    String c = a + " " + b;
    return c;
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TEXT', 'concatenateText', concatenateText('red', 'blue') = 'red blue';

CREATE OR REPLACE FUNCTION multiplyText(a text, b int) RETURNS text AS $$
    int i;
    String c = "";
    for(i=0;i<b;i++){ c = c + a; }
    return c;
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TEXT', 'multiplyText', multiplyText('dog ', 3) = 'dog dog dog ';

-- CHAR

CREATE OR REPLACE FUNCTION addGoodbye(a BPCHAR) RETURNS BPCHAR AS $$
    return (a + " Goodbye ^.^");
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'CHAR', 'testingBpChar', testingBpChar('HELLO!') = 'HELLO! Goodbye ^.^'::BPCHAR;

CREATE OR REPLACE FUNCTION concatenateChars(a BPCHAR, b BPCHAR, c BPCHAR) RETURNS BPCHAR AS $$
    return (a + " " + b + " " + c).ToUpper();
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'CHAR', 'concatenateChars', concatenateChars('hello'::BPCHAR, 'beautiful'::BPCHAR, 'world!'::BPCHAR) = 'HELLO BEAUTIFUL WORLD!'::BPCHAR;

-- VARCHAR

CREATE OR REPLACE FUNCTION concatenateVarChars(a VARCHAR, b VARCHAR, c BPCHAR) RETURNS VARCHAR AS $$
    return (a + " " + b + " " + c).ToUpper();
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'VARCHAR', 'concatenateVarChars', concatenateVarChars('hello'::VARCHAR, 'beautiful'::VARCHAR, 'world!'::BPCHAR) = 'HELLO BEAUTIFUL WORLD!'::VARCHAR;

CREATE OR REPLACE FUNCTION multiplyVarChar(a VARCHAR, b int) RETURNS VARCHAR AS $$
    string c = "";
    for(int i=0;i<b;i++){ c = c + a; }
    return c.ToUpper();
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'VARCHAR', 'multiplyVarChar', multiplyVarChar('hello '::VARCHAR, 5) = 'HELLO HELLO HELLO HELLO HELLO '::VARCHAR;

-- XML

CREATE OR REPLACE FUNCTION modifyXml(a XML) RETURNS XML AS $$
    string new_xml = a.Replace("Hello", "Goodbye");
    new_xml = new_xml.Replace("World", "beautiful World");
    return new_xml;
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'XML', 'modifyXml', modifyXml('<?xml version="1.0" encoding="utf-8"?><title>Hello, World!</title>'::XML)::text = '<?xml version="1.0" encoding="utf-8"?><title>Goodbye, beautiful World!</title>'::XML::text;

CREATE OR REPLACE FUNCTION createXml(title TEXT, p1 TEXT, p2 TEXT) RETURNS XML AS $$
    string c = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
    c += $"<title>{title.ToUpper()}</title>";
    c += "<body>";
    c += $"<p>{p1}</p>";
    c += $"<p>{p2}</p>";
    c += "</body>";
    return c;
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'XML', 'createXml', createXml('hello world'::TEXT, 'First paragraph'::TEXT, 'Second paragraph'::TEXT)::TEXT = '<?xml version="1.0" encoding="utf-8"?><title>HELLO WORLD</title><body><p>First paragraph</p><p>Second paragraph</p></body>'::XML::TEXT;