using ARLang.SyntaxTree;
using OneOf.Types;

namespace ARLang.Visitors.Interpreter;

public class Interpreter : IVisitorBase
{
    private RuntimeScope RuntimeScope = new();
    private List<ARLangDefinitionBase> syntaxTree = [];
    public void Visit(List<ARLangDefinitionBase> syntaxTree)
    {
        this.syntaxTree = syntaxTree;
        var mainFunction = syntaxTree.Cast<FunctionDefinition>().First(x => x.Name.ToLowerInvariant() == "Main".ToLowerInvariant());
        VisitStatements(mainFunction.Body);
    }

    private Result VisitStatements(List<ARLangStatementBase> statements)
    {
        foreach (var statement in statements)
        {
            Result result = VisitStatement(statement);
            if (result.IsSuccess)
            {
                continue;
            }
            else
            {
                return result;
            }
        }
        return new Success();
    }

    private Result VisitStatement(ARLangStatementBase statement)
    {
        if (statement is PrintLineStatement printlineStatement)
        {
            VisitPrintLineStatement(printlineStatement);
            return new Success();
        }
        else if (statement is PrintStatement printStatement)
        {
            VisitPrintStatement(printStatement);
            return new Success();
        }
        else if (statement is VariableDeclareStatement variableDeclareStatement)
        {
            VisitVariableDeclareStatement(variableDeclareStatement);
            return new Success();
        }
        else if (statement is AssignmentStatement assignmentStatement)
        {
            VisitAssignmentStatement(assignmentStatement);
            return new Success();
        }
        else if (statement is IfStatement ifStatement)
        {
            VisitIfStatement(ifStatement);
            return new Success();
        }
        else if (statement is WhileStatement whileStatement)
        {
            VisitWhileStatement(whileStatement);
            return new Success();
        }
        else if (statement is ReturnStatement returnStatement)
        {
            ReturnResult result = VisitReturnStatement(returnStatement);
            return result;
        }
        else
        {
            throw new Exception("INTERPRETER: Not possible.");
        }
    }

    // private ARLangExpressionBase VisitFunctionCallExpression(FunctionDefinition function)
    // {
    //     RuntimeScope newRuntimeScope = new(RuntimeScope);
    //     RuntimeScope = newRuntimeScope;
    //     syntaxTree.Cast<FunctionDefinition>().First(f => f.Name == )
    // }

    private ReturnResult VisitReturnStatement(ReturnStatement returnStatement)
    {
        var value = VisitExpression(returnStatement.Expression);
        return new ReturnResult(value);
    }

    private void VisitWhileStatement(WhileStatement whileStatement)
    {
        while (true)
        {
            ARLangExpressionBase conditionValue = VisitExpression(whileStatement.Condition);
            var conditionValueBoolean = conditionValue as BooleanConstantExpression ?? throw new Exception("INTERPRETER: Condition evaluation did not produce boolean.");
            if (conditionValueBoolean.Value)
            {
                VisitStatements(whileStatement.Body);
            }
            else
            {
                break;
            }
        }
    }

    private void VisitIfStatement(IfStatement ifStatement)
    {
        ARLangExpressionBase conditionValue = VisitExpression(ifStatement.Condition);
        var conditionValueBoolean = conditionValue as BooleanConstantExpression ?? throw new Exception("INTERPRETER: Condition evaluation did not produce boolean.");
        if (conditionValueBoolean.Value)
        {
            VisitStatements(ifStatement.ThenBranch);
            return;
        }
        else if (ifStatement.ElseBranch is not null)
        {
            VisitStatements(ifStatement.ElseBranch);
        }
    }

    private void VisitVariableDeclareStatement(VariableDeclareStatement variableDeclareStatement)
    {
        RuntimeScope.Declare(variableDeclareStatement.Name, variableDeclareStatement.DataType);
    }

