using ARLang.Core;

namespace ARLang.Tests;

public class ParserTests
{
    [Theory(DisplayName = "Valid expressions produce result")]
    [InlineData("1+2*3", 7)]
    [InlineData("-2*(3+3)", -12)]
    [InlineData("-1 + 2", 1)]
    [InlineData("2 * -3 + 4", -2)]
    [InlineData("--(1 + 2)", 3)]
    public void Parser_Evaluation_Tests(string sourceCode, double expected)
    {
        var tokens = new LexicalAnalyzer().ProduceTokens(sourceCode);
        var parseResult = new Parser().Parse(tokens);
        tokens.ForEach(token => Assert.IsNotType<TokenInvalid>(token.Value));
        Assert.True(parseResult.IsResult);
        Assert.Equal(expected, parseResult.AsResult.Evaluate());
    }

    [Theory(DisplayName = "Valid token sequence that form an invalid expression returns parse error")]
    [InlineData("")]
    [InlineData("1 2")]
    [InlineData("(*)76.23/11+-")]
    [InlineData("1)")]
    [InlineData("1+")]
    [InlineData("1+(2*3")]
    public void InvalidExpression_ReturnsParseError(string sourceCode)
    {
        var tokens = new LexicalAnalyzer().ProduceTokens(sourceCode);
        var parseResult = new Parser().Parse(tokens);
        tokens.ForEach(token => Assert.IsNotType<TokenInvalid>(token.Value));
        Assert.IsType<ArlParseError>(parseResult.Value);
    }
}