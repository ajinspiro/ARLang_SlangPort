namespace ARLang.SyntaxTree;

public record FunctionCallExpression(string Name, List<ARLangExpressionBase> Arguments) : ARLangExpressionBase;