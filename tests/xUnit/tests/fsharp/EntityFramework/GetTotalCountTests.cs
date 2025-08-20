using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;

[Trait("Language", "FSharp")]
[Trait("Category", "EntityFramework")]
public class GetTotalCountTestsFSharp : BaseGetTotalCountTests
{
    protected override string FunctionBody => @"
use ctx = new EFCoreTest.TestEntitiesContext()
ctx.TestEntities.Count()
    ";
    protected override LanguageType Language => LanguageType.PlfSharp;
}
