using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

[Trait("Language", "FSharp")]
[Trait("Category", "EntityFramework")]
public class GetAverageIDTestsFSharp : BaseGetAverageIDTests
{
    protected override string FunctionBody => @"
use ctx = new EFCoreTest.TestEntitiesContext()
ctx.TestEntities.Average(fun e -> float e.Id)
    ";
    protected override LanguageType Language => LanguageType.PlfSharp;
}
