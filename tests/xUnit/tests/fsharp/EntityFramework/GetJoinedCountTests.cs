using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

[Trait("Language", "FSharp")]
[Trait("Category", "EntityFramework")]
public class GetJoinedCountTestsFSharp : BaseGetJoinedCountTests
{
    protected override string FunctionBody => @"
let ctx = new EFCoreTest.TestEntitiesContext()
ctx.TestEntities.Join(ctx.Categories, (fun t -> t.Category), (fun c -> c.Category), (fun t c -> t)).Count()
    ";
    protected override LanguageType Language => LanguageType.PlfSharp;
}
