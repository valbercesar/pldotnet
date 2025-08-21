using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

[Trait("Language", "FSharp")]
[Trait("Category", "EntityFramework")]
public class GetSkipCountTestsFSharp : BaseGetSkipCountTests
{
    protected override string FunctionBody => @"
let ctx = new EFCoreTest.TestEntitiesContext()
let skipValue = if skip.HasValue then skip.Value else 0
ctx.TestEntities.Skip(skipValue).Count()
    ";
    protected override LanguageType Language => LanguageType.PlfSharp;
}
