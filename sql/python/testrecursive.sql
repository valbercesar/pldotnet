CREATE OR REPLACE FUNCTION fibbbPython(n integer) RETURNS integer AS $$
if n == 1 or n == 2:
   return 1
return plpy.execute("SELECT fibbbPython(%d) as n" % (n-1))[0]["n"] + plpy.execute("SELECT fibbbPython(%d) as n" % (n-2))[0]["n"]
$$ LANGUAGE plpython3u;
SELECT fibbbPython(30) = integer '832040';

CREATE OR REPLACE FUNCTION factPython(n integer) RETURNS integer AS $$
ret = 1
if n <= 1:
   return ret
else:
   return n * plpy.execute("SELECT factPython(%d) as n" % (n-1))[0]["n"]
$$ LANGUAGE plpython3u;
SELECT factPython(5) = integer '120';

CREATE OR REPLACE FUNCTION naturalPython(n numeric) RETURNS numeric AS $$
plpy.notice(n)
if (n < 0):
    return 0
elif (n == 1):
    return 1
else:
    return plpy.execute("SELECT naturalPython(%f) as n" % (n-1))[0]["n"]
$$ LANGUAGE plpython3u;

SELECT naturalPython(10) =  numeric '1';

SELECT naturalPython(10.5) = numeric '0';
