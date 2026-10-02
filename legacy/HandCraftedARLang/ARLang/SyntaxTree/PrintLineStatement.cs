namespace ARLang.SyntaxTree;

public record PrintLineStatement(ARLangExpressionBase Expression) : ARLangStatementBase;