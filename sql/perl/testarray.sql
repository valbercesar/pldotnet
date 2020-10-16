CREATE OR REPLACE FUNCTION sumArrayIntPerl(a integer[]) RETURNS integer AS $$
return $_[0] + $_[1] + $_[2];
$$ LANGUAGE plperl;
SELECT sumArrayInt( ARRAY[4,1,5] ) = integer '10';

CREATE OR REPLACE FUNCTION sumArrayNumPerl(a numeric[]) RETURNS numeric AS $$
return $_[0] + $_[1] + $_[2];
$$ LANGUAGE plperl;
SELECT sumArrayNum( ARRAY[1.00002, 1.00003, 1.00004] ) = numeric '3.00009';

CREATE OR REPLACE FUNCTION sumArrayTextPerl(a text[]) RETURNS text AS $$
return $_[0] + $_[1] + $_[2];
$$ LANGUAGE plperl;
SELECT sumArrayText( ARRAY['Rodrigo', ' Silva', ' Lima', ' Bahia'] ) = varchar 'Rodrigo Silva Lima Bahia';
