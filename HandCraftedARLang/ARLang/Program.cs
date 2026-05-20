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
TestLexerStub(
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
TestLexerStub(@"
Numeric c;
c = 2;
if ( !(c == 20  || c == 2) ) then
   PRINTLINE ""Hello World"";
   PRINTLINE ""Hello World"";
   PRINTLINE ""Hello World"";
else
    PRINTLINE ""ELSE part"";
endif
");
TestLexerStub(@"
NUMERIC I;
I = 0;
WHILE ( I <= 10 )
  PRINTLINE I;
  I = I + 1;
WEND
");
TestLexerStub(
@"
Boolean b;
b=!FALSE;
PRINTLINE b;
"
);
TestLexer(@"
FUNCTION NUMERIC Quad( NUMERIC a , NUMERIC b , NUMERIC c )
   NUMERIC n;
   n = b*b - 4*a*c;
   IF ( n < 0 ) THEN
        RETURN 0;
   ELSE 
     IF ( n == 0 ) THEN
         RETURN 1;
     ELSE
         RETURN 2;
     ENDIF
   ENDIF 
   RETURN 0;
END
FUNCTION BOOLEAN MAIN()
   NUMERIC d;
   d= Quad(1,0-5,6);

   IF ( d == 0 ) then
         PRINT ""No Roots"";
   ELSE
       IF ( d  == 1 ) then
         PRINT  ""Discriminant is zero"";
       ELSE
         PRINT  ""Two roots are available"";
       ENDIF
   ENDIF
   RETURN FALSE;
END
");
TestLexerStub(@"
BOOLEAN a;
BOOLEAN b;
BOOLEAN c;
BOOLEAN sum;
a = TRUE;
b = FALSE;
c = TRUE;
sum = a && b && c;
PRINTLINE sum;
");
TestLexerStub(@"
WHILE(a < 10)
    PRINTLINE a;
    a = a + 1;
WEND
PRINTLINE a;
");
TestLexerStub(@"
IF (a < b || b < f) THEN
    PRINTLINE ""+"";
ELSE 
    PRINTLINE ""-"";
ENDIF
PRINTLINE a;
");
TestLexerStub(@"
FUNCTION NUMERIC Main2()  
    return 124+1; 
END

FUNCTION NUMERIC Main()  
    NUMERIC a;
    a = Main2();
    PRINTLINE a;
END
");
static void TestLexerStub(params string[] expressionStrings) { }
static void TestLexer(params string[] expressionStrings)
{
    Lexer lexer = new();
    Interpreter interpreter = new();
    foreach (var expressionString in expressionStrings)
    {
        Console.WriteLine($"Performing lexical analysis on {expressionString}");
        var tokens = lexer.Tokenize(expressionString);
        foreach (var item in tokens)
        {
            Console.WriteLine(item);
        }
        // TypeChecker typeChecker = new();
        Parser parser = new(tokens);
        List<ARLangDefinitionBase> syntaxTree = parser.Parse();
        if (syntaxTree.Count == 1 && syntaxTree.First() is ErrorDefinition errorDefinition)
        {
            Console.Error.WriteLine(errorDefinition.Msg);
            return;
        }
        // typeChecker.Visit(syntaxTrees); // Will throw if type checking fails and preverts execution
        interpreter.Visit(syntaxTree);
    }
}