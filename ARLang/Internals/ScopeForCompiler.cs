using OneOf.Types;

namespace ARLang.Internals;

public class ScopeForCompiler(string name)
{
    private readonly string _name = name;
    private readonly Dictionary<string, Variable> symbols = [];
    public ScopeForCompiler? ParentScope { get; set; } = null;

    public ErrorOrSuccess Declare(string name, string type)
    {
        if (symbols.ContainsKey(name))
        {
            return new Error();
        }
        symbols.Add(name, new Variable(type, new None()));
        return new Success();
    }

    public ErrorOrSuccess Assign(string name, Value value)
    {
        bool isVarialbeDeclared = symbols.TryGetValue(name, out Variable? variable);
        if (isVarialbeDeclared)
        {
            if (variable is null) throw new InvalidProgramException(); // not possible
                                                                       // TODO: Implement type checking
            variable = variable with { Value = value };
            symbols[name] = variable;
            return new Success();
        }
        if (ParentScope is null)
        {
            return new Error();
        }
        return ParentScope.Assign(name, value);
    }

    public ErrorOrSuccess Resolve(string name)
    {
        bool isVariableDefinedInCurrentScope = symbols.TryGetValue(name, out Variable? variable);
        if (isVariableDefinedInCurrentScope)
        {
            return variable?.Value ?? throw new InvalidProgramException(); // wont throw in real use case
        }
        if (ParentScope is null)
        {
            return new Error();
        }
        return ParentScope.Resolve(name);
    }
}
