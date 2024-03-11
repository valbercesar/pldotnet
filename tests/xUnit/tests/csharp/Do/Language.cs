using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseDoLanguageTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseDoLanguageTests()
    {
        FunctionInfo = new SqlFunctionInfo{TestType = SqlTestType.DoBlock, };
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "Do")]
public class DoLanguageTestsCSharp : BaseDoLanguageTests
{
    protected override string FunctionBody => @"
do $$
    string arabic = ""هل تتكلم العربية؟"";
    Elog.Info($""Do you speak Arabic? => {arabic}"");

    string chinese = ""你会说中文吗？"";
    Elog.Info($""Do you speak Chinese? => {chinese}"");

    string japanese = ""あなたは日本語を話しますか？"";
    Elog.Info($""Do you speak Japanese? => {japanese}"");

    string portuguese = ""Você fala português?"";
    Elog.Info($""Do yo speak Portuguese? => {portuguese}"");

    string russian = ""а ты говоришь по русски?"";
    Elog.Info($""Do you speak Russian? => {russian}"");
$$ language plcsharp;
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}