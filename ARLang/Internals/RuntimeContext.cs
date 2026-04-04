using System;

namespace ARLang.Internals;

public class RuntimeContext
{
    public Scope Scope { get; set; } = new("root");
    public List<FunctionDef> FunctionDefinitions { get; set; } = [];
}
