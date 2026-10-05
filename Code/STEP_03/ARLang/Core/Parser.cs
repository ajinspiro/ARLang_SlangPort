using System.Collections.Immutable;
using OneOf;

namespace ARLang.Core;

public record ArlParseError(string Message);

[GenerateOneOf]
public partial class ArlParseResult : OneOfBase<ArlNumericExpression, ArlParseError>
{
    public bool IsResult => IsT0;
    public bool IsError => IsT1;
    public ArlNumericExpression AsResult => AsT0;
    public ArlParseError AsError => AsT1;
};

public class Parser
{
    private int index = 0;
    private ImmutableList<Token> tokens = [];

    public ArlParseResult Parse(ImmutableList<Token> tokens)
    {
        this.tokens = tokens;
        var result = ParseExpression();
        index = 0;
        return result;
    }

    private ArlParseResult ParseExpression()
    {
        ArlParseResult lhsResult = ParseTerm();
        if (lhsResult.IsError)
        {
            return lhsResult.AsError;
        }
        while (index + 1 <= tokens.Count && (tokens[index].IsTokenPlus || tokens[index].IsTokenMinus))
        {
            ArlNumericBinaryOperator operatorUnion = tokens[index].IsTokenPlus ? new Add() : new Sub();
            index++;
            ArlParseResult rhsResult = ParseTerm();
            if (rhsResult.IsError)
            {
                return rhsResult;
            }
            ArlNumericExpression binaryOperation = new ArlNumericBinaryOperation(lhsResult.AsResult, operatorUnion, rhsResult.AsResult);
            lhsResult = binaryOperation;
        }
        return lhsResult;
    }

    private ArlParseResult ParseTerm()
    {
        ArlParseResult lhsResult = ParseFactor();
        if (lhsResult.IsError)
        {
            return lhsResult.AsError;
        }
        while (index + 1 <= tokens.Count && (tokens[index].IsTokenStar || tokens[index].IsTokenSlash))
        {
            ArlNumericBinaryOperator operatorUnion = tokens[index].IsTokenStar ? new Mul() : new Div();
            index++;
            ArlParseResult rhsResult = ParseFactor();
            if (rhsResult.IsError)
            {
                return rhsResult;
            }
            ArlNumericExpression binaryOperation = new ArlNumericBinaryOperation(lhsResult.AsResult, operatorUnion, rhsResult.AsResult);
            lhsResult = binaryOperation;
        }
        return lhsResult;
    }

    private ArlParseResult ParseFactor()
    {
        if (tokens[index].IsTokenNumericConstant) // numeric expression
        {
            double value = tokens[index].AsTokenNumericConstant.Value;
            ArlNumericConstant astNode = new(value);
            ArlNumericExpression astNodeUnion = new(astNode);
            index++;
            return astNodeUnion;
        }
        if (tokens[index].IsTokenOpenParenthesis) // nested expression
        {
            index++;
            ArlParseResult nestedExpressionResult = ParseExpression();
            if (nestedExpressionResult.IsError)
            {
                return nestedExpressionResult;
            }
            if (!tokens[index].IsTokenCloseParenthesis)
            {
                return new ArlParseError("Parse error - close parenthesis missing.");
            }
            index++;
            return nestedExpressionResult;
        }
        if (tokens[index].IsTokenPlus || tokens[index].IsTokenMinus) // unary expression
        {
            ArlNumericUnaryOperator operatorUnion = tokens[index].IsTokenPlus ? new Add() : new Sub();
            index++;
            ArlParseResult nestedExpressionResult = ParseExpression();
            if (nestedExpressionResult.IsError)
            {
                return nestedExpressionResult;
            }
            ArlNumericUnaryOperation astNode = new(operatorUnion, nestedExpressionResult.AsResult);
            ArlNumericExpression astNodeUnion = new(astNode);
            return astNodeUnion;
        }
        return new ArlParseError("Something went wrong - control flow should never reach here");
    }
}