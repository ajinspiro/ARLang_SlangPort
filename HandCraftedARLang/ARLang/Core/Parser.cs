using ARLang.SyntaxTree;
using OneOf.Types;

namespace ARLang.Core;
/* 
    EBNF of expression evaluator

    Expr   ::= Term { ("+" | "-") Term }
    Term   ::= Factor { ("*" | "/") Factor }
    Factor ::= Number | "(" Expr ")" | ("+" | "-") Factor
  */
public class Parser(IList<Token> tokens)
{
    private readonly IList<Token> tokens = tokens;
    private int index = 0;
    private readonly SymbolInfoTable SymbolInfoTable = new();

    public List<ARLangStatementBase> Parse()
    {
        var result = ParseStatementList();
        index = 0; // reset parser instance
        return result;
    }

    private List<ARLangStatementBase> ParseStatementList()
    {
        List<ARLangStatementBase> statements = [];
        List<TokenType> tokensToExitOn = [TokenType.ELSE, TokenType.ENDIF, TokenType.WEND, TokenType.END_OF_STRING];
        while (!tokensToExitOn.Contains(tokens[index].Type))
        {
            var statement = ParseStatement();
            if (statement is ErrorStatement errorStatement)
            {
                Console.Error.WriteLine($"Error in statement: {errorStatement.Msg}");
                return [];
            }
            statements.Add(statement);
        }
        return statements;
    }

    private ARLangStatementBase ParseStatement()
    {
        return tokens[index] switch
        {
            { Type: TokenType.PRINTLN } => ParsePrintLineStatement(),
            { Type: TokenType.PRINT } => ParsePrintStatement(),
            { Type: TokenType.VARIABLE_STRING or TokenType.VARIABLE_NUMERIC or TokenType.VARIABLE_BOOL } => ParseVariableDeclareStatement(),
            { Type: TokenType.UNQUOTED_STRING } => ParseAssignmentStatement(),
            { Type: TokenType.IF } => ParseIfStatement(),
            { Type: TokenType.WHILE } => ParseWhileStatement(),
            { Type: TokenType.FUNCTION } => ParseFunctionDefinition(), // TODO: introduce a declaration layer
            { Type: TokenType.ILLEGAL_TOKEN } => new ErrorStatement("Illegal token encountered."),
            _ => throw new Exception()
        };
    }

    private ARLangStatementBase ParseFunctionDefinition()
    {
        throw new NotImplementedException();
    }

    private ARLangStatementBase ParseWhileStatement()
    {
        index++;
        ARLangExpressionBase logicalExpression = ParseLogicalExpression();
        List<ARLangStatementBase> loopBody = ParseStatementList();
        if (tokens[index].Type != TokenType.WEND)
        {
            return new ErrorStatement("PARSER: WEND keyword missing.");
        }
        return new WhileStatement(logicalExpression, loopBody);
    }

    private ARLangStatementBase ParseIfStatement()
    {
        index++;
        if (TokenType.OPEN_PARENTHESIS != tokens[index].Type)
        {
            return new ErrorStatement("IF_STATEMENT : Open parenthesis missing.");
        }
        ARLangExpressionBase condition = ParseLogicalExpression();
        if (TokenType.THEN != tokens[index].Type)
        {
            return new ErrorStatement("IF_STATEMENT : THEN keyword missing.");
        }
        index++;
        List<ARLangStatementBase> thenBranchStatements = ParseStatementList();
        if (TokenType.ENDIF == tokens[index].Type)
        {
            return new IfStatement(condition, thenBranchStatements);
        }
        if (TokenType.ELSE != tokens[index].Type)
        {
            return new ErrorStatement("IF_STATEMENT : ELSE keyword missing.");
        }
        index++;
        List<ARLangStatementBase> elseBranchStatements = ParseStatementList();
        if (TokenType.ENDIF != tokens[index].Type)
        {
            return new ErrorStatement("IF_STATEMENT : ENDIF keyword missing.");
        }
        index++;
        return new IfStatement(condition, thenBranchStatements, elseBranchStatements);
    }

