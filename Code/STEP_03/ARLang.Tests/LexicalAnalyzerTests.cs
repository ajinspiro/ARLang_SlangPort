using ARLang.Core;

namespace ARLang.Tests;

public class LexicalAnalyzerTests
{
    [Fact]
    public void Test1()
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
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }

    [Fact]
    public void Test2()
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
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }

    [Fact]
    public void Test3()
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
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }

    [Fact]
    public void Test4()
    {
        string expression1 = "";
        var actuals = new LexicalAnalyzer().ProduceTokens(expression1);

        Assert.NotNull(actuals);
        Assert.Empty(actuals);
    }

    [Fact]
    public void Test5_SemanticallyInvalidButLexicallyValidExpression()
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

        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }

    [Fact]
    public void Test6_InvalidExpression()
    {
        string expression1 = "6..2";
        var actuals = new LexicalAnalyzer().ProduceTokens(expression1);

        List<Token> expected = [
            new TokenInvalid($"Invalid token {expression1}")
            ];

        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }

    [Fact]
    public void Test7_InvalidExpression()
    {
        string expression1 = "1~2";
        var actuals = new LexicalAnalyzer().ProduceTokens(expression1);

        List<Token> expected = [
            new TokenNumericConstant(1),
            new TokenInvalid($"Invalid character ~"),
            new TokenNumericConstant(2)
            ];

        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }
}