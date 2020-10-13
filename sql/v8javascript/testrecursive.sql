
CREATE OR REPLACE FUNCTION fibbb(n integer) RETURNS integer AS $$
    const fibbb = plv8.find_function('fibbb')
    var ret = 1;
    if (n == 1 || n == 2) 
        return ret;
    return fibbb(n - 1) + fibbb(n - 2);;
$$ LANGUAGE plv8;
SELECT fibbb(30) = integer '832040';

CREATE OR REPLACE FUNCTION fact(n integer) RETURNS integer AS $$
    const fact = plv8.find_function('fact')
    var ret = 1;
    if (n <= 1) 
        return ret;
    else
    	return n*fact(n - 1);
$$ LANGUAGE plv8;
SELECT fact(5) = integer '120';

CREATE OR REPLACE FUNCTION natural(n numeric) RETURNS numeric AS $$
    const natural = plv8.find_function('natural')
    if (n < 0) 
        return 0;
    else if (n == 1)
	return 1;
    else
    	return natural(n - 1);
$$ LANGUAGE plv8;
SELECT natural(10) =  numeric '1';
SELECT natural(10.5) = numeric '0';