    private ARLangStatementBase ParseAssignmentStatement()
    {
        Token variableName = tokens[index];
        if (variableName.Value is null)
        {
            return new ErrorStatement("Symbol name was null.");
        }
        var union = SymbolInfoTable.Get(variableName.Value);
        if (union.IsT0)
        {
            return new ErrorStatement("Undeclared variable was used.");
        }
        index++; // move to = operator
        if (tokens[index].Type != TokenType.ASSIGN)
        {
            return new ErrorStatement("Assignment operator expected.");
        }
        index++; // move to the expression
        var expression = ParseExpression();
        if (tokens[index++].Type != TokenType.SEMICOLON)
        {
            return new ErrorStatement("Semicolon missing.");
        }
        return new AssignmentStatement(union.AsT1, expression);
    }

    private ARLangStatementBase ParseVariableDeclareStatement()
    {
        Token variableType = tokens[index];
        index++;
        if (tokens[index].Type != TokenType.UNQUOTED_STRING)
        {
            return new ErrorStatement("Syntax error: datatype must be followed by a variable name.");
        }
        Token variableName = tokens[index];
        index++;
        if (tokens[index].Type != TokenType.SEMICOLON)
        {
            return new ErrorStatement($"Semicolon expexted after a statement.");
        }
        // Adding variable to symbol table
        SymbolInfo symbolInfo = new(variableType.Type, new None(), variableName.Value);
        bool isSuccess = SymbolInfoTable.TryAdd(symbolInfo);
        if (isSuccess == false)
        {
            return new ErrorStatement($"Failed to store the variable '{variableName.Value}' in symbol table.");
        }
        if (tokens[index++].Type != TokenType.SEMICOLON)
        {
            return new ErrorStatement("Semicolon missing.");
        }
        return new VariableDeclareStatement(symbolInfo);
    }

    private ARLangStatementBase ParsePrintStatement()
    {
        index++;
        ARLangExpressionBase expression = ParseExpression();
        if (tokens[index].Type != TokenType.SEMICOLON)
        {
            return new ErrorStatement("Semicolon missing.");
        }
        index++;
        return new PrintStatement(expression);
    }

    private ARLangStatementBase ParsePrintLineStatement()
    {
        index++;
        ARLangExpressionBase expression = ParseExpression();
        if (tokens[index].Type != TokenType.SEMICOLON)
        {
            return new ErrorStatement("Semicolon missing.");
        }
        index++;
        return new PrintLineStatement(expression);
    }

    private ARLangExpressionBase ParseLogicalExpression()
    {
        TokenType tokenType;
        ARLangExpressionBase leftExp = ParseRelationalExpression();
        List<TokenType> relationalOperators = [TokenType.AND, TokenType.OR];
        while (relationalOperators.Contains(tokens[index].Type))
        {
            tokenType = tokens[index].Type;
            index++;
            ARLangExpressionBase rightExp = ParseRelationalExpression();
            return tokenType switch
            {
                TokenType.AND => new LogicalAndExpression(leftExp, rightExp),
                TokenType.OR => new LogicalOrExpression(leftExp, rightExp),
                _ => new ErrorExpression("PARSER: Invalid relational operation")
            };
        }
        return leftExp;
    }

