/**************** Null return test functions ***************************/
CREATE OR REPLACE FUNCTION returnNullInt() RETURNS integer AS $$
return null;
$$ LANGUAGE plv8;
SELECT returnNullInt() is NULL;

CREATE OR REPLACE FUNCTION returnNullSmallInt() RETURNS smallint AS $$
return null;
$$ LANGUAGE plv8;
SELECT returnNullSmallInt() is NULL;

CREATE OR REPLACE FUNCTION returnNullBigInt() RETURNS bigint AS $$
return null;
$$ LANGUAGE plv8;
SELECT returnNullBigInt() is NULL;

/**************** Null operations test functions ***************************/
CREATE OR REPLACE FUNCTION sumNullArgInt(a integer, b integer) RETURNS integer AS $$
return a + b;
$$
LANGUAGE plv8;
SELECT sumNullArgInt(null,null) = integer '0';
SELECT sumNullArgInt(null,3) = integer '3';
SELECT sumNullArgInt(3,null) = integer '3';
SELECT sumNullArgInt(3,3) = integer '6';

CREATE OR REPLACE FUNCTION sumNullArgSmallInt(a smallint, b smallint) RETURNS smallint AS $$
return a + b;
$$
LANGUAGE plv8;
SELECT sumNullArgSmallInt(null,null) = integer '0';
SELECT sumNullArgSmallInt(null,CAST(101 AS smallint)) = integer '101';
SELECT sumNullArgSmallInt(CAST(101 AS smallint),null) = integer '101';
SELECT sumNullArgSmallInt(CAST(101 AS smallint),CAST(101 AS smallint)) = smallint '202';

CREATE OR REPLACE FUNCTION sumNullArgBigInt(a bigint, b bigint) RETURNS bigint AS $$
return a + b;
$$
LANGUAGE plv8;
SELECT sumNullArgBigInt(null,null) = integer '0';
SELECT sumNullArgBigInt(null,100) = integer '100';
/* Failing test case */
SELECT sumNullArgBigInt(9223372036854775707,null) = bigint '9223372036854775707';
SELECT sumNullArgBigInt(9223372036854775707,100) = bigint '9223372036854775807';

/**************** Conditional return test functions ***************************/
CREATE OR REPLACE FUNCTION checkedSumNullArgInt(a integer, b integer) RETURNS integer AS $$
if(!a || !b)
    return null;
else
    return a + b;
$$
LANGUAGE plv8;
SELECT checkedSumNullArgInt(null,null) is NULL;
SELECT checkedSumNullArgInt(null,3) is NULL;
SELECT checkedSumNullArgInt(3,null) is NULL;
SELECT checkedSumNullArgInt(3,3) = integer '6';

CREATE OR REPLACE FUNCTION checkedSumNullArgSmallInt(a smallint, b smallint) RETURNS smallint AS $$
if(!a || !b)
    return null;
else
    return a + b;
$$
LANGUAGE plv8;
SELECT checkedSumNullArgSmallInt(null,null) is NULL;
SELECT checkedSumNullArgSmallInt(null,CAST(133 AS smallint)) is NULL;
SELECT checkedSumNullArgSmallInt(CAST(133 AS smallint),null) is NULL;
SELECT checkedSumNullArgSmallInt(CAST(133 AS smallint),CAST(133 AS smallint)) = smallint '266';

CREATE OR REPLACE FUNCTION checkedSumNullArgBigInt(a bigint, b bigint) RETURNS bigint AS $$
if(!a || !b)
    return null;
else
    return a + b;
$$
LANGUAGE plv8 STABLE STRICT;
SELECT checkedSumNullArgBigInt(null,null) is NULL;
SELECT checkedSumNullArgBigInt(null,100) is NULL;
SELECT checkedSumNullArgBigInt(9223372036854775707,null) is NULL;
/* Failing test case*/
SELECT checkedSumNullArgBigInt(9223372036854775707,100) = bigint '9223372036854775807';

/**************** Conditional return test functions (Mixed Args)  ***************************/
CREATE OR REPLACE FUNCTION checkedSumNullArgMixed(a integer, b smallint, c bigint) RETURNS bigint AS $$
if(!a || !b || !c)
    return null;
else
    return a + b + c;
$$
LANGUAGE plv8;
SELECT checkedSumNullArgMixed(null,null,null) is NULL;
SELECT checkedSumNullArgMixed(null,CAST(1313 as smallint),null) is NULL;
SELECT checkedSumNullArgMixed(1313,null,null) is NULL;
SELECT checkedSumNullArgMixed(null,null,3) is NULL;
SELECT checkedSumNullArgMixed(1313,CAST(1313 as smallint), 1313) = smallint '3939';

