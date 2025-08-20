using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

[Trait("Language", "FSharp")]
[Trait("Category", "EntityFramework")]
public class GetTakeCountTestsFSharp : BaseGetTakeCountTests
{
    protected override string FunctionBody => @"
let ctx = new EFCoreTest.TestEntitiesContext()
let takeValue = if take.HasValue then take.Value else 0
ctx.TestEntities.Take(takeValue).Count()
    ";
    protected override LanguageType Language => LanguageType.PlfSharp;
}
