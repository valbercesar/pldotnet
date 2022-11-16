-- TEXT
CREATE OR REPLACE FUNCTION identityStr(a text) RETURNS text AS $$
    System.Console.WriteLine("Got string: {0}", a);
    return a;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TEXT', 'identityStr', identityStr('dog') = 'dog';

CREATE OR REPLACE FUNCTION concatenateText(a text, b text) RETURNS text AS $$
    String c = a + " " + b;
    return c;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TEXT', 'concatenateText', concatenateText('red', 'blue') = 'red blue';

CREATE OR REPLACE FUNCTION multiplyText(a text, b int) RETURNS text AS $$
    int i;
    String c = "";
    for(i=0;i<b;i++){ c = c + a; }
    return c;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TEXT', 'multiplyText', multiplyText('dog ', 3) = 'dog dog dog ';

-- CHAR
CREATE OR REPLACE FUNCTION addGoodbye(a BPCHAR) RETURNS BPCHAR AS $$
    return (a + " Goodbye ^.^");
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'CHAR', 'testingBpChar', addGoodbye('HELLO!') = 'HELLO! Goodbye ^.^'::BPCHAR;

CREATE OR REPLACE FUNCTION concatenateChars(a BPCHAR, b BPCHAR, c BPCHAR) RETURNS BPCHAR AS $$
    return (a + " " + b + " " + c).ToUpper();
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'CHAR', 'concatenateChars', concatenateChars('hello'::BPCHAR, 'beautiful'::BPCHAR, 'world!'::BPCHAR) = 'HELLO BEAUTIFUL WORLD!'::BPCHAR;

-- VARCHAR
CREATE OR REPLACE FUNCTION concatenateVarChars(a VARCHAR, b VARCHAR, c BPCHAR) RETURNS VARCHAR AS $$
    return (a + " " + b + " " + c).ToUpper();
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'VARCHAR', 'concatenateVarChars', concatenateVarChars('hello'::VARCHAR, 'beautiful'::VARCHAR, 'world!'::BPCHAR) = 'HELLO BEAUTIFUL WORLD!'::VARCHAR;

CREATE OR REPLACE FUNCTION multiplyVarChar(a VARCHAR, b int) RETURNS VARCHAR AS $$
    string c = "";
    for(int i=0;i<b;i++){ c = c + a; }
    return c.ToUpper();
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'VARCHAR', 'multiplyVarChar', multiplyVarChar('hello '::VARCHAR, 5) = 'HELLO HELLO HELLO HELLO HELLO '::VARCHAR;

-- XML
CREATE OR REPLACE FUNCTION modifyXml(a XML) RETURNS XML AS $$
    string new_xml = a.Replace("Hello", "Goodbye");
    new_xml = new_xml.Replace("World", "beautiful World");
    return new_xml;
$$ LANGUAGE plcsharp STRICT;
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
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'XML', 'createXml', createXml('hello world'::TEXT, 'First paragraph'::TEXT, 'Second paragraph'::TEXT)::TEXT = '<?xml version="1.0" encoding="utf-8"?><title>HELLO WORLD</title><body><p>First paragraph</p><p>Second paragraph</p></body>'::XML::TEXT;

--- Text Arrays
CREATE OR REPLACE FUNCTION returnTextArray(texts text[]) RETURNS text[] AS $$
return texts;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TEXT[]', 'returnTextArray1', returnTextArray(ARRAY['test1'::text, null::text, 'test string 2'::text, null::text]) = ARRAY['test1'::text, null::text, 'test string 2'::text, null::text];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TEXT[]', 'returnTextArray2', returnTextArray(ARRAY[[null::text, null::text], ['test1'::text, 'test string 2'::text]]) = ARRAY[[null::text, null::text], ['test1'::text, 'test string 2'::text]];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TEXT[]', 'returnTextArray3', returnTextArray(ARRAY[[[null::text, null::text], [null::text, null::text]], [['test1'::text, 'test 2'::text], ['test 3  abc'::text, 'test4'::text]]]) = ARRAY[[[null::text, null::text], [null::text, null::text]], [['test1'::text, 'test 2'::text], ['test 3  abc'::text, 'test4'::text]]];

