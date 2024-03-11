using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseDoMinDouble2Tests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseDoMinDouble2Tests()
    {
        FunctionInfo = new SqlFunctionInfo{TestType = SqlTestType.DoBlock, };
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "Do")]
public class DoMinDouble2TestsCSharp : BaseDoMinDouble2Tests
{
    protected override string FunctionBody => @"
do $$
    double[] doublevalues = {2.25698, -2.85956, 2.85456, -0.00128, 0.00127, 12.36875, -23.2354};
    double min = double.MaxValue;
    for(int i = 0; i < doublevalues.Length; i++)
    {
        double value = (double)doublevalues.GetValue(i);
        min = min < value ? min : value;
    }
    Elog.Info($""Minimum value = {min}"");
$$ language plcsharp;
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}