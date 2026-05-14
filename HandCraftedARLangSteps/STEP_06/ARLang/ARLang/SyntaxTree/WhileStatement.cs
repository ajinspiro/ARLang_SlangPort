namespace ARLang.SyntaxTree;

public record WhileStatement(ARLangExpressionBase Condition, List<ARLangStatementBase> Body) : ARLangStatementBase;