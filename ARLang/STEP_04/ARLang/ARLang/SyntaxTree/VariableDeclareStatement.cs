namespace ARLang.SyntaxTree;

public record VariableDeclareStatement(VariableExpression Expression) : ARLangStatementBase;
