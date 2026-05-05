using ARLang.SyntaxTree;

namespace ARLang.Core;
/* 
    EBNF of expression evaluator
    <Expr> ::= <Term> | Term { + | - } <Expr>
    <Term> ::= <Factor> | <Factor> {*|/} <Term>
    <Factor>::= <number> | ( <expr> ) | {+|-} <factor>
  */
public class Parser(IList<SymbolInfo> tokens)
{
    private readonly IList<SymbolInfo> tokens = tokens;
    private int index = 0;

    public ARLangExpressionBase Parse()
    {
        return ParseExpression();
    }

    private ARLangExpressionBase ParseExpression()
    {
        ARLangExpressionBase returnValue = ParseTerm();
        while (tokens[index].TokenType == TokenType.PLUS || tokens[index].TokenType == TokenType.MINUS)
        {
            SymbolInfo operatorBackup = tokens[index];
            index++;
            ARLangExpressionBase expression = ParseExpression();
            returnValue = operatorBackup.TokenType == TokenType.PLUS ? new AdditionExpression(returnValue, expression) : new SubtractionExpression(returnValue, expression);
        }
        return returnValue;
    }

    private ARLangExpressionBase ParseTerm()
    {
        ARLangExpressionBase returnValue = ParseFactor();
        while (tokens[index].TokenType == TokenType.STAR || tokens[index].TokenType == TokenType.SLASH)
        {
            SymbolInfo operatorBackup = tokens[index];
            index++;
            ARLangExpressionBase term = ParseTerm();
            returnValue = operatorBackup.TokenType == TokenType.STAR ? new MultiplicationExpression(returnValue, term) : new DivisionExpression(returnValue, term);
        }
        return returnValue;
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