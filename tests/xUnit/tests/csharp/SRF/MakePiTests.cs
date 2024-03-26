using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Xunit;

public abstract class BaseMakePiTests : PlDotNetTest
{
    protected abstract string FunctionBody { get; }

    protected abstract LanguageType Language { get; }

    public BaseMakePiTests()
    {
        FunctionInfo = new SqlFunctionInfo
        {
            Name = "MakePi",
            Arguments = new List<FunctionArgument>(),
            ReturnType = "SETOF float8",
            Language = Language,
            IsStrict = false,
            Body = FunctionBody,
        };
    }

    public static object[][] TestCases()
    {
        return new object[][]
        {
            new object[]
            {
                "c#-srf-pi",
                "make_pi-1",
                "",
                @"WITH data AS (SELECT numbers() AS num, make_pi() AS pi_value)
              INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
              SELECT 'c#-srf-pi', 'make_pi-1', pi_value < 3.143 FROM data WHERE num = 1000 LIMIT 1;"
            },
            new object[]
            {
                "c#-srf-pi",
                "make_pi-2",
                "",
                @"WITH data AS (SELECT numbers() AS num, make_pi() AS pi_value)
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-srf-pi', 'make_pi-2', pi_value > 3.141 FROM data WHERE num = 1000 LIMIT 1;
"
            },
        };
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestSum2SmallInT(
        string featureName,
        string testName,
        string input,
        string expectedResult
    )
    {
        RunGenericTest(featureName, testName, input, expectedResult);
    }
}

[Trait("Language", "CSharp")]
[Trait("Category", "SRF")]
public class MakePiTestsCsharp : BaseMakePiTests
{
    protected override string FunctionBody =>
        @"
double sum = 0.0;
for(int i=0;;i++){yield return 4*(sum+=((i%2)==0?1.0:-1.0)/(2*i+1));}";
    protected override LanguageType Language => LanguageType.PlcSharp;
}
