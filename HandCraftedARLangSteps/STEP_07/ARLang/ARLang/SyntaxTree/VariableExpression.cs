using ARLang.Core;

namespace ARLang.SyntaxTree;

public record VariableExpression(SymbolInfo SymbolInfo) : ARLangExpressionBase;