using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

[Trait("Language", "FSharp")]
[Trait("Category", "EntityFramework")]
public class GetFirstOrDefaultNameTestsFSharp : BaseGetFirstOrDefaultNameTests
{
    protected override string FunctionBody => @"
let ctx = new EFCoreTest.TestEntitiesContext()
ctx.TestEntities.Select(fun e -> e.Name).FirstOrDefault()
    ";
    protected override LanguageType Language => LanguageType.PlfSharp;
}
