namespace ARLang.SyntaxTree;

/// <summary>
/// Syntax tree node for representing a unary minus of an expression. Always a non-terminal node.
/// </summary>
/// <param name="Expression"></param>
public record UnaryMinusExpression(ARLangExpressionBase Expression) : ARLangExpressionBase;