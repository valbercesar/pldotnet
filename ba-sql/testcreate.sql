CREATE OR REPLACE FUNCTION fibonacci(n integer) RETURNS integer AS $$
    int? ret = 1;
    if (n == 1 || n == 2) 
        return ret;
    return fibonacci(n.GetValueOrDefault()-1) + fibonacci(n.GetValueOrDefault()-2);;
$$ LANGUAGE plcsharp;

CREATE OR REPLACE FUNCTION updateMoneyArray(values_array MONEY[], desired MONEY, index integer[]) RETURNS MONEY[] AS $$
int[] arrayInteger = index.Cast<int>().ToArray();
values_array.SetValue(desired, arrayInteger);
return values_array;
$$ LANGUAGE plcsharp STRICT;
