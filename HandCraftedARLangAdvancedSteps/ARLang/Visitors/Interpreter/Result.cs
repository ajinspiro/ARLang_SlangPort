using ARLang.SyntaxTree;
using OneOf;
using OneOf.Types;

namespace ARLang.Visitors.Interpreter;

[GenerateOneOf]
public partial class Result : OneOfBase<Error, Success, ReturnResult>
{
    public bool IsError => IsT0;
    public bool IsSuccess => IsT1;
    public bool IsReturnResult => IsT2;

    public Error AsError => AsT0;
    public Success AsSuccess => AsT1;
    public ReturnResult AsReturnResult => AsT2;
}

public record ReturnResult(ARLangExpressionBase Value) : ARLangExpressionBase;