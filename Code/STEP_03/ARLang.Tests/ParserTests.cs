using ARLang.Core;

namespace ARLang.Tests;

public class ParserTests
{
    [Theory]
    [InlineData("1+2*3", 7)]
    [InlineData("-2*(3+3)", -12)]
    public void Test1(string sourceCode, double expected)
    {
        var tokens = LexicalAnalyzer.Instance.ProduceTokens(sourceCode);
        var parseResult = Parser.Instance.Parse(tokens);
        Assert.True(parseResult.IsResult);
        Assert.Equal(expected, parseResult.AsResult.Evaluate());
    }

    [Fact]
    public void TestName()
    {
        string sourceCode = "(*)76.23/11+-";
        // Given

        // When

        // Then
    }
}