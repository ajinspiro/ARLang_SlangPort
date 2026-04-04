namespace ARLang.Internals;

public record FunctionDef(string Name,
                          Dictionary<string, string> Params,
                          ARLangParser.StatementsContext Body,
                          string ReturnType
                          );
