-- Float4 (real): 6 digits of precison
CREATE OR REPLACE FUNCTION returnRealFSharp() RETURNS real AS $$
Some 1.50055f
$$ LANGUAGE plfsharp;
SELECT returnRealFSharp() = real '1.50055';

CREATE OR REPLACE FUNCTION sumRealFSharp(a real, b real) RETURNS real AS $$
match (a, b) with
| Some _a, Some _b -> Some (_a + _b)
| _ -> None
$$ LANGUAGE plfsharp;
SELECT sumRealFSharp(1.50055, 1.50054) = real '3.00109'; -- 3.00109

--- Float8 (double precision): 15 digits of precison
CREATE OR REPLACE FUNCTION returnDoubleFSharp() RETURNS double precision AS $$
Some 11.0050000000005
$$ LANGUAGE plfsharp;
SELECT returnDoubleFSharp() = double precision '11.0050000000005';

CREATE OR REPLACE FUNCTION sumDoubleFSharp(a double precision, b double precision) RETURNS double precision AS $$
match (a, b) with
| Some _a, Some _b -> Some (_a + _b)
| _ -> None
$$ LANGUAGE plfsharp;
SELECT sumDoubleFSharp(10.5000000000055, 10.5000000000054) = double precision  '21.0000000000109'; -- 21.0000000000109
