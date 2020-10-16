CREATE OR REPLACE FUNCTION returnXPerl() RETURNS integer AS $$
return 10;
$$ LANGUAGE plperl;
SELECT returnXPerl() = integer '10';

CREATE OR REPLACE FUNCTION inc2Perl(val integer) RETURNS integer AS $$
return $_[0] + 2;
$$ LANGUAGE plperl;
SELECT inc2Perl(8) = integer '10';

CREATE OR REPLACE FUNCTION sum2Perl(a integer, b integer) RETURNS integer AS $$
return $_[0] + $_[1];
$$ LANGUAGE plperl;
SELECT sum2Perl(3,2) = integer '5';

CREATE OR REPLACE FUNCTION sum3Perl(aaa integer, bbb integer, ccc integer) RETURNS integer AS $$
return $_[0] + $_[1] + $_[2];
$$
LANGUAGE plperl;
SELECT sum3Perl(3,2,1) = integer '6';

CREATE OR REPLACE FUNCTION sum4Perl(a integer, b integer, c integer, d integer) RETURNS integer AS $$
return $_[0] + $_[1] + $_[2] + $_[3];
$$ LANGUAGE plperl;
SELECT sum4Perl(4,3,2,1) = integer '10';
