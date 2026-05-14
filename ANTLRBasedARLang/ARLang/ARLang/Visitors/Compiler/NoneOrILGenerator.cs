using System.Reflection.Emit;
using OneOf;
using OneOf.Types;

namespace ARLang.Visitors.Compiler;

[GenerateOneOf]
public partial class NoneOrILGenerator : OneOfBase<None, ILGenerator>
{
    public bool IsNone => IsT0;
    public bool IsILGenerator => IsT1;

    public None AsNone => AsT0;
    public ILGenerator AsILGenerator => AsT1;
}
