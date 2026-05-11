using OneOf;
using OneOf.Types;

namespace ARLang.Core;

[GenerateOneOf]
public partial class ARLangValue : OneOfBase<None, double, string, bool>;

public record SymbolInfo(TokenType TokenType, ARLangValue Value, string? SymbolName = null);