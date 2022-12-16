CREATE OR REPLACE FUNCTION ageTestR(name varchar, age integer, lname varchar) RETURNS varchar AS $$
res <- ''
if (age < 18)
    res <- paste('Hey ', name, ' ', lname, '! Dude you are still a kid.', sep='')
else if (age >= 18 && age < 40)
    res <- paste('Hey ', name, ' ', lname, '! You are in the mood!', sep='')
else
    res <- paste('Hey ', name, ' ', lname, '! You are getting experienced!', sep='')
return(res);
$$ LANGUAGE plr;

SELECT ageTestR('Billy', 10, 'The KID') = varchar 'Hey Billy The KID! Dude you are still a kid.';
SELECT ageTestR('John', 33, 'Smith') =  varchar 'Hey John Smith! You are in the mood!';
SELECT ageTestR('Robson', 41, 'Cruzoe') =  varchar 'Hey Robson Cruzoe! You are getting experienced!';
