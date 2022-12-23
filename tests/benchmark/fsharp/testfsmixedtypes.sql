CREATE OR REPLACE FUNCTION ageTestFSharp(name varchar, age integer, lname varchar) RETURNS varchar AS $$
match name, age, lname with
| Some _name, Some _age, Some _lname ->
    if (_age < 18) then
        Some (System.Runtime.CompilerServices.FormattableStringFactory.Create("Hey {0} {1}! Dude you are still a kid.", _name, _lname).ToString())
    elif (_age >= 18 && _age < 40) then
        Some (System.Runtime.CompilerServices.FormattableStringFactory.Create("Hey {0} {1}! You are in the mood!", _name, _lname).ToString())
    else
        Some (System.Runtime.CompilerServices.FormattableStringFactory.Create("Hey {0} {1}! You are getting experienced!", _name, _lname).ToString())
| _ -> None
$$ LANGUAGE plfsharp;
SELECT ageTestFSharp('Billy', 10, 'The KID') = varchar 'Hey Billy The KID! Dude you are still a kid.';
SELECT ageTestFSharp('John', 33, 'Smith') =  varchar 'Hey John Smith! You are in the mood!';
SELECT ageTestFSharp('Robson', 41, 'Cruzoe') =  varchar 'Hey Robson Cruzoe! You are getting experienced!';

