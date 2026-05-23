using ARLang.Core;

namespace ARLang.SyntaxTree;

public record VariableDeclareStatement(DataType DataType, string Name) : ARLangStatementBase;
