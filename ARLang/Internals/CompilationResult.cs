using OneOf;
using OneOf.Types;

namespace ARLang.Internals;

[GenerateOneOf]
public partial class CompilationResult : OneOfBase<Error, Success, Success<EValueType>, KeyValuePair<string, Type>, Dictionary<string, Type>>
{
    public bool IsError => IsT0;
    public bool IsSuccess => IsT1;
    public bool IsSuccessWithType => IsT2;
    public bool IsSuccessWithArgs => IsT3;
    public bool IsSuccessWithDic => IsT4;

    public Error AsError => AsT0;
    public Success AsSuccess => AsT1;
    public EValueType AsSuccessWithType => AsT2.Value;
    public KeyValuePair<string, Type> AsSuccessWithArgs => AsT3;
    public Dictionary<string, Type> AsSuccessWithDic => AsT4;
}
