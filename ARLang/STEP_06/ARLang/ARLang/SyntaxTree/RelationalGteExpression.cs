namespace ARLang.SyntaxTree;

public record RelationalGteExpression(ARLangExpressionBase Expression1, ARLangExpressionBase Expression2) : RelationalExpressionBase;