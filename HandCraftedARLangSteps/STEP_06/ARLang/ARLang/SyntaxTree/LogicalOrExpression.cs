namespace ARLang.SyntaxTree;

public record class LogicalOrExpression(ARLangExpressionBase Expression1, ARLangExpressionBase Expression2) : ARLangExpressionBase;