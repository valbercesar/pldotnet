CREATE OR REPLACE FUNCTION returnInvalidBoolFSharp() RETURNS boolean AS $$
Some True
$$ LANGUAGE plfsharp;
SELECT returnInvalidBoolFSharp() is true;

CREATE OR REPLACE FUNCTION returnValidBoolFSharp() RETURNS boolean AS $$
Some true
$$ LANGUAGE plfsharp;
SELECT returnValidBoolFSharp() is true;

CREATE OR REPLACE FUNCTION returnInvalidVoidFSharp() RETURNS boolean AS $$
()
$$ LANGUAGE plfsharp;
SELECT returnInvalidVoidFSharp() is true;

CREATE OR REPLACE FUNCTION returnInvalidBodyFSharp() RETURNS boolean AS $$
$$ LANGUAGE plfsharp;
SELECT returnInvalidBodyFSharp() is true;

CREATE OR REPLACE FUNCTION returnMissingFSharp() RETURNS boolean AS $$
let a = true
let b = 2
$$ LANGUAGE plfsharp;
SELECT returnMissingValueFSharp() is true;

CREATE OR REPLACE FUNCTION returnInvalidReturnTypeFSharp(a integer, b integer) RETURNS integer AS $$
match (a, b) with
| (Some a, Some b) ->
    let c = a + b
    Convert.ToString(c);
| _ -> ""
$$ LANGUAGE plfsharp;
SELECT returnInvalidReturnTypeFSharp() is '10';

CREATE OR REPLACE FUNCTION returnValidReturnTypeFSharp(a integer, b integer) RETURNS text AS $$
match (a, b) with
| (Some a, Some b) ->
    let c = a + b
    Convert.ToString(c) |> Some
| _ -> None
$$ LANGUAGE plfsharp;
SELECT returnValidReturnTypeFSharp(10::integer, 20::integer) = '30';

CREATE OR REPLACE FUNCTION returnMissingPattern(a integer, b integer) RETURNS text AS $$
match (a, b) with
| (Some a, Some b) ->
    let c = a + b
    Convert.ToString(c) |> Some
| _ ->
$$ LANGUAGE plfsharp;
SELECT returnMissingCommaFSharp(10::integer, 20::integer) = '30';

CREATE OR REPLACE FUNCTION returnInvalidOperationFSharp(a text, b double precision) RETURNS double precision AS $$
match (a, b) with
| (Some a, Some b) -> Math.Sqrt(a) + b
| _ -> -1.0
$$ LANGUAGE plfsharp;
SELECT returnInvalidOperationFSharp('9', 20::double precision) = 23::double precision;

CREATE OR REPLACE FUNCTION returnValidOperationFSharp(a text, b double precision) RETURNS double precision AS $$
match (a, b) with
| (Some _a, Some _b) -> Math.Sqrt(Convert.ToDouble(_a)) + _b |> Some
| _ -> None
$$ LANGUAGE plfsharp;
SELECT returnValidOperationFSharp('9', 20::double precision) = 23::double precision;
SELECT returnValidOperationFSharp(null, 20::double precision) is null;
SELECT returnValidOperationFSharp('9', null) is null;

CREATE OR REPLACE FUNCTION returnUnknownNameFSharp(a varchar, b integer) RETURNS varchar AS $$
match (a, b) with
| (Some _a, Some _b) -> append_integer_to_string a b |> Some
| _ -> None
$$ LANGUAGE plfsharp;
SELECT returnUnknownNameFSharp('this is the integer: ', 123::integer) = 'this is the integer: 123';

CREATE OR REPLACE FUNCTION returnKnownNameFSharp(a varchar, b integer) RETURNS varchar AS $$
let append_integer_to_string (_a : string) (_b : int option) : string option =
    match _b with
    | Some v -> String.Concat(_a, Convert.ToString(v)) |> Some
    | _ -> Some _a
match a with
| Some _a -> append_integer_to_string _a b
| _ -> None
$$ LANGUAGE plfsharp;
SELECT returnKnownNameFSharp('this is the integer: ', 123::integer) = 'this is the integer: 123';
