using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

[Trait("Language", "FSharp")]
[Trait("Category", "EntityFramework")]
public class UpdateEntityNameTestsFSharp : BaseUpdateEntityNameTests
{
    protected override string FunctionBody => @"
let ctx = new EFCoreTest.TestEntitiesContext()
ctx.Database.AutoTransactionsEnabled <- false
let idValue = id.Value
let entity = ctx.TestEntities.SingleOrDefault(fun e -> e.Id = idValue)
if entity <> null then
    entity.Name <- new_name
    ctx.SaveChanges() |> ignore
    ";
    protected override LanguageType Language => LanguageType.PlfSharp;
}
