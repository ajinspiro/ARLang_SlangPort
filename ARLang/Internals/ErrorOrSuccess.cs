using OneOf;
using OneOf.Types;

namespace ARLang.Internals;

[GenerateOneOf]
public partial class ErrorOrSuccess : OneOfBase<Error, Success, Value, Value[], KeyValuePair<string, string>, Dictionary<string, string>>
{
    public bool IsError => IsT0;
    public bool IsSuccess => IsT1;
    public bool IsSuccessWithValue => IsT2;
    public bool IsSuccessWithValueArray => IsT3;
    public bool IsSuccessWithKeyValuePair => IsT4;
    public bool IsSuccessWithDictionary => IsT5;


    public Error AsError => AsT0;
    public Success AsSuccess => AsT1;
    public Value AsSuccessWithValue => AsT2;
    public Value[] AsSuccessWithValueArray => AsT3;
    public KeyValuePair<string, string> AsSuccessWithKeyValuePair => AsT4;
    public Dictionary<string, string> AsSuccessWithDictionary => AsT5;
}