CREATE OR REPLACE FUNCTION JoinTextArray(texts text[]) RETURNS text AS $$
Array flatten_texts = Array.CreateInstance(typeof(object), texts.Length);
ArrayHandler.FlatArray(texts, ref flatten_texts);
string result = "";
for(int i = 0; i < flatten_texts.Length; i++)
{   
    if (flatten_texts.GetValue(i) == null)
        continue;
    result = (string)(result + (string)flatten_texts.GetValue(i));
}
return result;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TEXT[]', 'JoinTextArray1', JoinTextArray(ARRAY['test1'::text, null::text, ' test string 2'::text, null::text]) = 'test1 test string 2';
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TEXT[]', 'JoinTextArray2', JoinTextArray(ARRAY[[['test1'::text, null::text, ' appended'::text], [' to'::text, ' another'::text, ' text:'::text]], [[' test string 2,'::text, null::text, ' is'::text], [' this'::text, ' text'::text, ' good?'::text]]]) = 'test1 appended to another text: test string 2, is this text good?';

CREATE OR REPLACE FUNCTION CreateTextMultidimensionalArray() RETURNS text[] AS $$
string?[, ,] text_three_dimensional = new string?[2, 2, 2] {{{"text 1", "text 2"}, {null, null}}, {{"text 3", null}, {"text 4", "text5"}}};
return text_three_dimensional;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TEXT[]', 'CreateTextMultidimensionalArray', CreateTextMultidimensionalArray() = ARRAY[[['text 1'::text, 'text 2'::text], [null::text, null::text]], [['text 3'::text, null::text], ['text 4'::text, 'text5'::text]]];

CREATE OR REPLACE FUNCTION updateArrayTextIndex(texts text[], desired text, index integer[]) RETURNS text[] AS $$
int[] arrayInteger = index.Cast<int>().ToArray();
texts.SetValue(desired, arrayInteger);
return texts;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TEXT[]', 'updateArrayTextIndex1', updateArrayTextIndex(ARRAY['test1'::text, null::text, ' test string 2'::text, null::text], 'test updated', ARRAY[1]) = ARRAY['test1'::text, 'test updated'::text, ' test string 2'::text, null::text];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'TEXT[]', 'updateArrayTextIndex2', updateArrayTextIndex(ARRAY[[['test1'::text, null::text, ' appended'::text], [' to'::text, ' another'::text, ' text:'::text]], [[' test string 2,'::text, null::text, ' is'::text], [' this'::text, ' text'::text, ' good?'::text]]], 'test updated', ARRAY[1, 0, 2]) = ARRAY[[['test1'::text, null::text, ' appended'::text], [' to'::text, ' another'::text, ' text:'::text]], [[' test string 2,'::text, null::text, 'test updated'::text], [' this'::text, ' text'::text, ' good?'::text]]];

--- BPCHAR Arrays
CREATE OR REPLACE FUNCTION updateCharArrayIndex(values_array BPCHAR[], desired BPCHAR, index integer[]) RETURNS BPCHAR[] AS $$
int[] arrayInteger = index.Cast<int>().ToArray();
values_array.SetValue(desired, arrayInteger);
return values_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BPCHAR[]', 'updateCharArrayIndex1', updateCharArrayIndex(ARRAY['hello'::BPCHAR, 'hi'::BPCHAR, null::BPCHAR, 'bye'::BPCHAR], 'goodbye'::BPCHAR, ARRAY[2]) = ARRAY['hello'::BPCHAR, 'hi'::BPCHAR, 'goodbye'::BPCHAR, 'bye'::BPCHAR];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BPCHAR[]', 'updateCharArrayIndex2', updateCharArrayIndex(ARRAY[[null::BPCHAR, null::BPCHAR], [null::BPCHAR, 'bye'::BPCHAR]], 'goodbye'::BPCHAR, ARRAY[1,0]) = ARRAY[[null::BPCHAR, null::BPCHAR], ['goodbye'::BPCHAR, 'bye'::BPCHAR]];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BPCHAR[]', 'updateCharArrayIndex3', updateCharArrayIndex(ARRAY['goodbye'::BPCHAR, 'bye'::BPCHAR, null::BPCHAR, 'bye'::BPCHAR], 'goodbye'::BPCHAR, ARRAY[2]) = ARRAY['goodbye'::BPCHAR, 'bye'::BPCHAR, 'goodbye'::BPCHAR, 'bye'::BPCHAR];

