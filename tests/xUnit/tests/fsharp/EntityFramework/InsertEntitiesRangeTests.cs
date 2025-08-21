using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

[Trait("Language", "FSharp")]
[Trait("Category", "EntityFramework")]
public class InsertEntitiesRangeTestsFSharp : BaseInsertEntitiesRangeTests
{
    protected override string FunctionBody => @"
let ctx = new EFCoreTest.TestEntitiesContext()
ctx.Database.AutoTransactionsEnabled <- false
let entities = Array.init names_array.Length (fun i ->
    let ent = new EFCoreTest.TestEntityFramework()
    ent.Name <- names_array.GetValue(i).ToString()
    ent.Category <- categories_array.GetValue(i).ToString()
    let priceVal = prices_array.GetValue(i)
    ent.Price <- if priceVal = null then 0m else (priceVal :?> System.Nullable<decimal>).Value
    let dateVal = created_at_array.GetValue(i)
    ent.CreatedAt <- if dateVal = null then System.DateTime.Now else (dateVal :?> System.Nullable<System.DateTime>).Value
    let activeVal = is_active_array.GetValue(i)
    ent.IsActive <- if activeVal = null then false else (activeVal :?> System.Nullable<bool>).Value
    ent)
ctx.TestEntities.AddRange(entities)
ctx.SaveChanges() |> ignore
";
    protected override LanguageType Language => LanguageType.PlfSharp;
}
