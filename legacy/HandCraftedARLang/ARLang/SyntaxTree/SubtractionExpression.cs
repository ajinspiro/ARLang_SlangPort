namespace ARLang.SyntaxTree;

/// <summary>
/// Syntax tree node for representing a binary subtraction of two expressions. Always a non-terminal node.
/// </summary>
/// <param name="Expression1"></param>
/// <param name="Expression2"></param>
public record SubtractionExpression(ARLangExpressionBase Expression1, ARLangExpressionBase Expression2) : ARLangExpressionBase;