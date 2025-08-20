using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

[Trait("Language", "FSharp")]
[Trait("Category", "EntityFramework")]
public class InsertEntityTestsFSharp : BaseInsertEntityTests
{
    protected override string FunctionBody => @"
let ctx = new EFCoreTest.TestEntitiesContext()
ctx.Database.AutoTransactionsEnabled <- false
let ent = new EFCoreTest.TestEntityFramework()
ent.Name <- name
ent.Category <- category
ent.Price <- if price.HasValue then price.Value else 0m
ent.CreatedAt <- if created_at.HasValue then created_at.Value else System.DateTime.Now
ent.IsActive <- if is_active.HasValue then is_active.Value else false
ctx.TestEntities.Add(ent) |> ignore
ctx.SaveChanges() |> ignore
";
    protected override LanguageType Language => LanguageType.PlfSharp;
}
