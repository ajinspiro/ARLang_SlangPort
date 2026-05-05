using ARLang.Core;

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
        var tokens = lexer.Tokenize();
        foreach (var item in tokens)
        {
            Console.WriteLine(item);
        }
        Console.WriteLine();
    }
}