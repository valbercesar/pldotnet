CREATE OR REPLACE FUNCTION fibbbPg(n integer) RETURNS integer AS $$
DECLARE
    ret integer;
BEGIN
    ret := 1;
    if n = 1 OR n = 2 THEN
        RETURN ret;
    END IF;
    RETURN fibbbPg(n-1) + fibbbPg(n-2);
END
$$ LANGUAGE plpgsql;
SELECT fibbbPg(30) = integer '832040';

CREATE OR REPLACE FUNCTION factPg(n integer) RETURNS integer AS $$
DECLARE
    ret integer;
BEGIN
    ret := 1;
    IF n <= 1 THEN
        RETURN ret;
    ELSE
    	RETURN n*factPg(n-1);
    END IF;
END
$$ LANGUAGE plpgsql;
SELECT factPg(5) = integer '120';

CREATE OR REPLACE FUNCTION naturalPg(n numeric) RETURNS numeric AS $$
DECLARE
BEGIN
    IF n < 0 THEN
        RETURN 0;
    ELSIF n = 1 THEN
	    RETURN 1;
    ELSE
        RETURN natural(n-1);
    END IF;
END
$$ LANGUAGE plpgsql;
SELECT naturalPg(10) =  numeric '1';
SELECT naturalPg(10.5) = numeric '0';
