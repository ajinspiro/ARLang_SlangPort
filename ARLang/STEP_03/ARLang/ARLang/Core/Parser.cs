using ARLang.SyntaxTree;

namespace ARLang.Core;
/* 
    EBNF of expression evaluator

    Expr   ::= Term { ("+" | "-") Term }
    Term   ::= Factor { ("*" | "/") Factor }
    Factor ::= Number | "(" Expr ")" | ("+" | "-") Factor
  */
public class Parser(IList<SymbolInfo> tokens)
{
    private readonly IList<SymbolInfo> tokens = tokens;
    private int index = 0;

    public ARLangExpressionBase Parse()
    {
        var result = ParseExpression();
        index = 0; // reset parser instance
        return result;
    }

    private ARLangExpressionBase ParseExpression()
    {
        ARLangExpressionBase leftExp = ParseTerm();
        while (tokens[index].TokenType == TokenType.PLUS || tokens[index].TokenType == TokenType.MINUS)
        {
            SymbolInfo operatorBackup = tokens[index];
            index++;
            ARLangExpressionBase rightExp = ParseTerm();
            leftExp = operatorBackup.TokenType == TokenType.PLUS ? new AdditionExpression(leftExp, rightExp) : new SubtractionExpression(leftExp, rightExp);
        }
        return leftExp;
    }

    private ARLangExpressionBase ParseTerm()
    {
        ARLangExpressionBase leftExp = ParseFactor();
        while (tokens[index].TokenType == TokenType.STAR || tokens[index].TokenType == TokenType.SLASH)
        {
            SymbolInfo operatorBackup = tokens[index];
            index++;
            ARLangExpressionBase rightExp = ParseFactor();
            leftExp = operatorBackup.TokenType == TokenType.STAR ? new MultiplicationExpression(leftExp, rightExp) : new DivisionExpression(leftExp, rightExp);
        }
        return leftExp;
    }

    private ARLangExpressionBase ParseFactor()
    {
        if (tokens[index].TokenType == TokenType.NUMBER)
        {
            // Extracting number from union
            return tokens[index++].Value.Match<ARLangExpressionBase>(
                error => new ErrorExpression("Parsing error: Expecting a number, but got <None>."),
                number => new NumericConstantExpression(number)
            );
        }
        if (tokens[index].TokenType == TokenType.OPEN_PARENTHESIS)
        {
            // Nested expression
            index++;
            ARLangExpressionBase returnValue = ParseExpression();
            if (tokens[index].TokenType != TokenType.CLOSE_PARENTHESIS)
            {
                return new ErrorExpression("Invalid expression: Missing close parenthesis");
            }
            index++;
            return returnValue;
        }
        if (tokens[index].TokenType == TokenType.PLUS)
        {
            index++;
            // Unary plus expression
            ARLangExpressionBase factor = ParseFactor();
            return new UnaryPlusExpression(factor);
        }
        if (tokens[index].TokenType == TokenType.MINUS)
        {
            index++;
            // Unary minus expression
            ARLangExpressionBase factor = ParseFactor();
            return new UnaryMinusExpression(factor);
        }
        return new ErrorExpression("Illegal token");
    }
}