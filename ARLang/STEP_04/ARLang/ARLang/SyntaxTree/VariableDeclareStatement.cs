using ARLang.Core;

namespace ARLang.SyntaxTree;

public record VariableDeclareStatement(SymbolInfo SymbolInfo) : ARLangStatementBase;
