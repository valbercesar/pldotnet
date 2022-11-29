do $$
    int c = 10 + 25;
    Elog.pldotnet_Info($"c = {c}");
$$ language plcsharp;

do $$
    int c = 1450 + 275;
    Elog.pldotnet_Info($"c = {c}");
$$ language plcsharp;

do $$
    double[] doublevalues = {2.25698, 2.85956, 2.85456, 0.00128, 0.00127, 2.36875};
    double min = double.MaxValue;
    for(int i = 0; i < doublevalues.Length; i++)
    {
        double value = (double)doublevalues.GetValue(i);
        min = min < value ? min : value;
    }
    Elog.pldotnet_Info($"Minimum value = {min}");
$$ language plcsharp;

do $$
    double[] doublevalues = {2.25698, -2.85956, 2.85456, -0.00128, 0.00127, 12.36875, -23.2354};
    double min = double.MaxValue;
    for(int i = 0; i < doublevalues.Length; i++)
    {
        double value = (double)doublevalues.GetValue(i);
        min = min < value ? min : value;
    }
    Elog.pldotnet_Info($"Minimum value = {min}");
$$ language plcsharp;

do $$
    let c = 10 + 25
    Elog.pldotnet_Info("c = " + c.ToString())
$$ language plfsharp;

do $$
    let c = 1450 + 275
    Elog.pldotnet_Info("c = " + c.ToString())
$$ language plfsharp;

do $$
    let message = "PL.NET IS THE BEST PROCEDURE LANGUAGE!"
    Elog.pldotnet_Info(message)
$$ language plfsharp;
