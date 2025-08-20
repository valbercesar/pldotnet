using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

[Trait("Language", "FSharp")]
[Trait("Category", "EntityFramework")]
public class DeleteFirstEntityTestsFSharp : BaseDeleteFirstEntityTests
{
    protected override string FunctionBody => @"
let ctx = new EFCoreTest.TestEntitiesContext()
ctx.Database.AutoTransactionsEnabled <- false
let first = ctx.TestEntities.OrderBy(fun e -> e.Id).FirstOrDefault()
if first <> null then
    ctx.TestEntities.Remove(first) |> ignore
    ctx.SaveChanges() |> ignore
    ";
    protected override LanguageType Language => LanguageType.PlfSharp;
}
