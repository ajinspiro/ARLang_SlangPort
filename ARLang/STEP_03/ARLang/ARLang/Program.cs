using ARLang.Core;
using ARLang.SyntaxTree;
using ARLang.Visitors.Interpreter;

// 1+2
// 5*10
// -(10 + (30+50)) = -90
// 2*(5+(3-4+5)) = 18 (not -2)
// 3-4+5 = 4 (not -6)

TestLexer("PRINTLN 3-4+5; \r\n PRINT 2*(5+((3-4)+5)); \r\n PRINTLN 2*(5+(3-4+5)); \r\n PRINT -(10); PRINTLN 1+2; \r\n PRINT 5*10; \r\n PRINTLN -(10 + (30+50));");

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
        List<ARLangStatementBase> syntaxTrees = parser.Parse();
        interpreter.Visit(syntaxTrees);
    }
}