namespace ARLang.Visitors.Interpreter;

public class RuntimeContext
{
    public Scope Scope { get; set; } = new("root");
    public List<FunctionDef> FunctionDefinitions { get; set; } = [];
}

public record FunctionDef(string Name,
                          Dictionary<string, string> Params,
                          ARLangParser.StatementsContext Body,
                          string ReturnType
                          );