using Antlr4.Runtime;
using ARLang.Internals;
using ARLang.Visitors;

ICharStream input = args.Length == 1 ? CharStreams.fromString(await File.ReadAllTextAsync(args[0])) : CharStreams.fromStream(Console.OpenStandardInput());
ARLangLexer lexer = new(input);
CommonTokenStream tokens = new(lexer);
ARLangParser parser = new(tokens);
RuntimeContext runtimeContext = new();
var module = parser.module();
Compiler visitor = new(runtimeContext, "DynamicTest");
var value = visitor.Visit(module);
Console.WriteLine("============RESULTS===========");
string valueToPrint = value.Match(
    error => "Error",
    success => "Success (no value)",
    successWithValue => "Success with value: {successWithValue.Match(d => d.ToString(), s => s, b => b.ToString(), n => '<none>')}"
);
Console.WriteLine(valueToPrint);
