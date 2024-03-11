using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

public abstract class BaseDoEmojiTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseDoEmojiTests()
    {
        FunctionInfo = new SqlFunctionInfo{TestType = SqlTestType.DoBlock, };
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "Do")]
public class DoEmojiTestsCSharp : BaseDoEmojiTests
{
    protected override string FunctionBody => @"
do $$
    string emoji = ""🐂"";
	Elog.Info($""The emoji \""{emoji}\"" has lenght {emoji.Length}."");

    emoji = ""\ud83e\udd70"";
	Elog.Info($""The emoji \""{emoji}\"" has lenght {emoji.Length}."");
$$ language plcsharp;
    ";
    protected override LanguageType Language => LanguageType.PlcSharp;
}