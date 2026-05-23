
using ARLang.Core;
using OneOf;
using OneOf.Types;

namespace ARLang.Visitors.Interpreter;

public class RuntimeScope
{
    private readonly RuntimeScope? ParentRuntimeScope = null;
    private readonly Dictionary<string, RuntimeVariable> table = [];
    public RuntimeScope(RuntimeScope? parent = null)
    {
        ParentRuntimeScope = parent;
    }
    public bool Declare(string name, DataType dataType)
    {
        ARLangValue value = dataType switch
        {
            DataType.NUMERIC => 0,
            DataType.STRING => string.Empty,
            DataType.BOOLEAN => false,
            _ => throw new Exception("INTERPRETER: Not possible. Any datatype error will be caught in parser/semantic analyser.")
        };
        return table.TryAdd(name, new RuntimeVariable(dataType, name, value));
    }

    public OneOf<Error, RuntimeVariable> Lookup(string name)
    {
        bool isSuccess = table.TryGetValue(name, out RuntimeVariable? symbolInfo);
        if (isSuccess)
        {
            if (symbolInfo is null) throw new Exception("INTERPRETER: Not possible.");
            return symbolInfo;
        }
        else if (ParentRuntimeScope is null)
        {
            return new Error();
        }
        else
        {
            return ParentRuntimeScope.Lookup(name);
        }
    }

    public void Assign(string name, ARLangValue value)
    {
        RuntimeVariable? variableToUpdate = table[name];
        if (variableToUpdate is not null)
        {
            variableToUpdate = variableToUpdate with { Value = value };
            table[name] = variableToUpdate;
        }
        else if (ParentRuntimeScope is null)
        {
            throw new Exception("INTERPRETER: Not possible. Semantic analysis will catch undeclared variables.");
        }
        else
        {
            ParentRuntimeScope.Assign(name, value);
        }
    }
}
