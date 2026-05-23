using ARLang.Core;
using OneOf;

namespace ARLang.Visitors.Interpreter;

[GenerateOneOf]
public partial class ARLangValue : OneOfBase<double, string, bool>;

public record RuntimeVariable(DataType DataType, string SymbolName, ARLangValue Value);