CREATE OR REPLACE FUNCTION AddSmileToChars(values_array BPCHAR[]) RETURNS BPCHAR[] AS $$
Array flatten_values = Array.CreateInstance(typeof(object), values_array.Length);
ArrayHandler.FlatArray(values_array, ref flatten_values);
for(int i = 0; i < flatten_values.Length; i++)
{   
    if (flatten_values.GetValue(i) == null)
        continue;

    string orig_value = (string)flatten_values.GetValue(i);
    string new_value = orig_value + " :)";
    
    flatten_values.SetValue((string)new_value, i);
}
return flatten_values;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BPCHAR[]', 'AddSmileToChars1', AddSmileToChars(ARRAY['hello'::BPCHAR, 'hi'::BPCHAR, null::BPCHAR, 'bye'::BPCHAR]) = ARRAY['hello :)'::BPCHAR, 'hi :)'::BPCHAR, null::BPCHAR, 'bye :)'::BPCHAR];

CREATE OR REPLACE FUNCTION CreateCharMultidimensionalArray() RETURNS BPCHAR[] AS $$
string objects_value = "Multiple dimensions";
string?[, ,] three_dimensional_array = new string?[2, 2, 2] {{{objects_value, objects_value}, {null, null}}, {{objects_value, null}, {objects_value, objects_value}}};
return three_dimensional_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BPCHAR[]', 'CreateCharMultidimensionalArray1', CreateCharMultidimensionalArray() = ARRAY[[['Multiple dimensions'::BPCHAR, 'Multiple dimensions'::BPCHAR], [null::BPCHAR, null::BPCHAR]], [['Multiple dimensions'::BPCHAR, null::BPCHAR], ['Multiple dimensions'::BPCHAR, 'Multiple dimensions'::BPCHAR]]];

--- VARCHAR Arrays
CREATE OR REPLACE FUNCTION updateVarcharArrayIndex(values_array VARCHAR[], desired VARCHAR, index integer[]) RETURNS VARCHAR[] AS $$
int[] arrayInteger = index.Cast<int>().ToArray();
values_array.SetValue(desired, arrayInteger);
return values_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'VARCHAR[]', 'updateVarcharArrayIndex1', updateVarcharArrayIndex(ARRAY['hello'::VARCHAR, 'hi'::VARCHAR, null::VARCHAR, 'bye'::VARCHAR], 'goodbye'::VARCHAR, ARRAY[2]) = ARRAY['hello'::VARCHAR, 'hi'::VARCHAR, 'goodbye'::VARCHAR, 'bye'::VARCHAR];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'VARCHAR[]', 'updateVarcharArrayIndex2', updateVarcharArrayIndex(ARRAY[[null::VARCHAR, null::VARCHAR], [null::VARCHAR, 'bye'::VARCHAR]], 'goodbye'::VARCHAR, ARRAY[1,0]) = ARRAY[[null::VARCHAR, null::VARCHAR], ['goodbye'::VARCHAR, 'bye'::VARCHAR]];

CREATE OR REPLACE FUNCTION AddSmileToVarchars(values_array VARCHAR[]) RETURNS VARCHAR[] AS $$
Array flatten_values = Array.CreateInstance(typeof(object), values_array.Length);
ArrayHandler.FlatArray(values_array, ref flatten_values);
for(int i = 0; i < flatten_values.Length; i++)
{   
    if (flatten_values.GetValue(i) == null)
        continue;

    string orig_value = (string)flatten_values.GetValue(i);
    string new_value = orig_value + " :)";
    
    flatten_values.SetValue((string)new_value, i);
}
return flatten_values;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'VARCHAR[]', 'AddSmileToVarchars1', AddSmileToVarchars(ARRAY['hello'::VARCHAR, 'hi'::VARCHAR, null::VARCHAR, 'bye'::VARCHAR]) = ARRAY['hello :)'::VARCHAR, 'hi :)'::VARCHAR, null::VARCHAR, 'bye :)'::VARCHAR];

CREATE OR REPLACE FUNCTION GetVarcharMultidimensionArray() RETURNS VARCHAR[] AS $$
string objects_value = "Multiple dimensions";
string?[, ,] three_dimensional_array = new string?[2, 2, 2] {{{objects_value, objects_value}, {null, null}}, {{objects_value, null}, {objects_value, objects_value}}};
return three_dimensional_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'VARCHAR[]', 'GetVarcharMultidimensionArray', GetVarcharMultidimensionArray() = ARRAY[[['Multiple dimensions'::VARCHAR, 'Multiple dimensions'::VARCHAR], [null::VARCHAR, null::VARCHAR]], [['Multiple dimensions'::VARCHAR, null::VARCHAR], ['Multiple dimensions'::VARCHAR, 'Multiple dimensions'::VARCHAR]]];

