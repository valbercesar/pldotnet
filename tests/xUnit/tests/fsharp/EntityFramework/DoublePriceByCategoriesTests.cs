using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

[Trait("Language", "FSharp")]
[Trait("Category", "EntityFramework")]
public class DoublePriceByCategoriesTestsFSharp : BaseDoublePriceByCategoriesTests
{
    protected override string FunctionBody => @"
let ctx = new EFCoreTest.TestEntitiesContext()
ctx.Database.AutoTransactionsEnabled <- false
let categoriesList = [| for i in 0 .. categories.Length - 1 -> categories.GetValue(i).ToString() |] |> Array.toList
let toUpdate = ctx.TestEntities.Where(fun e -> categoriesList.Contains(e.Category)).ToList()
for e in toUpdate do
    e.Price <- e.Price * 2m
if toUpdate.Count > 0 then
    ctx.SaveChanges() |> ignore
";
    protected override LanguageType Language => LanguageType.PlfSharp;
}
