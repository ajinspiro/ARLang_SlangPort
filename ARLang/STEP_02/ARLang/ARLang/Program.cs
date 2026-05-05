using ARLang.Core;
using ARLang.SyntaxTree;
using ARLang.Visitors;
using ARLang.Visitors.Interpreter;

// 1+2
// 5*10
// -(10 + (30+50)) = -90

TestLexer("1+2", "5*10", "-(10 + (30+50))");
static void TestLexer(params string[] expressionStrings)
{
    foreach (var expressionString in expressionStrings)
    {
        Console.WriteLine($"Performing lexical analysis on {expressionString}");
        Lexer lexer = new(expressionString);
        SymbolInfo symbol;
        do
        {
            symbol = lexer.GetToken();
            Console.WriteLine(symbol);
        } while (symbol.TokenType != TokenType.END_OF_STRING);
        Console.WriteLine();
    }
}

static void EvaluateExpression(params ARLangExpressionBase[] expressions)
{
    IVisitorBase interpreter = new Interpreter();
    foreach (var expression in expressions)
    {
        var result = interpreter.Visit(expression);
        Console.WriteLine(result);
    }
}
