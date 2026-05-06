using ARLang.SyntaxTree;

namespace ARLang.Visitors.Interpreter;

public class Interpreter : IVisitorBase
{
    public void Visit(List<ARLangStatementBase> statements)
    {
        foreach (var statement in statements)
        {
            VisitStatement(statement);
        }
    }
    
    private void VisitStatement(ARLangStatementBase statement)
    {
        if (statement is PrintLineStatement printlineStatement)
        {
            VisitPrintLineStatement(printlineStatement);
        }
        else if (statement is PrintStatement printStatement)
        {
            VisitPrintStatement(printStatement);
        }
    }

    private void VisitPrintLineStatement(PrintLineStatement printlineStatement)
    {
        ARLangExpressionBase exp = VisitExpression(printlineStatement.Expression);
        if (exp is NumericConstantExpression num)
        {
            Console.WriteLine(num.Value);
        }
        else if (exp is ErrorExpression error)
        {
            Console.Error.WriteLine(error.Msg);
        }
        else
        {
            Console.Error.WriteLine("Invalid type of expression received in printline statement");
        }
    }

    private void VisitPrintStatement(PrintStatement printStatement)
    {
        ARLangExpressionBase exp = VisitExpression(printStatement.Expression);
        if (exp is NumericConstantExpression num)
        {
            Console.Write(num.Value);
        }
        else if (exp is ErrorExpression error)
        {
            Console.Error.WriteLine(error.Msg);
        }
        else
        {
            Console.Error.WriteLine("Invalid type of expression received in printline statement");
        }
    }

    private ARLangExpressionBase VisitExpression(ARLangExpressionBase expression)
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
        var value1 = VisitExpression(exp.Expression1) as NumericConstantExpression;
        var value2 = VisitExpression(exp.Expression2) as NumericConstantExpression;
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
        var value1 = VisitExpression(exp.Expression1) as NumericConstantExpression;
        var value2 = VisitExpression(exp.Expression2) as NumericConstantExpression;
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
        var value1 = VisitExpression(exp.Expression1) as NumericConstantExpression;
        var value2 = VisitExpression(exp.Expression2) as NumericConstantExpression;
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
        var value1 = VisitExpression(exp.Expression1) as NumericConstantExpression;
        var value2 = VisitExpression(exp.Expression2) as NumericConstantExpression;
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
        var value = VisitExpression(exp.Expression) as NumericConstantExpression;
        if (value is null)
        {
            return new ErrorExpression("Expression failed to evaluate.");
        }
        return value; // Does nothing to the value
    }

    private ARLangExpressionBase VisitUnaryMinus(UnaryMinusExpression exp)
    {
        var value = VisitExpression(exp.Expression) as NumericConstantExpression;
        if (value is null)
        {
            return new ErrorExpression("Expression failed to evaluate.");
        }
        return new NumericConstantExpression(-value.Value); // Negate the value
    }
}