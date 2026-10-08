using ARLang.Core;

namespace ARLang.Tests;

public class LexicalAnalyzerTests
{
    [Fact]
    public void ExpressionContainsNoInvalidCharacters_SuccessfulLexicalOutput()
    {
        string expression1 = "1+2*3";
        var actuals = new LexicalAnalyzer().ProduceTokens(expression1);

        List<Token> expected = [
            new TokenNumericConstant(1),
            new TokenPlus(),
            new TokenNumericConstant(2),
            new TokenStar(),
            new TokenNumericConstant(3)
            ];
        Assert.Equal(expected.Count, actuals.Count);
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }

    [Fact]
    public void ExpressionWithWhitespacesContainsNoInvalidCharacters_SuccessfulLexicalOutput()
    {
        string expression1 = " 34 + 2.3 * 0.55";
        var actuals = new LexicalAnalyzer().ProduceTokens(expression1);

        List<Token> expected = [
            new TokenNumericConstant(34),
            new TokenPlus(),
            new TokenNumericConstant(2.3),
            new TokenStar(),
            new TokenNumericConstant(0.55)
            ];
        Assert.Equal(expected.Count, actuals.Count);
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }

    [Fact]
    public void ExpressionWithWhitespacesAndTabsContainsNoInvalidCharacters_SuccessfulLexicalOutput()
    {
        string expression1 = "\t 34   + 2.3\t  \n * 0.55\t";
        var actuals = new LexicalAnalyzer().ProduceTokens(expression1);

        List<Token> expected = [
            new TokenNumericConstant(34),
            new TokenPlus(),
            new TokenNumericConstant(2.3),
            new TokenStar(),
            new TokenNumericConstant(0.55)
            ];
        Assert.Equal(expected.Count, actuals.Count);
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }

    [Fact]
    public void EmptyExpression_EmptyTokenResult()
    {
        string expression1 = "";
        var actuals = new LexicalAnalyzer().ProduceTokens(expression1);

        Assert.NotNull(actuals);
        Assert.Empty(actuals);
    }

    [Fact]
    public void IncorrectExpressionButWithoutAnyInvalidCharacters_SuccessfulLexicalOutput()
    {
        string expression1 = "(*)76.23/11+-";
        var actuals = new LexicalAnalyzer().ProduceTokens(expression1);

        List<Token> expected = [
            new TokenOpenParenthesis(),
            new TokenStar(),
            new TokenCloseParenthesis(),
            new TokenNumericConstant(76.23),
            new TokenSlash(),
            new TokenNumericConstant(11),
            new TokenPlus(),
            new TokenMinus()
            ];

        Assert.Equal(expected.Count, actuals.Count);
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }

    [Fact]
    public void IncorrectExpressionWithInvalidCharacter_ContainsInvalidToken()
    {
        string expression1 = "6..2";
        var actuals = new LexicalAnalyzer().ProduceTokens(expression1);

        List<Token> expected = [
            new TokenInvalid($"Invalid token {expression1}")
            ];

        Assert.Equal(expected.Count, actuals.Count);
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }

    [Fact]
    public void IncorrectExpressionWithInvalidCharacter2_ContainsInvalidToken()
    {
        string expression1 = "1~2";
        var actuals = new LexicalAnalyzer().ProduceTokens(expression1);

        List<Token> expected = [
            new TokenNumericConstant(1),
            new TokenInvalid($"Invalid character ~"),
            new TokenNumericConstant(2)
            ];

        Assert.Equal(expected.Count, actuals.Count);
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }

    [Fact]
    public void InvalidExpressionWithoutAnyInvalidCharacter()
    {
        string expression1 = "1 2";
        var actuals = new LexicalAnalyzer().ProduceTokens(expression1);

        List<Token> expected = [
            new TokenNumericConstant(1),
            new TokenNumericConstant(2)
        ];

        Assert.Equal(expected.Count, actuals.Count);
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }

    [Fact]
    public void InvalidExpressionWithAnyInvalidCharacter()
    {
        string expression1 = "1)";
        var actuals = new LexicalAnalyzer().ProduceTokens(expression1);

        List<Token> expected = [
            new TokenNumericConstant(1),
            new TokenCloseParenthesis()
        ];

        Assert.Equal(expected.Count, actuals.Count);
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }
}