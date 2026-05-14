using ARLang.Core;
using ARLang.SyntaxTree;
using OneOf.Types;

namespace ARLang.Visitors.Interpreter;

public class Interpreter : IVisitorBase
{
    private readonly SymbolInfoTable SymbolInfoTable = new();

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
        else if (statement is VariableDeclareStatement variableDeclareStatement)
        {
            VisitVariableDeclareStatement(variableDeclareStatement);
        }
        else if (statement is AssignmentStatement assignmentStatement)
        {
            VisitAssignmentStatement(assignmentStatement);
        }
    }

    private void VisitVariableDeclareStatement(VariableDeclareStatement variableDeclareStatement)
    {
        SymbolInfoTable.TryAdd(variableDeclareStatement.SymbolInfo);
    }

    private void VisitAssignmentStatement(AssignmentStatement assignmentStatement)
    {
        if (assignmentStatement.SymbolInfo.SymbolName is null)
        {
            throw new Exception("Variable name not found to assign.");
        }
        ARLangExpressionBase visitedExpression = VisitExpression(assignmentStatement.Expression);
        var union = SymbolInfoTable.Get(assignmentStatement.SymbolInfo.SymbolName);
        if (union.IsT0)
        {
            throw new Exception("Variable entry not found.");
        }
        ARLangValue value = visitedExpression switch
        {
            BooleanConstantExpression b => b.Value,
            NumericConstantExpression n => n.Value,
            StringLiteralExpression s => s.Value,
            _ => new None()
        };
        SymbolInfoTable.TryAssign(new SymbolInfo(TokenType.UNQUOTED_STRING, value, assignmentStatement.SymbolInfo.SymbolName));
    }

    private void VisitPrintLineStatement(PrintLineStatement printlineStatement)
    {
        ARLangExpressionBase exp = VisitExpression(printlineStatement.Expression);
        if (exp is NumericConstantExpression num)
        {
            Console.WriteLine(num.Value);
        }
        else if (exp is BooleanConstantExpression boolean)
        {
            Console.WriteLine(boolean.Value);
        }
        else if (exp is StringLiteralExpression stringVal)
        {
            Console.WriteLine(stringVal.Value);
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
        else if (exp is BooleanConstantExpression boolean)
        {
            Console.Write(boolean.Value);
        }
        else if (exp is StringLiteralExpression stringVal)
        {
            Console.Write(stringVal.Value);
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
            BooleanConstantExpression e => e,
            StringLiteralExpression e => e,
            VariableExpression e => VisitVariableAccessExpression(e),
            _ => new ErrorExpression("Invalid expression")
        };
    }

    private ARLangExpressionBase VisitVariableAccessExpression(VariableExpression e)
    {
        if (e.SymbolInfo.SymbolName is null)
        {
            return new ErrorExpression("Symbolname was null.");
        }
        var union = SymbolInfoTable.Get(e.SymbolInfo.SymbolName);
        if (union.IsT0)
        {
            return new ErrorExpression("Symboltable did not contain an entry for the variable");
        }
        return union.AsT1.Value.Match<ARLangExpressionBase>(
            none => new ErrorExpression("Uninitialized variable was used."),
            number => new NumericConstantExpression(number),
            stringVal => new StringLiteralExpression(stringVal),
            boolVal => new BooleanConstantExpression(boolVal)
        );
    }

    private ARLangExpressionBase VisitAddition(AdditionExpression exp)
    {
        var value1 = VisitExpression(exp.Expression1);
        var value2 = VisitExpression(exp.Expression2);
        ARLangExpressionBase expReturn = (value1, value2) switch
        {
            (NumericConstantExpression n1, NumericConstantExpression n2) => new NumericConstantExpression(n1.Value + n2.Value),
            (StringLiteralExpression s1, StringLiteralExpression s2) => new StringLiteralExpression(string.Concat(s1.Value, s2.Value)),
            _ => new ErrorExpression("Invalid expression passed to addition operator.")
        };
        return expReturn;
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