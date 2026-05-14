namespace ARLang.SyntaxTree;

/// <summary>
/// Syntax tree node for representing a unary plus of an expression. Always a non-terminal node.
/// </summary>
/// <param name="Expression"></param>
public record UnaryPlusExpression(ARLangExpressionBase Expression) : ARLangExpressionBase;