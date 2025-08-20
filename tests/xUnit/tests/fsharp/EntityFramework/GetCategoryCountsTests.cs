using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

[Trait("Language", "FSharp")]
[Trait("Category", "EntityFramework")]
public class GetCategoryCountsTestsFSharp : BaseGetCategoryCountsTests
{
    protected override string FunctionBody => @"
let ctx = new EFCoreTest.TestEntitiesContext()
let groups = ctx.TestEntities.GroupBy(fun e -> e.Category).ToList()
let results =
    groups
    |> Seq.map (fun g -> g.Key + "":"" + g.Count().ToString())
    |> Seq.sort
    |> Seq.toArray
System.String.Join("","", results)
    ";
    protected override LanguageType Language => LanguageType.PlfSharp;
}
