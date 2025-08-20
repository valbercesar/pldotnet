using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

[Trait("Language", "FSharp")]
[Trait("Category", "EntityFramework")]
public class GetAnyInactiveTestsFSharp : BaseGetAnyInactiveTests
{
    protected override string FunctionBody => @"
use ctx = new EFCoreTest.TestEntitiesContext()
ctx.TestEntities.Any(fun e -> not e.IsActive)
    ";
    protected override LanguageType Language => LanguageType.PlfSharp;
}
