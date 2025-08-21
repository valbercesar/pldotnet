using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

[Trait("Language", "FSharp")]
[Trait("Category", "EntityFramework")]
public class GetAllActiveTestsFSharp : BaseGetAllActiveTests
{
    protected override string FunctionBody => @"
use ctx = new EFCoreTest.TestEntitiesContext()
ctx.TestEntities.All(fun e -> e.IsActive)
    ";
    protected override LanguageType Language => LanguageType.PlfSharp;
}
