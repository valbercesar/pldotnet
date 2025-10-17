CREATE FUNCTION hello_world() RETURNS text AS $$
return "Hello, World!";
$$ LANGUAGE plcsharp STRICT;

