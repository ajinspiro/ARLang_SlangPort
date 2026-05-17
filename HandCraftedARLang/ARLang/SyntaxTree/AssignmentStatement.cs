using ARLang.Core;

namespace ARLang.SyntaxTree;

public record AssignmentStatement(VariableExpression Variable, ARLangExpressionBase Expression) : ARLangStatementBase;