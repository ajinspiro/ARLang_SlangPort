using ARLang.Core;
using ARLang.SyntaxTree;
using ARLang.Visitors.Interpreter;
using OneOf.Types;

namespace ARLang.Visitors.TypeChecker;

public class TypeChecker : IVisitorBase
{
    private readonly SymbolInfoTable SymbolInfoTable = new();

    public void Visit(List<ARLangStatementBase> statements)
    {
        foreach (var statement in statements)
        {
            var result = VisitStatement(statement);
            if (result.IsError)
            {
                throw new Exception(result.AsError.Value);
            }
        }
    }
    private TypeCheckResult VisitStatement(ARLangStatementBase statement)
    {
        if (statement is PrintLineStatement printlineStatement)
        {
            return VisitPrintLineStatement(printlineStatement);
        }
        else if (statement is PrintStatement printStatement)
        {
            return VisitPrintStatement(printStatement);
        }
        else if (statement is VariableDeclareStatement variableDeclareStatement)
        {
            return VisitVariableDeclareStatement(variableDeclareStatement);
        }
        else if (statement is AssignmentStatement assignmentStatement)
        {
            return VisitAssignmentStatement(assignmentStatement);
        }
        else
        {
            return new Error<string>("TYPE_ERR: Invalid statement.");
        }
    }

    private TypeCheckResult VisitAssignmentStatement(AssignmentStatement assignmentStatement)
    {
        if (assignmentStatement.SymbolInfo.SymbolName is null)
        {
            throw new Exception("TYPE_ERR: Variable name not found to assign.");
        }
        TypeCheckResult expressionType = VisitExpression(assignmentStatement.Expression);
        if (expressionType.IsError)
        {
            return expressionType.AsError;
        }
        TypeCheckResult s = assignmentStatement.SymbolInfo.TokenType switch
        {
            TokenType.VARIABLE_NUMBER => SupportedTypes.Numeric,
            TokenType.VARIABLE_STRING => SupportedTypes.String,
            TokenType.VARIABLE_BOOL => SupportedTypes.Boolean,
            _ => new Error<string>($"TYPE_ERR: Invalid datatype.")
        };
        if (s.IsError)
        {
            return s.AsError;
        }
        if (s.AsSuccess == expressionType.AsSuccess)
        {
            // Type check pass.
            ARLangValue value = s.AsSuccess switch
            {
                SupportedTypes.Numeric => default(double),
                SupportedTypes.Boolean => default(bool),
                SupportedTypes.String => string.Empty,
                _ => throw new Exception("TYPE_ERR: Not possible.")
            };
            SymbolInfoTable.TryAssign(new SymbolInfo(TokenType.UNQUOTED_STRING, value, assignmentStatement.SymbolInfo.SymbolName));
            return s.AsSuccess;
        }
        else
        {
            // Type check failed.
            return new Error<string>("TYPE_ERR: Assignment failed due to type mismatch.");
        }
    }

    private TypeCheckResult VisitVariableDeclareStatement(VariableDeclareStatement variableDeclareStatement)
    {
        bool isSuccess = SymbolInfoTable.TryAdd(variableDeclareStatement.SymbolInfo);
        if (!isSuccess)
        {
            return new Error<string>($"Redeclaration of variable '{variableDeclareStatement.SymbolInfo.SymbolName}' detected.");
        }
        return variableDeclareStatement.SymbolInfo.TokenType switch
        {
            TokenType.VARIABLE_NUMBER => SupportedTypes.Numeric,
            TokenType.VARIABLE_STRING => SupportedTypes.String,
            TokenType.VARIABLE_BOOL => SupportedTypes.Boolean,
            _ => new Error<string>($"TYPE_ERR: Invalid datatype.")
        };
    }

    private TypeCheckResult VisitPrintLineStatement(PrintLineStatement printlineStatement)
    {
        return VisitExpression(printlineStatement.Expression);
    }
    private TypeCheckResult VisitPrintStatement(PrintStatement printStatement)
    {
        return VisitExpression(printStatement.Expression);
    }
    private TypeCheckResult VisitExpression(ARLangExpressionBase expression)
    {
        return expression switch
        {
            AdditionExpression e => VisitAddition(e),
            SubtractionExpression e => VisitSubtraction(e),
            MultiplicationExpression e => VisitMultiplication(e),
            DivisionExpression e => VisitDivision(e),
            UnaryPlusExpression e => VisitUnaryPlus(e),
            UnaryMinusExpression e => VisitUnaryMinus(e),
            NumericConstantExpression => SupportedTypes.Numeric,
            BooleanConstantExpression => SupportedTypes.Boolean,
            StringLiteralExpression => SupportedTypes.String,
            VariableExpression e => VisitVariableAccessExpression(e),
            _ => new Error<string>("Invalid expression")
        };
    }

