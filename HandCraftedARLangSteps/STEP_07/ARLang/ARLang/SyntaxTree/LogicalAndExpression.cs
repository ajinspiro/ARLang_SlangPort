namespace ARLang.SyntaxTree;

public record LogicalAndExpression(ARLangExpressionBase Expression1, ARLangExpressionBase Expression2) : ARLangExpressionBase;