/* Failing because PL/Perl functions cannot call each other directly.

Source https://www.postgresql.org/docs/9.0/plperl-under-the-hood.html

CREATE OR REPLACE FUNCTION fibbbPerl(n integer) RETURNS integer AS $$
my ($n) = @_;
if($n == 1){
   return 2;
}elsif($n == 2){
   return 1;
}
return spi_exec_query("SELECT fibbbPerl(1)") + spi_exec_query("SELECT fibbbPerl(2)");
$$ LANGUAGE plperl;

SELECT fibbbPerl(30) = integer '832040';

CREATE OR REPLACE FUNCTION factPerl(n integer) RETURNS integer AS $$
ret = 1
if n <= 1:
   return ret
else:
   return n * plpy.execute("SELECT factPerl(%d) as n" % (n-1))[0]["n"]
$$ LANGUAGE plperl;
SELECT factPerl(5) = integer '120';

CREATE OR REPLACE FUNCTION naturalPerl(n numeric) RETURNS numeric AS $$
plpy.notice(n)
if (n < 0):
    return 0
elif (n == 1):
    return 1
else:
    return plpy.execute("SELECT naturalPerl(%f) as n" % (n-1))[0]["n"]
$$ LANGUAGE plperl;

SELECT naturalPerl(10) =  numeric '1';

SELECT naturalPerl(10.5) = numeric '0';
*/