    private TypeCheckResult VisitVariableAccessExpression(VariableExpression e)
    {
        if (e.SymbolInfo.SymbolName is null)
        {
            return new Error<string>("TYPE_ERR: Symbolname was null.");
        }
        var union = SymbolInfoTable.Get(e.SymbolInfo.SymbolName);
        if (union.IsT0)
        {
            return new Error<string>("TYPE_ERR: Uninitialized variable was accessed. Initialize before use.");
        }
        return union.AsT1.Value.Match<TypeCheckResult>(
            none => new Error<string>("TYPE_ERR: Uninitialized variable was used."),
            number => SupportedTypes.Numeric,
            stringVal => SupportedTypes.String,
            boolVal => SupportedTypes.Boolean
        );
    }

    private TypeCheckResult VisitUnaryPlus(UnaryPlusExpression e)
    {
        var result = VisitExpression(e.Expression);
        return VisitUnaryCommon(result);
    }
    private TypeCheckResult VisitUnaryMinus(UnaryMinusExpression e)
    {
        var result = VisitExpression(e.Expression);
        return VisitUnaryCommon(result);
    }
    private static TypeCheckResult VisitUnaryCommon(TypeCheckResult result)
    {
        return result.Match<TypeCheckResult>(
            error => error,
            success =>
            {
                if (success == SupportedTypes.Numeric)
                    return SupportedTypes.Numeric;
                else
                    return new Error<string>("TYPE_ERR: Non numeric symbol received for unary operation");
            }
        );
    }
    private TypeCheckResult VisitDivision(DivisionExpression e)
    {
        var result1 = VisitExpression(e.Expression1);
        var result2 = VisitExpression(e.Expression2);
        return VisitBinaryCommon(result1, result2);
    }
    private TypeCheckResult VisitMultiplication(MultiplicationExpression e)
    {
        var result1 = VisitExpression(e.Expression1);
        var result2 = VisitExpression(e.Expression2);
        return VisitBinaryCommon(result1, result2);
    }
    private TypeCheckResult VisitSubtraction(SubtractionExpression e)
    {
        var result1 = VisitExpression(e.Expression1);
        var result2 = VisitExpression(e.Expression2);
        return VisitBinaryCommon(result1, result2);
    }
    private TypeCheckResult VisitAddition(AdditionExpression e)
    {
        var result1 = VisitExpression(e.Expression1);
        var result2 = VisitExpression(e.Expression2);
        return VisitBinaryCommon(result1, result2);
    }
    private static TypeCheckResult VisitBinaryCommon(TypeCheckResult result1, TypeCheckResult result2)
    {
        var resultAtom1 = result1.Match<TypeCheckResult>(
            error => error,
            success =>
            {
                if (success is SupportedTypes.Numeric || success is SupportedTypes.String)
                    return success;
                else
                    return new Error<string>("TYPE_ERR: Non numeric symbol received for operand 1 in binary operation");
            }
        );
        if (resultAtom1.IsError)
        {
            return resultAtom1.AsError;
        }
        var resultAtom2 = result2.Match<TypeCheckResult>(
            error => error,
            success =>
            {
                if (success is SupportedTypes.Numeric || success is SupportedTypes.String)
                    return success;
                else
                    return new Error<string>("TYPE_ERR: Non numeric symbol received for operand 2 in binary operation");
            }
        );
        if (resultAtom2.IsError)
        {
            return resultAtom2.AsError;
        }

        if (resultAtom1.AsSuccess == SupportedTypes.Numeric && resultAtom2.AsSuccess == SupportedTypes.Numeric)
        {
            return SupportedTypes.Numeric;
        }
        if (resultAtom1.AsSuccess == SupportedTypes.String && resultAtom2.AsSuccess == SupportedTypes.String)
        {
            return SupportedTypes.String;
        }
        return new Error<string>("TYPE_ERR: Something went wrong");
    }
}
