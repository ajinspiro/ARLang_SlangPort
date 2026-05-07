using OneOf;
using OneOf.Types;

namespace ARLang.Core;

public record SymbolInfoTable
{
    private readonly Dictionary<string, SymbolInfo> table = [];

    public bool TryAdd(SymbolInfo symbolInfo)
    {
        if (symbolInfo.SymbolName is null)
        {
            return false;
        }

        return table.TryAdd(symbolInfo.SymbolName, symbolInfo);
    }

    public OneOf<None, SymbolInfo> Get(string name)
    {
        bool isSuccess = table.TryGetValue(name, out SymbolInfo? symbolInfo);
        if (isSuccess && symbolInfo is not null)
        {
            return symbolInfo;
        }
        else
        {
            return new None();
        }
    }
}
