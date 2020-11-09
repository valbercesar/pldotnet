CREATE OR REPLACE FUNCTION helloPersonAgeFSharp(per Person) RETURNS integer AS $$
match per with
| None -> None
| Some _per -> Some _per.age
$$ LANGUAGE plfsharp;
SELECT helloPersonAgeFSharp(('John Smith', 38, 85.5, 1.71, 999.999, true)) = integer '38';

CREATE OR REPLACE FUNCTION helloPersonFSharp(per Person) RETURNS Person AS $$
match per with
| None -> None
| Some _per ->
    let mutable p = new person()
    p.name <- _per.name
    p.age <- _per.age + 1
    p.weight <- _per.weight
    p.height <- _per.height
    p.salary <- _per.salary
    p.married <- _per.married
    Some p
$$ LANGUAGE plfsharp;
SELECT helloPersonFSharp(
    ('John Smith'::text, 38, 85.5::double precision, 1.71::real, 999.999::double precision, true)
) = ('John Smith'::text, 39, 85.5::double precision, 1.71::real, 999.999::double precision, true);
