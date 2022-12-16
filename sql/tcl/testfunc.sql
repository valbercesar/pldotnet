CREATE OR REPLACE FUNCTION returnXTcl() RETURNS integer AS $$
return 10;
$$ LANGUAGE pltcl;
SELECT returnXTcl() = integer '10';

CREATE OR REPLACE FUNCTION inc2Tcl(val integer) RETURNS integer AS $$
return [expr $1 + 2];
$$
LANGUAGE pltcl;
SELECT inc2Tcl(8) = integer '10';

CREATE OR REPLACE FUNCTION sum2Tcl(a integer, b integer) RETURNS integer AS $$
return [expr $1 + $2];
$$
LANGUAGE pltcl;
SELECT sum2Tcl(3,2) = integer '5';

CREATE OR REPLACE FUNCTION sum3Tcl(aaa integer, bbb integer, ccc integer) RETURNS integer AS $$
return [expr $1 + $2 + $3];
$$
LANGUAGE pltcl;
SELECT sum3Tcl(3,2,1) = integer '6';

CREATE OR REPLACE FUNCTION sum4Tcl(a integer, b integer, c integer, d integer) RETURNS integer AS $$
return [expr $1 + $2 + $3 + $4];
$$
LANGUAGE pltcl;
SELECT sum4Tcl(4,3,2,1) = integer '10';
