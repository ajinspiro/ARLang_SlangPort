using ARLang.Core;

namespace ARLang.SyntaxTree;

public record AssignmentStatement(SymbolInfo SymbolInfo, ARLangExpressionBase Expression) : ARLangStatementBase;