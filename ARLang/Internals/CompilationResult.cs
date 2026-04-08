using OneOf;
using OneOf.Types;

namespace ARLang.Internals;

[GenerateOneOf]
public partial class CompilationResult : OneOfBase<Error, Success, Success<EValueType>>
{
    public bool IsError => IsT0;
    public bool IsSuccess => IsT1;
    public bool IsSuccessWithType => IsT2;

    public Error AsError => AsT0;
    public Success AsSuccess => AsT1;
    public EValueType AsSuccessWithType => AsT2.Value;
}
