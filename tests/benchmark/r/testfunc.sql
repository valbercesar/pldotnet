CREATE OR REPLACE FUNCTION returnXR() RETURNS integer AS $$
return(10)
$$ LANGUAGE plr;
SELECT returnXR() = integer '10';

CREATE OR REPLACE FUNCTION inc2R(val integer) RETURNS integer AS $$
return(val + 2)
$$
LANGUAGE plr;
SELECT inc2R(8) = integer '10';

CREATE OR REPLACE FUNCTION sum2R(a integer, b integer) RETURNS integer AS $$
return(a + b)
$$
LANGUAGE plr;
SELECT sum2R(3,2) = integer '5';

CREATE OR REPLACE FUNCTION sum3R(aaa integer, bbb integer, ccc integer) RETURNS integer AS $$
return(aaa + bbb + ccc)
$$
LANGUAGE plr;
SELECT sum3R(3,2,1) = integer '6';

CREATE OR REPLACE FUNCTION sum4R(a integer, b integer, c integer, d integer) RETURNS integer AS $$
return(a + b + c + d)
$$
LANGUAGE plr;
SELECT sum4R(4,3,2,1) = integer '10';