--- XML Arrays
CREATE OR REPLACE FUNCTION updateXMLArrayIndex(values_array XML[], desired XML, index integer[]) RETURNS XML[] AS $$
int[] arrayInteger = index.Cast<int>().ToArray();
values_array.SetValue(desired, arrayInteger);
return values_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'XML[]', 'updateXMLArrayIndex1', updateXMLArrayIndex(ARRAY['<?xml version="1.0" encoding="utf-8"?><title>Hello, World!</title>'::XML, '<?xml version="1.0" encoding="utf-8"?><title>Test 1!</title>'::XML, null::XML, '<?xml version="1.0" encoding="utf-8"?><title>Goodbye, World!</title>'::XML], '<?xml version="1.0" encoding="utf-8"?><title>Writing tests!</title>'::XML, ARRAY[2])::TEXT = ARRAY['<?xml version="1.0" encoding="utf-8"?><title>Hello, World!</title>'::XML, '<?xml version="1.0" encoding="utf-8"?><title>Test 1!</title>'::XML, '<?xml version="1.0" encoding="utf-8"?><title>Writing tests!</title>'::XML, '<?xml version="1.0" encoding="utf-8"?><title>Goodbye, World!</title>'::XML]::TEXT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'XML[]', 'updateXMLArrayIndex2', updateXMLArrayIndex(ARRAY[[null::XML, null::XML], [null::XML, '<?xml version="1.0" encoding="utf-8"?><title>Goodbye, World!</title>'::XML]], '<?xml version="1.0" encoding="utf-8"?><title>Writing tests!</title>'::XML, ARRAY[1,0])::TEXT = ARRAY[[null::XML, null::XML], ['<?xml version="1.0" encoding="utf-8"?><title>Writing tests!</title>'::XML, '<?xml version="1.0" encoding="utf-8"?><title>Goodbye, World!</title>'::XML]]::TEXT;

CREATE OR REPLACE FUNCTION ReplaceXMLsWord(values_array XML[]) RETURNS XML[] AS $$
Array flatten_values = Array.CreateInstance(typeof(object), values_array.Length);
ArrayHandler.FlatArray(values_array, ref flatten_values);
for(int i = 0; i < flatten_values.Length; i++)
{   
    if (flatten_values.GetValue(i) == null)
        continue;

    string orig_value = (string)flatten_values.GetValue(i);
    string new_value = orig_value.Replace("Hello", "Goodbye");
    
    flatten_values.SetValue((string)new_value, i);
}
return flatten_values;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'XML[]', 'ReplaceXMLsWord1', ReplaceXMLsWord(ARRAY['Hello Mikael'::XML, 'Hello Rosicley'::XML, null::XML, 'Hello Todd'::XML])::TEXT = ARRAY['Goodbye Mikael'::XML, 'Goodbye Rosicley'::XML, null::XML, 'Goodbye Todd'::XML]::TEXT;

CREATE OR REPLACE FUNCTION GetXMLMultidimensionArray() RETURNS XML[] AS $$
string objects_value = "<?xml version=\"1.0\" encoding=\"utf-8\"?><title>Hello, World!</title>";
string?[, ,] three_dimensional_array = new string?[2, 2, 2] {{{objects_value, objects_value}, {null, null}}, {{objects_value, null}, {objects_value, objects_value}}};
return three_dimensional_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'XML[]', 'GetXMLMultidimensionArray', GetXMLMultidimensionArray()::TEXT = ARRAY[[['<?xml version="1.0" encoding="utf-8"?><title>Hello, World!</title>'::XML, '<?xml version="1.0" encoding="utf-8"?><title>Hello, World!</title>'::XML], [null::XML, null::XML]], [['<?xml version="1.0" encoding="utf-8"?><title>Hello, World!</title>'::XML, null::XML], ['<?xml version="1.0" encoding="utf-8"?><title>Hello, World!</title>'::XML, '<?xml version="1.0" encoding="utf-8"?><title>Hello, World!</title>'::XML]]]::TEXT;
