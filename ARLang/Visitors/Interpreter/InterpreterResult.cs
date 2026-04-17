using OneOf;
using OneOf.Types;

namespace ARLang.Visitors.Interpreter;

[GenerateOneOf]
public partial class InterpreterResult : OneOfBase<Error, Success, Value, Value[], KeyValuePair<string, string>, Dictionary<string, string>, None>
{
    public bool IsError => IsT0;
    public bool IsSuccess => IsT1;
    public bool IsSuccessWithValue => IsT2;
    public bool IsSuccessWithValueArray => IsT3;
    public bool IsSuccessWithKeyValuePair => IsT4;
    public bool IsSuccessWithDictionary => IsT5;
    public bool IsNone => IsT6;


    public Error AsError => AsT0;
    public Success AsSuccess => AsT1;
    public Value AsSuccessWithValue => AsT2;
    public Value[] AsSuccessWithValueArray => AsT3;
    public KeyValuePair<string, string> AsSuccessWithKeyValuePair => AsT4;
    public Dictionary<string, string> AsSuccessWithDictionary => AsT5;
    public None AsNone => AsT6;
}

[GenerateOneOf]
public partial class Value : OneOfBase<double, string, bool, None>
{
    public bool IsNumeric => IsT0;
    public bool IsString => IsT1;
    public bool IsBoolean => IsT2;
    public bool IsNone => IsT3;

    public double AsNumeric => AsT0;
    public string AsString => AsT1;
    public bool AsBoolean => AsT2;
    public None AsNone => AsT3;
}