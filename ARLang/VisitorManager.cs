using Antlr4.Runtime;
using ARLang.Visitors.Interpreter;
using OneOf;
using OneOf.Types;

namespace ARLang;

public class VisitorManager
{
    private readonly string mode;

    public VisitorManager(string mode)
    {
        if (!(mode == "--compile" || mode == "--execute"))
        {
            throw new InvalidOperationException();
        }
        this.mode = mode;
    }

    public void Execute(string slangScriptPath)
    {
        if (slangScriptPath.EndsWith(".sl"))
        {
            ExecutePrivate(slangScriptPath);
        }
        else
        {
            foreach (var path in Directory.EnumerateFiles(slangScriptPath))
            {
                ExecutePrivate(path);
            }
        }
    }

    private static void ExecutePrivate(string slangScriptPath)
    {
        System.Console.WriteLine($"====Executing {slangScriptPath}====");
        string scriptContent = File.ReadAllText(slangScriptPath);
        var module = GetModule(scriptContent);
        Interpreter visitor = new();
        var value = visitor.Visit(module);
        Console.WriteLine("============RESULTS===========");
        string valueToPrint = value.Match(
            error => "Error",
            success => "Success (no value)",
            successWithValue => $"Success with value: {successWithValue.Match(d => d.ToString(), s => s, b => b.ToString(), n => "<none>")} ",
            error => "Error",
            error => "Error",
            error => "Error",
            error => "Error",
            error => "Error"
        );
        Console.WriteLine(valueToPrint);
    }

    private static ARLangParser.ModuleContext GetModule(string scriptContent)
    {
        ICharStream input = CharStreams.fromString(scriptContent);
        ARLangLexer lexer = new(input);
        CommonTokenStream tokens = new(lexer);
        ARLangParser parser = new(tokens);
        return parser.module();
    }
}