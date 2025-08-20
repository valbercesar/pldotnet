using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

[Trait("Language", "FSharp")]
[Trait("Category", "EntityFramework")]
public class GetDistinctCategoriesCountTestsFSharp : BaseGetDistinctCategoriesCountTests
{
    protected override string FunctionBody => @"
let ctx = new EFCoreTest.TestEntitiesContext()
ctx.TestEntities.Select(fun e -> e.Category).Distinct().Count()
    ";
    protected override LanguageType Language => LanguageType.PlfSharp;
}
