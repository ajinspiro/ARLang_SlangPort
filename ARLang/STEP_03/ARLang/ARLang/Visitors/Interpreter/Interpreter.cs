using ARLang.SyntaxTree;

namespace ARLang.Visitors.Interpreter;

public class Interpreter : IVisitorBase
{
    public ARLangExpressionBase Visit(ARLangExpressionBase expression)
    {
        return expression switch
        {
            AdditionExpression e => VisitAddition(e),
            SubtractionExpression e => VisitSubtraction(e),
            MultiplicationExpression e => VisitMultiplication(e),
            DivisionExpression e => VisitDivision(e),
            UnaryPlusExpression e => VisitUnaryPlus(e),
            UnaryMinusExpression e => VisitUnaryMinus(e),
            NumericConstantExpression e => e,
            _ => new ErrorExpression("Invalid expression")
        };
    }

    private ARLangExpressionBase VisitAddition(AdditionExpression exp)
    {
        var value1 = Visit(exp.Expression1) as NumericConstantExpression;
        var value2 = Visit(exp.Expression2) as NumericConstantExpression;
        if (value1 is null)
        {
            return new ErrorExpression("Expression 1 failed to evaluate.");
        }
        if (value2 is null)
        {
            return new ErrorExpression("Expression 2 failed to evaluate.");
        }
        return new NumericConstantExpression(value1.Value + value2.Value);
    }

    private ARLangExpressionBase VisitSubtraction(SubtractionExpression exp)
    {
        var value1 = Visit(exp.Expression1) as NumericConstantExpression;
        var value2 = Visit(exp.Expression2) as NumericConstantExpression;
        if (value1 is null)
        {
            return new ErrorExpression("Expression 1 failed to evaluate.");
        }
        if (value2 is null)
        {
            return new ErrorExpression("Expression 2 failed to evaluate.");
        }
        return new NumericConstantExpression(value1.Value - value2.Value);
    }

    private ARLangExpressionBase VisitMultiplication(MultiplicationExpression exp)
    {
        var value1 = Visit(exp.Expression1) as NumericConstantExpression;
        var value2 = Visit(exp.Expression2) as NumericConstantExpression;
        if (value1 is null)
        {
            return new ErrorExpression("Expression 1 failed to evaluate.");
        }
        if (value2 is null)
        {
            return new ErrorExpression("Expression 2 failed to evaluate.");
        }
        return new NumericConstantExpression(value1.Value * value2.Value);
    }

    private ARLangExpressionBase VisitDivision(DivisionExpression exp)
    {
        var value1 = Visit(exp.Expression1) as NumericConstantExpression;
        var value2 = Visit(exp.Expression2) as NumericConstantExpression;
        if (value1 is null)
        {
            return new ErrorExpression("Expression 1 failed to evaluate.");
        }
        if (value2 is null)
        {
            return new ErrorExpression("Expression 2 failed to evaluate.");
        }
        if (value2.Value is 0)
        {
            return new ErrorExpression("Division by zero is undefined.");
        }
        return new NumericConstantExpression(value1.Value / value2.Value);
    }

    private ARLangExpressionBase VisitUnaryPlus(UnaryPlusExpression exp)
    {
        var value = Visit(exp.Expression) as NumericConstantExpression;
        if (value is null)
        {
            return new ErrorExpression("Expression failed to evaluate.");
        }
        return value; // Does nothing to the value
    }

    private ARLangExpressionBase VisitUnaryMinus(UnaryMinusExpression exp)
    {
        var value = Visit(exp.Expression) as NumericConstantExpression;
        if (value is null)
        {
            return new ErrorExpression("Expression failed to evaluate.");
        }
        return new NumericConstantExpression(-value.Value); // Negate the value
    }
}