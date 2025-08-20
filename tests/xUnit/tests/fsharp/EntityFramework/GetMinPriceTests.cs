using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

[Trait("Language", "FSharp")]
[Trait("Category", "EntityFramework")]
public class GetMinPriceTestsFSharp : BaseGetMinPriceTests
{
    protected override string FunctionBody => @"
use ctx = new EFCoreTest.TestEntitiesContext()
ctx.TestEntities.Min(fun e -> e.Price)
    ";
    protected override LanguageType Language => LanguageType.PlfSharp;
}
