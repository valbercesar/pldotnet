CREATE OR REPLACE FUNCTION maxSmallIntPg() RETURNS smallint AS $$
DECLARE
BEGIN
    RETURN 32767;
END
$$ LANGUAGE plpgsql;
SELECT maxSmallIntPg() = integer '32767';

CREATE OR REPLACE FUNCTION sum2SmallIntPg(a smallint, b smallint) RETURNS smallint AS $$
DECLARE
BEGIN
    RETURN (a+b);
END
$$ LANGUAGE plpgsql;
SELECT sum2SmallIntPg(CAST(100 AS smallint), CAST(101 AS smallint)) = smallint '201';

CREATE OR REPLACE FUNCTION maxIntegerPg() RETURNS integer AS $$
DECLARE
BEGIN
    RETURN 2147483647;
END
$$ LANGUAGE plpgsql;
SELECT maxIntegerPg() = integer '2147483647';

CREATE OR REPLACE FUNCTION sum2IntegerPg(a integer, b integer) RETURNS integer AS $$
DECLARE
BEGIN
    RETURN a+b;
END
$$ LANGUAGE plpgsql;
SELECT sum2IntegerPg(32770, 100) = bigint '32870';

CREATE OR REPLACE FUNCTION maxBigIntPg() RETURNS bigint AS $$
DECLARE
BEGIN
    RETURN 9223372036854775807;
END
$$ LANGUAGE plpgsql;
SELECT maxBigIntPg() = bigint '9223372036854775807';

CREATE OR REPLACE FUNCTION sum2BigIntPg(a bigint, b bigint) RETURNS bigint AS $$
DECLARE
BEGIN
    RETURN a+b;
END
$$ LANGUAGE plpgsql;
SELECT sum2BigIntPg(9223372036854775707, 100) = bigint '9223372036854775807';

CREATE OR REPLACE FUNCTION mixedBigIntPg(a integer, b bigint, c bigint) RETURNS bigint AS $$
DECLARE
BEGIN
    RETURN a+b+c;
END
$$ LANGUAGE plpgsql;
SELECT mixedBigIntPg(32767,  CAST(2147483647 as bigint), CAST(100 as bigint)) = bigint '2147516514';

CREATE OR REPLACE FUNCTION mixedIntPg(a integer, b smallint, c integer) RETURNS integer AS $$
dECLARE
BEGIN
    RETURN a+b+c;
END
$$ LANGUAGE plpgsql;
SELECT mixedIntPg(32767,  CAST(32767 AS smallint), 100) = integer '65634';
