using ARLang.Core;
using ARLang.SyntaxTree;
using ARLang.Visitors.Interpreter;
using ARLang.Visitors.TypeChecker;

// 1+2
// 5*10
// -(10 + (30+50)) = -90
// 2*(5+(3-4+5)) = 18 (not -2)
// 3-4+5 = 4 (not -6)

// TestLexer("PRINTLINE 3-4+5; \r\n PRINT 2*(5+((3-4)+5)); \r\n PRINTLINE 2*(5+(3-4+5)); \r\n PRINT -(10); PRINTLINE 1+2; \r\n PRINT 5*10; \r\n PRINTLINE -(10 + (30+50));");
// TestLexer("PRINTLINE TRUE;");
TestLexer(
@"
NUMERIC a;  

a = 2*3+5* 30 + -(4*5+3);  

PRINTLINE a;  



PRINT ""Hello "" + ""World"";



PRINTLINE """";


STRING c;

c = ""Hello "";   



c = c + ""World"";

PRINTLINE c;


BOOLEAN d;

d= TRUE;

PRINTLINE d;

d= FALSE;

PRINTLINE d;
"
);


static void TestLexerStub(params string[] expressionStrings) { }
static void TestLexer(params string[] expressionStrings)
{
    Interpreter interpreter = new();
    foreach (var expressionString in expressionStrings)
    {
        Console.WriteLine($"Performing lexical analysis on {expressionString}");
        Lexer lexer = new(expressionString);
        var tokens = lexer.Tokenize();
        // foreach (var item in tokens)
        // {
        //     Console.WriteLine(item);
        // }
        // TypeChecker typeChecker = new();
        Parser parser = new(tokens);
        List<ARLangStatementBase> syntaxTrees = parser.Parse();
        // typeChecker.Visit(syntaxTrees); // Will throw if type checking fails and preverts execution
        interpreter.Visit(syntaxTrees);
    }
}