    private void VisitAssignmentStatement(AssignmentStatement assignmentStatement)
    {
        ARLangExpressionBase visitedExpression = VisitExpression(assignmentStatement.Expression);

        ARLangValue value = visitedExpression switch
        {
            BooleanConstantExpression b => b.Value,
            NumericConstantExpression n => n.Value,
            StringLiteralExpression s => s.Value,
            _ => throw new Exception("INTERPRETER: Not possible.")
        };
        RuntimeScope.Assign(assignmentStatement.Variable.Name, value);
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
            Console.Error.WriteLine("INTERPRETER: Invalid type of expression received in printline statement");
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
            Console.Error.WriteLine("INTERPRETER: Invalid type of expression received in printline statement");
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
            RelationalEqExpression e => VisitRelationalEqExpression(e),
            RelationalGtExpression e => VisitRelationalGtExpression(e),
            RelationalLtExpression e => VisitRelationalLtExpression(e),
            RelationalGteExpression e => VisitRelationalGteExpression(e),
            RelationalLteExpression e => VisitRelationalLteExpression(e),
            RelationalNeqExpression e => VisitRelationalNeqExpression(e),
            LogicalAndExpression e => VisitLogicalAndExpression(e),
            LogicalOrExpression e => VisitLogicalOrExpression(e),
            LogicalNotExpression e => VisitLogicalNotExpression(e),
            _ => new ErrorExpression("INTERPRETER: Invalid expression")
        };
    }

    private ARLangExpressionBase VisitLogicalNotExpression(LogicalNotExpression e)
    {
        ARLangExpressionBase expression = VisitExpression(e.Expression);
        return expression switch
        {
            BooleanConstantExpression b => new BooleanConstantExpression(!b.Value),
            _ => new ErrorExpression("INTERPRETER: Invalid expression")
        };
    }

    private ARLangExpressionBase VisitLogicalOrExpression(LogicalOrExpression e)
    {
        var result1 = VisitExpression(e.Expression1);
        var result2 = VisitExpression(e.Expression2);

        return (result1, result2) switch
        {
            (BooleanConstantExpression b1, BooleanConstantExpression b2) => new BooleanConstantExpression(b1.Value || b2.Value),
            _ => new ErrorExpression("INTERPRETER: Invalid logical AND operation")
        };
    }

    private ARLangExpressionBase VisitLogicalAndExpression(LogicalAndExpression e)
    {
        var result1 = VisitExpression(e.Expression1);
        var result2 = VisitExpression(e.Expression2);

        return (result1, result2) switch
        {
            (BooleanConstantExpression b1, BooleanConstantExpression b2) => new BooleanConstantExpression(b1.Value && b2.Value),
            _ => new ErrorExpression("INTERPRETER: Invalid logical AND operation")
        };
    }

    private ARLangExpressionBase VisitRelationalNeqExpression(RelationalNeqExpression e)
    {
        var result1 = VisitExpression(e.Expression1);
        var result2 = VisitExpression(e.Expression2);
        return (result1, result2) switch
        {
            (NumericConstantExpression n1, NumericConstantExpression n2) => new BooleanConstantExpression(n1.Value != n2.Value),
            (BooleanConstantExpression b1, BooleanConstantExpression b2) => new BooleanConstantExpression(b1.Value != b2.Value),
            (StringLiteralExpression s1, StringLiteralExpression s2) => new BooleanConstantExpression(s1.Value != s2.Value),
            _ => new ErrorExpression("INTERPRETER: Invalid relational NEQ operation")
        };
    }

    private ARLangExpressionBase VisitRelationalLteExpression(RelationalLteExpression e)
    {
        var result1 = VisitExpression(e.Expression1);
        var result2 = VisitExpression(e.Expression2);
        return (result1, result2) switch
        {
            (NumericConstantExpression n1, NumericConstantExpression n2) => new BooleanConstantExpression(n1.Value <= n2.Value),
            _ => new ErrorExpression("INTERPRETER: Invalid relational LTE operation")
        };
    }

    private ARLangExpressionBase VisitRelationalGteExpression(RelationalGteExpression e)
    {
        var result1 = VisitExpression(e.Expression1);
        var result2 = VisitExpression(e.Expression2);
        return (result1, result2) switch
        {
            (NumericConstantExpression n1, NumericConstantExpression n2) => new BooleanConstantExpression(n1.Value >= n2.Value),
            _ => new ErrorExpression("INTERPRETER: Invalid relational GTE operation")
        };
    }

    private ARLangExpressionBase VisitRelationalLtExpression(RelationalLtExpression e)
    {
        var result1 = VisitExpression(e.Expression1);
        var result2 = VisitExpression(e.Expression2);
        return (result1, result2) switch
        {
            (NumericConstantExpression n1, NumericConstantExpression n2) => new BooleanConstantExpression(n1.Value < n2.Value),
            _ => new ErrorExpression("INTERPRETER: Invalid relational LT operation")
        };
    }

    private ARLangExpressionBase VisitRelationalGtExpression(RelationalGtExpression e)
    {
        var result1 = VisitExpression(e.Expression1);
        var result2 = VisitExpression(e.Expression2);
        return (result1, result2) switch
        {
            (NumericConstantExpression n1, NumericConstantExpression n2) => new BooleanConstantExpression(n1.Value > n2.Value),
            _ => new ErrorExpression("INTERPRETER: Invalid relational GT operation")
        };
    }

    private ARLangExpressionBase VisitRelationalEqExpression(RelationalEqExpression e)
    {
        var result1 = VisitExpression(e.Expression1);
        var result2 = VisitExpression(e.Expression2);
        return (result1, result2) switch
        {
            (NumericConstantExpression n1, NumericConstantExpression n2) => new BooleanConstantExpression(n1.Value == n2.Value),
            (BooleanConstantExpression b1, BooleanConstantExpression b2) => new BooleanConstantExpression(b1.Value == b2.Value),
            (StringLiteralExpression s1, StringLiteralExpression s2) => new BooleanConstantExpression(s1.Value == s2.Value),
            _ => new ErrorExpression("INTERPRETER: Invalid relational EQ operation")
        };
    }

    private ARLangExpressionBase VisitVariableAccessExpression(VariableExpression e)
    {
        var union = RuntimeScope.Lookup(e.Name);
        if (union.IsT0)
        {
            return new ErrorExpression("INTERPRETER: Runtime environment did not contain an entry for the variable");
        }
        return union.AsT1.Value.Match<ARLangExpressionBase>(
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
            _ => new ErrorExpression("INTERPRETER: Invalid expression passed to addition operator.")
        };
        return expReturn;
    }

    private ARLangExpressionBase VisitSubtraction(SubtractionExpression exp)
    {
        var value1 = VisitExpression(exp.Expression1) as NumericConstantExpression;
        var value2 = VisitExpression(exp.Expression2) as NumericConstantExpression;
        if (value1 is null)
        {
            return new ErrorExpression("INTERPRETER: Expression 1 failed to evaluate.");
        }
        if (value2 is null)
        {
            return new ErrorExpression("INTERPRETER: Expression 2 failed to evaluate.");
        }
        return new NumericConstantExpression(value1.Value - value2.Value);
    }

    private ARLangExpressionBase VisitMultiplication(MultiplicationExpression exp)
    {
        var value1 = VisitExpression(exp.Expression1) as NumericConstantExpression;
        var value2 = VisitExpression(exp.Expression2) as NumericConstantExpression;
        if (value1 is null)
        {
            return new ErrorExpression("INTERPRETER: Expression 1 failed to evaluate.");
        }
        if (value2 is null)
        {
            return new ErrorExpression("INTERPRETER: Expression 2 failed to evaluate.");
        }
        return new NumericConstantExpression(value1.Value * value2.Value);
    }

    private ARLangExpressionBase VisitDivision(DivisionExpression exp)
    {
        var value1 = VisitExpression(exp.Expression1) as NumericConstantExpression;
        var value2 = VisitExpression(exp.Expression2) as NumericConstantExpression;
        if (value1 is null)
        {
            return new ErrorExpression("INTERPRETER: Expression 1 failed to evaluate.");
        }
        if (value2 is null)
        {
            return new ErrorExpression("INTERPRETER: Expression 2 failed to evaluate.");
        }
        if (value2.Value is 0)
        {
            return new ErrorExpression("INTERPRETER: Division by zero is undefined.");
        }
        return new NumericConstantExpression(value1.Value / value2.Value);
    }

    private ARLangExpressionBase VisitUnaryPlus(UnaryPlusExpression exp)
    {
        var value = VisitExpression(exp.Expression) as NumericConstantExpression;
        if (value is null)
        {
            return new ErrorExpression("INTERPRETER: Expression failed to evaluate.");
        }
        return value; // Does nothing to the value
    }

    private ARLangExpressionBase VisitUnaryMinus(UnaryMinusExpression exp)
    {
        var value = VisitExpression(exp.Expression) as NumericConstantExpression;
        if (value is null)
        {
            return new ErrorExpression("INTERPRETER: Expression failed to evaluate.");
        }
        return new NumericConstantExpression(-value.Value); // Negate the value
    }
}