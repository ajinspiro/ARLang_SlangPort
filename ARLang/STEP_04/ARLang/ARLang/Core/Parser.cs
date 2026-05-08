using ARLang.SyntaxTree;
using OneOf.Types;

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
    private readonly RuntimeContext context = new();
    public List<ARLangStatementBase> Parse()
    {
        var result = ParseStatementList();
        index = 0; // reset parser instance
        return result;
    }

    private List<ARLangStatementBase> ParseStatementList()
    {
        List<ARLangStatementBase> statements = [];
        while (tokens[index].TokenType != TokenType.END_OF_STRING)
        {
            var statement = ParseStatement();
            if (statement is ErrorStatement)
            {
                Console.Error.WriteLine("Error: Illegal token/statement encountered.");
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
            { TokenType: TokenType.PRINTLN } => ParsePrintLineStatement(),
            { TokenType: TokenType.PRINT } => ParsePrintStatement(),
            { TokenType: TokenType.VARIABLE_STRING or TokenType.VARIABLE_NUMBER or TokenType.VARIABLE_BOOL } => ParseVariableDeclareStatement(),
            { TokenType: TokenType.UNQUOTED_STRING } => ParseAssignmentStatement(),
            { TokenType: TokenType.ILLEGAL_TOKEN } => new ErrorStatement("Illegal token encountered."),
            _ => throw new Exception()
        };
    }

    private ARLangStatementBase ParseAssignmentStatement()
    {
        SymbolInfo variableName = tokens[index];
        if (variableName.SymbolName is null)
        {
            return new ErrorStatement("Symbol name was null.");
        }
        var union = context.SymbolInfoTable.Get(variableName.SymbolName);
        if (union.IsT0)
        {
            return new ErrorStatement("Undeclared variable was used.");
        }
        index++; // move to = operator
        if (tokens[index].TokenType != TokenType.ASSIGN)
        {
            return new ErrorStatement("Assignment operator expected.");
        }
        index++; // move to the expression
        var expression = ParseExpression();
        if (tokens[index++].TokenType != TokenType.SEMICOLON)
        {
            return new ErrorStatement("Semicolon missing.");
        }
        return new AssignmentStatement(union.AsT1, expression);
    }

    private ARLangStatementBase ParseVariableDeclareStatement()
    {
        SymbolInfo variableType = tokens[index];
        index++;
        if (tokens[index].TokenType != TokenType.UNQUOTED_STRING)
        {
            return new ErrorStatement("Syntax error: datatype must be followed by a variable name.");
        }
        SymbolInfo variableName = tokens[index];
        index++;
        if (tokens[index].TokenType != TokenType.SEMICOLON)
        {
            return new ErrorStatement($"Semicolon expexted after a statement.");
        }
        // Adding variable to symbol table
        SymbolInfo symbolInfo = new(variableType.TokenType, new None(), variableName.SymbolName);
        bool isSuccess = context.SymbolInfoTable.TryAdd(symbolInfo);
        if (isSuccess == false)
        {
            return new ErrorStatement($"Failed to store the variable '{variableName.SymbolName}' in symbol table.");
        }
        if (tokens[index++].TokenType != TokenType.SEMICOLON)
        {
            return new ErrorStatement("Semicolon missing.");
        }
        return new VariableDeclareStatement(symbolInfo);
    }

    private ARLangStatementBase ParsePrintStatement()
    {
        index++;
        ARLangExpressionBase expression = ParseExpression();
        if (tokens[index].TokenType != TokenType.SEMICOLON)
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
        if (tokens[index].TokenType != TokenType.SEMICOLON)
        {
            return new ErrorStatement("Semicolon missing.");
        }
        index++;
        return new PrintLineStatement(expression);
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
            // Value will be Number. So lets use AsT1 property directly. Its safe.
            return new NumericConstantExpression(tokens[index++].Value.AsT1);
        }
        if (tokens[index].TokenType == TokenType.STRING)
        {
            // Extracting number from union
            // Value will be Number. So lets use AsT2 property directly. Its safe.
            return new StringLiteralExpression(tokens[index++].Value.AsT2);
        }
        if (tokens[index].TokenType == TokenType.BOOL_TRUE || tokens[index].TokenType == TokenType.BOOL_FALSE)
        {
            return new BooleanConstantExpression(tokens[index++].TokenType == TokenType.BOOL_TRUE ? true : false);
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
        if (tokens[index].TokenType == TokenType.UNQUOTED_STRING)
        {
            var union = context.SymbolInfoTable.Get(tokens[index++].SymbolName!); //Supressing null because lexer will set symbol name for unquoted strings. Its safe.
            return union.Match<ARLangExpressionBase>(
                none => new ErrorExpression("Variable not found"),
                symbolInfo => new VariableExpression(symbolInfo)
                );
        }
        return new ErrorExpression("Illegal token");
    }
}