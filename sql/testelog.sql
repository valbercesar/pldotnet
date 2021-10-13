CREATE OR REPLACE FUNCTION elogWarningTest(a text) RETURNS BOOL AS $$
pldotnet_Warning($"Hello {a}!");
return true;
$$ LANGUAGE plcsharp;
SELECT elogWarningTest('World') = true;

CREATE OR REPLACE FUNCTION elogInfoTest(a text) RETURNS BOOL AS $$
pldotnet_Info($"Hello {a}!");
return true;
$$ LANGUAGE plcsharp;
SELECT elogInfoTest('World') = true;
