CREATE OR REPLACE FUNCTION returnXPg() RETURNS integer AS $$
DECLARE
BEGIN
    RETURN 10;
END
$$ LANGUAGE plpgsql;
SELECT returnXPg() = integer '10';

CREATE OR REPLACE FUNCTION inc2Pg(val integer) RETURNS integer AS $$
DECLARE
BEGIN
    RETURN val + 2;
END
$$
LANGUAGE plpgsql;
SELECT inc2Pg(8) = integer '10';

CREATE OR REPLACE FUNCTION sum2Pg(a integer, b integer) RETURNS integer AS $$
DECLARE
BEGIN
    RETURN a + b;
END
$$
LANGUAGE plpgsql;
SELECT sum2Pg(3,2) = integer '5';

CREATE OR REPLACE FUNCTION sum3Pg(aaa integer, bbb integer, ccc integer) RETURNS integer AS $$
DECLARE
BEGIN
    RETURN aaa + bbb + ccc;
END
$$
LANGUAGE plpgsql;
SELECT sum3Pg(3,2,1) = integer '6';

CREATE OR REPLACE FUNCTION sum4Pg(a integer, b integer, c integer, d integer) RETURNS integer AS $$
DECLARE
BEGIN
    RETURN a + b + c + d;
END
$$
LANGUAGE plpgsql;
SELECT sum4Pg(4,3,2,1) = integer '10';
