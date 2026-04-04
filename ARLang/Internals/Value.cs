using System;
using OneOf;
using OneOf.Types;

namespace ARLang.Internals;

[GenerateOneOf]
public partial class Value : OneOfBase<decimal, string, bool, None>
{
    public bool IsNumeric => IsT0;
    public bool IsString => IsT1;
    public bool IsBoolean => IsT2;
    public bool IsNone => IsT3;

    public decimal AsNumeric => AsT0;
    public string AsString => AsT1;
    public bool AsBoolean => AsT2;
    public None AsNone => AsT3;
}
