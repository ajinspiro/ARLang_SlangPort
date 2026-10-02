using ARLang.Core;

namespace ARLang.SyntaxTree;

public record FunctionDefinition(string Name, DataType ReturnType, List<FunctionParameter> Parameters, List<ARLangStatementBase> Body) : ARLangDefinitionBase;

public record FunctionParameter(string Name, DataType DataType);