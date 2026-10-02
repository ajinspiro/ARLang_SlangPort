namespace ARLang.SyntaxTree;

public record RelationalLtExpression(ARLangExpressionBase Expression1, ARLangExpressionBase Expression2) : RelationalExpressionBase;