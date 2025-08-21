using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

[Trait("Language", "FSharp")]
[Trait("Category", "EntityFramework")]
public class GetMaxPriceTestsFSharp : BaseGetMaxPriceTests
{
    protected override string FunctionBody => @"
use ctx = new EFCoreTest.TestEntitiesContext()
ctx.TestEntities.Max(fun e -> e.Price)
    ";
    protected override LanguageType Language => LanguageType.PlfSharp;
}
