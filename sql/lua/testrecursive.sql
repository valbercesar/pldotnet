/* Not supported */

-- CREATE OR REPLACE FUNCTION fibbbLua(n integer) RETURNS integer AS $$
-- local spi = require"pllua.spi"
-- if n < 3 then
--    return 1
-- end
-- return spi.execute("SELECT * from fibbbLua(" .. n - 1 .. ")") + spi.execute("SELECT * from fibbbLua(" .. n - 2 .. ")")
-- $$ LANGUAGE pllua;
-- SELECT fibbbLua(30) = integer '832040';

-- CREATE OR REPLACE FUNCTION factLua(n integer) RETURNS integer AS $$
-- local ret = 1
-- if n <= 1 then
--    return ret
-- else
--    return factLua(n-1)
-- end
-- $$ LANGUAGE pllua;
-- SELECT factLua(5) = integer '120';

-- CREATE OR REPLACE FUNCTION naturalLua(n numeric) RETURNS numeric AS $$
-- if n < 0 then
--     return 0
-- elsif (n == 1) then
--     return 1
-- else
--     return naturalLua(n-1)
-- end
-- $$ LANGUAGE pllua;

-- SELECT naturalLua(10) =  numeric '1';

-- SELECT naturalLua(10.5) = numeric '0';
