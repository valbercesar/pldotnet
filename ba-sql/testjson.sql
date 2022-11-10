-- JSON

CREATE OR REPLACE FUNCTION returnJson(a JSON) RETURNS JSON AS $$
    if(a==null)
        return "{\"NULL\": \"NULL_NULL_NULL\"}";
    return a;
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'JSON', 'returnJson1', returnJson('{"a":"Sunday", "b":"Monday", "c":"Tuesday"}'::JSON)::TEXT = '{"a":"Sunday", "b":"Monday", "c":"Tuesday"}'::JSON::TEXT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'JSON', 'returnJson2', returnJson('{"a":"Sunday", "c":"Tuesday", "b":"Monday"}'::JSON)::TEXT = '{"a":"Sunday", "c":"Tuesday", "b":"Monday"}'::JSON::TEXT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'JSON', 'returnJson3', returnJson('{"Sunday":"2022-11-06", "Monday":"2022-11-07", "Tuesday":"2022-11-08"}'::JSON)::TEXT = '{"Sunday":"2022-11-06", "Monday":"2022-11-07", "Tuesday":"2022-11-08"}'::JSON::TEXT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'JSON', 'returnJson4', returnJson(NULL::JSON)::TEXT = '{"NULL": "NULL_NULL_NULL"}'::JSON::TEXT;

CREATE OR REPLACE FUNCTION modifyJson(a JSON, b TEXT, c TEXT) RETURNS JSON AS $$
    string new_value = $", \"{b}\":\"{c}\""+"}";
    return a.Replace("}", new_value);
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'JSON', 'modifyJson1', modifyJson('{"a":"Sunday", "b":"Monday"}'::JSON, 'c'::TEXT, 'Tuesday'::TEXT)::TEXT = '{"a":"Sunday", "b":"Monday", "c":"Tuesday"}'::JSON::TEXT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'JSON', 'modifyJson2', modifyJson('{"Sunday":"2022-11-06", "Monday":"2022-11-07"}'::JSON, NULL::TEXT, NULL::TEXT)::TEXT = '{"Sunday":"2022-11-06", "Monday":"2022-11-07", "":""}'::JSON::TEXT;

-- JSONB
