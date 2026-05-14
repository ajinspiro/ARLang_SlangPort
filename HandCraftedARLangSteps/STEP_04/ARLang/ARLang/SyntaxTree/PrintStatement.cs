namespace ARLang.SyntaxTree;

public record PrintStatement(ARLangExpressionBase Expression) : ARLangStatementBase;