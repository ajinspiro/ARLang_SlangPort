using ARLang.Core;

namespace ARLang.Tests;

public class AbstractSyntaxTreeAndEvaluatorTests
{
    [Fact]
    public void Test1()
    {
        ArlNumericExpression ast1 = new ArlNumericBinaryOperation(
            new ArlNumericConstant(1),
            new Add(),
            new ArlNumericBinaryOperation(
                new ArlNumericConstant(2),
                new Mul(),
                new ArlNumericConstant(3)
            )
        );

        ArlNumericExpression ast2 = new ArlNumericUnaryOperation(new Sub(), ast1);

        Assert.Equal(7, ast1.Evaluate());
        Assert.Equal(-7, ast2.Evaluate());
    }
}
