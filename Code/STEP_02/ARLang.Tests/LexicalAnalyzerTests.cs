using ARLang.Core;

namespace ARLang.Tests;

public class LexicalAnalyzerTests
{
    [Fact]
    public void Test1()
    {
        string expression1 = "1+2*3";
        var actuals = LexicalAnalyzer.Instance.ProduceTokens(expression1);

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
        var actuals = LexicalAnalyzer.Instance.ProduceTokens(expression1);

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
        var actuals = LexicalAnalyzer.Instance.ProduceTokens(expression1);

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
}