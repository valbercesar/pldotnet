CREATE OR REPLACE FUNCTION returnNullBoolFSharp() RETURNS boolean AS $$
None
$$ LANGUAGE plfsharp;
SELECT returnNullBoolFSharp() is NULL;

CREATE OR REPLACE FUNCTION BooleanNullAndFSharp(a boolean, b boolean) RETURNS boolean AS $$
match (a, b) with
| Some _a, Some _b -> Some (_a && _b)
| Some false, None -> Some false
| None, Some false -> Some false
| _ -> None
$$ LANGUAGE plfsharp;
SELECT BooleanNullAndFSharp(true, null) is NULL;
SELECT BooleanNullAndFSharp(null, true) is NULL;
SELECT BooleanNullAndFSharp(false, null) is false;
SELECT BooleanNullAndFSharp(null, false) is false;
SELECT BooleanNullAndFSharp(null, null) is NULL;

CREATE OR REPLACE FUNCTION BooleanNullOrFSharp(a boolean, b boolean) RETURNS boolean AS $$
match (a, b) with
| Some _a, Some _b -> Some (_a || _b)
| Some true, None -> Some true
| None, Some true -> Some true
| _ -> None
$$ LANGUAGE plfsharp;
SELECT BooleanNullOrFSharp(true, null) is true;
SELECT BooleanNullOrFSharp(null, true) is true;
SELECT BooleanNullOrFSharp(false, null) is NULL;
SELECT BooleanNullOrFSharp(null, false) is NULL;
SELECT BooleanNullOrFSharp(null, null) is NULL;

CREATE OR REPLACE FUNCTION BooleanNullXorFSharp(a boolean, b boolean) RETURNS boolean AS $$
match (a, b) with
| Some _a, Some _b -> Some (_a <> _b)
| _ -> None
$$ LANGUAGE plfsharp;
SELECT BooleanNullXorFSharp(true, null) is NULL;
SELECT BooleanNullXorFSharp(null, true) is NULL;
SELECT BooleanNullXorFSharp(false, null) is NULL;
SELECT BooleanNullXorFSharp(null, false) is NULL;
SELECT BooleanNullXorFSharp(null, null) is NULL;
