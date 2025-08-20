using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

[Trait("Language", "FSharp")]
[Trait("Category", "EntityFramework")]
public class DeleteEntitiesByIdsTestsFSharp : BaseDeleteEntitiesByIdsTests
{
    protected override string FunctionBody => @"
let ctx = new EFCoreTest.TestEntitiesContext()
ctx.Database.AutoTransactionsEnabled <- false
let idsList = [| for i in 0 .. ids.Length - 1 -> int (ids.GetValue(i).ToString()) |] |> Array.toList
let toDelete = ctx.TestEntities.Where(fun e -> idsList.Contains(e.Id)).ToList()
if toDelete.Count > 0 then
    ctx.TestEntities.RemoveRange(toDelete)
    ctx.SaveChanges() |> ignore
    ";
    protected override LanguageType Language => LanguageType.PlfSharp;
}
