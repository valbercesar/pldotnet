using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

[Trait("Language", "FSharp")]
[Trait("Category", "EntityFramework")]
public class GetSingleOrDefaultByNameTestsFSharp : BaseGetSingleOrDefaultByNameTests
{
    protected override string FunctionBody => @"
let ctx = new EFCoreTest.TestEntitiesContext()
let entity = ctx.TestEntities.SingleOrDefault(fun e -> e.Name = test_name)
match entity with
| null -> 0
| _ -> entity.Id
    ";
    protected override LanguageType Language => LanguageType.PlfSharp;
}
