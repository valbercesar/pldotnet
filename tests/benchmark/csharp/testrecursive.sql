CREATE OR REPLACE FUNCTION fibbb(n integer) RETURNS integer AS $$
    if (n <= 2)
    {
        return 1;
    }
    return fibbb(n-1) + fibbb(n-2);
$$ LANGUAGE plcsharp;
SELECT fibbb(30) = integer '832040';

CREATE OR REPLACE FUNCTION fact(n integer) RETURNS integer AS $$
    if (n <= 1)
        return 1;
    else
    	return n*fact(n.GetValueOrDefault()-1);
$$ LANGUAGE plcsharp;
SELECT fact(5) = integer '120';

-- CREATE OR REPLACE FUNCTION natural(n numeric) RETURNS numeric AS $$
--     if (n < 0)
--         return 0;
--     else if (n == 1)
-- 	return 1;
--     else
--     	return natural(n-1);
-- $$ LANGUAGE plcsharp;
-- SELECT natural(10) =  numeric '1';
-- SELECT natural(10.5) = numeric '0';
