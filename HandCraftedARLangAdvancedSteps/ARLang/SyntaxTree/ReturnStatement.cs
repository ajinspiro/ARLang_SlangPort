namespace ARLang.SyntaxTree;

public record ReturnStatement(ARLangExpressionBase Expression) : ARLangStatementBase;