    private ARLangExpressionBase ParseRelationalExpression()
    {
        TokenType tokenType;
        ARLangExpressionBase leftExp = ParseExpression();
        List<TokenType> relationalOperators = [TokenType.GT, TokenType.LT, TokenType.GTE, TokenType.LTE, TokenType.NEQ, TokenType.EQ];
        while (relationalOperators.Contains(tokens[index].Type))
        {
            tokenType = tokens[index].Type;
            index++;
            ARLangExpressionBase rightExp = ParseExpression();
            return tokenType switch
            {
                TokenType.GT => new RelationalGtExpression(leftExp, rightExp),
                TokenType.GTE => new RelationalGteExpression(leftExp, rightExp),
                TokenType.LT => new RelationalLtExpression(leftExp, rightExp),
                TokenType.LTE => new RelationalLteExpression(leftExp, rightExp),
                TokenType.EQ => new RelationalEqExpression(leftExp, rightExp),
                TokenType.NEQ => new RelationalNeqExpression(leftExp, rightExp),
                _ => new ErrorExpression("PARSER: Invalid relational operation")
            };
        }
        return leftExp;
    }

    private ARLangExpressionBase ParseExpression()
    {
        ARLangExpressionBase leftExp = ParseTerm();
        while (tokens[index].Type == TokenType.PLUS || tokens[index].Type == TokenType.MINUS)
        {
            Token operatorBackup = tokens[index];
            index++;
            ARLangExpressionBase rightExp = ParseTerm();
            leftExp = operatorBackup.Type == TokenType.PLUS ? new AdditionExpression(leftExp, rightExp) : new SubtractionExpression(leftExp, rightExp);
        }
        return leftExp;
    }

    private ARLangExpressionBase ParseTerm()
    {
        ARLangExpressionBase leftExp = ParseFactor();
        while (tokens[index].Type == TokenType.STAR || tokens[index].Type == TokenType.SLASH)
        {
            Token operatorBackup = tokens[index];
            index++;
            ARLangExpressionBase rightExp = ParseFactor();
            leftExp = operatorBackup.Type == TokenType.STAR ? new MultiplicationExpression(leftExp, rightExp) : new DivisionExpression(leftExp, rightExp);
        }
        return leftExp;
    }

    private ARLangExpressionBase ParseFactor()
    {
        if (tokens[index].Type == TokenType.NUMBER)
        {
            // Value will be Number. So lets parse value directly. Its safe.
            return new NumericConstantExpression(double.Parse(tokens[index++].Value));
        }
        if (tokens[index].Type == TokenType.STRING)
        {
            // Extracting number from union 
            return new StringLiteralExpression(tokens[index++].Value);
        }
        if (tokens[index].Type == TokenType.BOOL_TRUE || tokens[index].Type == TokenType.BOOL_FALSE)
        {
            return new BooleanConstantExpression(tokens[index++].Type == TokenType.BOOL_TRUE ? true : false);
        }
        if (tokens[index].Type == TokenType.OPEN_PARENTHESIS)
        {
            // Nested expression
            index++;
            ARLangExpressionBase returnValue = ParseLogicalExpression();
            if (tokens[index].Type != TokenType.CLOSE_PARENTHESIS)
            {
                return new ErrorExpression("Invalid expression: Missing close parenthesis");
            }
            index++;
            return returnValue;
        }
        if (tokens[index].Type == TokenType.PLUS)
        {
            index++;
            // Unary plus expression
            ARLangExpressionBase factor = ParseFactor();
            return new UnaryPlusExpression(factor);
        }
        if (tokens[index].Type == TokenType.MINUS)
        {
            index++;
            // Unary minus expression
            ARLangExpressionBase factor = ParseFactor();
            return new UnaryMinusExpression(factor);
        }
        if (tokens[index].Type == TokenType.NOT)
        {
            index++;
            ARLangExpressionBase factor = ParseFactor();
            return new LogicalNotExpression(factor);
        }
        if (tokens[index].Type == TokenType.UNQUOTED_STRING)
        {
            var union = SymbolInfoTable.Get(tokens[index++].Value); //Supressing null because lexer will set symbol name for unquoted strings. Its safe.
            return union.Match<ARLangExpressionBase>(
                none => new ErrorExpression("Variable not found"),
                symbolInfo => new VariableExpression(symbolInfo)
                );
        }
        return new ErrorExpression("Illegal token");
    }
}