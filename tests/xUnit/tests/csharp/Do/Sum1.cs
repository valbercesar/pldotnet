using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseDoSum1Tests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseDoSum1Tests()
    {
        FunctionInfo = new SqlFunctionInfo{TestType = SqlTestType.DoBlock, };
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "Do")]
public class DoSum1TestsCSharp : BaseDoSum1Tests
{
    protected override string FunctionBody => @"
do $$
    int c = 10 + 25;
    Elog.Info($""c = {c}"");
$$ language plcsharp;
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}