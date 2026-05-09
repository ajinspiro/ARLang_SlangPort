namespace ARLang.SyntaxTree;

public record RelationalGtExpression(ARLangExpressionBase Expression1, ARLangExpressionBase Expression2) : ARLangExpressionBase;