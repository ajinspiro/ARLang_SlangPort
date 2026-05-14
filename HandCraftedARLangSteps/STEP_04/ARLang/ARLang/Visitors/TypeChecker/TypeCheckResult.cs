using ARLang.Visitors.TypeChecker;
using OneOf;
using OneOf.Types;

namespace ARLang.Visitors.Interpreter;

[GenerateOneOf]
public partial class TypeCheckResult : OneOfBase<Error<string>, SupportedTypes>
{
    public bool IsError => IsT0;
    public bool IsSuccess => IsT1;

    public Error<string> AsError => AsT0;
    public SupportedTypes AsSuccess => AsT1;
};
