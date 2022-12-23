CREATE OR REPLACE FUNCTION sumArrayIntFSharp(a integer[]) RETURNS integer AS $$
match a with
| Some _a -> _a |> Array.sum |> Some
| _ -> None
$$ LANGUAGE plfsharp;
SELECT sumArrayIntFSharp( ARRAY[4,1,5] ) = integer '10';

CREATE OR REPLACE FUNCTION sumArrayNumFSharp(a numeric[]) RETURNS numeric AS $$
match a with
| Some _a -> _a |> Array.sum |> Some
| _ -> None
$$ LANGUAGE plfsharp;
SELECT sumArrayNumFSharp( ARRAY[1.00002, 1.00003, 1.00004] ) = numeric '3.00009';

CREATE OR REPLACE FUNCTION sumArrayTextFSharp(a text[]) RETURNS text AS $$
match a with
| Some _a -> _a |> Array.fold (+) "" |> Some
| _ -> None
$$ LANGUAGE plfsharp;
SELECT sumArrayTextFSharp( ARRAY['Rafael', ' da', ' Veiga', ' Cabral'] ) = varchar 'Rafael da Veiga Cabral';
