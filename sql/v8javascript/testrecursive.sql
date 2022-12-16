
CREATE OR REPLACE FUNCTION fibbbV8(n integer) RETURNS integer AS $$
    const fibbbV8 = plv8.find_function('fibbbV8')
    var ret = 1;
    if (n == 1 || n == 2) 
        return ret;
    return fibbbV8(n - 1) + fibbbV8(n - 2);;
$$ LANGUAGE plv8;
SELECT fibbbV8(30) = integer '832040';

CREATE OR REPLACE FUNCTION factV8(n integer) RETURNS integer AS $$
    const factV8 = plv8.find_function('factV8')
    var ret = 1;
    if (n <= 1) 
        return ret;
    else
    	return n*factV8(n - 1);
$$ LANGUAGE plv8;
SELECT factV8(5) = integer '120';

CREATE OR REPLACE FUNCTION naturalV8(n numeric) RETURNS numeric AS $$
    const naturalV8 = plv8.find_function('naturalV8')
    if (n < 0) 
        return 0;
    else if (n == 1)
	return 1;
    else
    	return naturalV8(n - 1);
$$ LANGUAGE plv8;
SELECT naturalV8(10) =  numeric '1';
SELECT naturalV8(10.5) = numeric '0';
