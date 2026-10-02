namespace ARLang.SyntaxTree;

/// <summary>
/// Syntax tree node for representing number. Always a termianl node.
/// </summary>
/// <param name="Value"></param>
public record NumericConstantExpression(double Value) : ARLangExpressionBase;