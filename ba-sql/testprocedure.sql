CREATE PROCEDURE printSum(a integer, b integer) AS $$
int c = (int)a + (int)b;
Elog.pldotnet_Info($"c = {c}");
$$ LANGUAGE plcsharp;
CALL printSum(10, 25);
CALL printSum(1450, 275);

CREATE PROCEDURE printSmallestValue(doublevalues double precision[]) AS $$
double min = double.MaxValue;
for(int i = 0; i < doublevalues.Length; i++)
{
    double value = (double)doublevalues.GetValue(i);
    min = min < value ? min : value;
}
Elog.pldotnet_Info($"Minimum value = {min}");
$$ LANGUAGE plcsharp;
CALL printSmallestValue(ARRAY[2.25698, 2.85956, 2.85456, 0.00128, 0.00127, 2.36875]);
CALL printSmallestValue(ARRAY[2.25698, -2.85956, 2.85456, -0.00128, 0.00127, 12.36875, -23.2354]);
