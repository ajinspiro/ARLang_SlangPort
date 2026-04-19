using Antlr4.Runtime;
using ARLang.Visitors.Compiler;
using ARLang.Visitors.Interpreter;

ICharStream input = CharStreams.fromString(await File.ReadAllTextAsync(args[0]));
ARLangLexer lexer = new(input);
CommonTokenStream tokens = new(lexer);
ARLangParser parser = new(tokens);
RuntimeContext runtimeContext = new();
var module = parser.module();
Interpreter visitor = new(runtimeContext);
// Compiler visitor = new("DynamicTest");
var value = visitor.Visit(module);
Console.WriteLine("============RESULTS===========");
string valueToPrint = value.Match(
    error => "Error",
    success => "Success (no value)",
    successWithValue => "Success with value: {successWithValue.Match(d => d.ToString(), s => s, b => b.ToString(), n => \"<none>\")}",
    error => "Error",
    error => "Error",
    error => "Error",
    error => "Error",
    error => "Error"
);
Console.WriteLine(valueToPrint);
