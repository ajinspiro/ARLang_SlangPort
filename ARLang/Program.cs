using Antlr4.Runtime;
using ARLang;
using ARLang.Visitors.Compiler;
using ARLang.Visitors.Interpreter;

// Compiler visitor = new("DynamicTest");

VisitorManager manager = new(args[0]);
manager.Execute(args[1]);
