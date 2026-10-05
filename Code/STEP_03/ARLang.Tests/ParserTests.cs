using ARLang.Core;

namespace ARLang.Tests;

public class ParserTests
{
    [Theory]
    [InlineData("1+2*3", 7)]
    [InlineData("-2*(3+3)", -12)]
    public void Test1(string sourceCode, double expected)
    {
        var tokens = new LexicalAnalyzer().ProduceTokens(sourceCode);
        var parseResult = new Parser().Parse(tokens);
        tokens.ForEach(token => Assert.IsNotType<TokenInvalid>(token.Value));
        Assert.True(parseResult.IsResult);
        Assert.Equal(expected, parseResult.AsResult.Evaluate());
    }

    [Fact]
    public void Test2()
    {
        string sourceCode = "(*)76.23/11+-";
        var tokens = new LexicalAnalyzer().ProduceTokens(sourceCode);
        var parseResult = new Parser().Parse(tokens);
        tokens.ForEach(token => Assert.IsNotType<TokenInvalid>(token.Value));
        parseResult.Switch(
            result => Assert.Fail("Test failed - operation was expected to return a parse error"),
            parseError => { return; }
        );
    }
}