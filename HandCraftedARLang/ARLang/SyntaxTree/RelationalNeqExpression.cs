namespace ARLang.SyntaxTree;

public record RelationalNeqExpression(ARLangExpressionBase Expression1, ARLangExpressionBase Expression2) : RelationalExpressionBase;