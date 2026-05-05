using ARLang.Core;
using ARLang.Visitors.Interpreter;

// 1+2
// 5*10
// -(10 + (30+50)) = -90
// 2*(5+(3-4+5)) = 18 (not -2)
// 3-4+5 = 4 (not -6)

TestLexer("3-4+5", "2*(5+((3-4)+5))", "-(10)", "1+2", "5*10", "-(10 + (30+50))");

static void TestLexer(params string[] expressionStrings)
{
    Interpreter interpreter = new();
    foreach (var expressionString in expressionStrings)
    {
        Console.WriteLine($"Performing lexical analysis on {expressionString}");
        Lexer lexer = new(expressionString);
        var tokens = lexer.Tokenize();
        foreach (var item in tokens)
        {
            Console.WriteLine(item);
        } 
        Parser parser = new(tokens);
        var syntaxTree = parser.Parse();
        var result = interpreter.Visit(syntaxTree);
        Console.WriteLine($"Result : {result}");
        Console.WriteLine();
    }
}