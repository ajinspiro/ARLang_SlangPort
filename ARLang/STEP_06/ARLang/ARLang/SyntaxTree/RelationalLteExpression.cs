namespace ARLang.SyntaxTree;

public record RelationalLteExpression(ARLangExpressionBase Expression1, ARLangExpressionBase Expression2) : ARLangExpressionBase;