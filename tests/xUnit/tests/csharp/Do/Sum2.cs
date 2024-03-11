using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseDoSum2Tests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseDoSum2Tests()
    {
        FunctionInfo = new SqlFunctionInfo{TestType = SqlTestType.DoBlock, };
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "Do")]
public class DoSum2TestsCSharp : BaseDoSum2Tests
{
    protected override string FunctionBody => @"
do $$
    int c = 1450 + 275;
    Elog.Info($""c = {c}"");
$$ language plcsharp;
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}