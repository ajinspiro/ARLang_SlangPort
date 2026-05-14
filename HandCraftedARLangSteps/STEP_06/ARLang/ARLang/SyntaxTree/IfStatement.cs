namespace ARLang.SyntaxTree;

public record IfStatement(ARLangExpressionBase Condition, List<ARLangStatementBase> ThenBranch, List<ARLangStatementBase>? ElseBranch = null) : ARLangStatementBase;