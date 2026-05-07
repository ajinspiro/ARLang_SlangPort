namespace ARLang.Core;

public record RuntimeContext
{
    public SymbolInfoTable SymbolInfoTable { get; set; } = new();
}
