using ARLang.SyntaxTree;
using ARLang.Visitors.Interpreter;
using OneOf.Types;

namespace ARLang.Visitors.TypeChecker;

public class TypeChecker : IVisitorBase
{
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
        else
        {
            return new Error<string>("Invalid statement.");
        }
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
            NumericConstantExpression e => SupportedTypes.Numeric,
            _ => new Error<string>("Invalid expression")
        };
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
                    return new Error<string>("Non numeric symbol received for unary operation");
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
                if (success == SupportedTypes.Numeric)
                    return SupportedTypes.Numeric;
                else
                    return new Error<string>("Non numeric symbol received for operand 1 in division operation");
            }
        );
        if (resultAtom1.IsError)
        {
            return resultAtom1.AsError;
        }
        var resultAtom2 = result1.Match<TypeCheckResult>(
            error => error,
            success =>
            {
                if (success == SupportedTypes.Numeric)
                    return SupportedTypes.Numeric;
                else
                    return new Error<string>("Non numeric symbol received for operand 2 in division operation");
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
        return new Error<string>("Something went wrong");
    }
}
