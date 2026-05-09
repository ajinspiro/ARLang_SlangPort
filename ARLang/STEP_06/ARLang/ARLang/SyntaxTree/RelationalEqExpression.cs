namespace ARLang.SyntaxTree;

public record RelationalEqExpression(ARLangExpressionBase Expression1, ARLangExpressionBase Expression2) : ARLangExpressionBase;