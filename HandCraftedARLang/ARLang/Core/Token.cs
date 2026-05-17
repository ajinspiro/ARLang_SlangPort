namespace ARLang.Core;

/// <summary>
/// Represents a token that was lexically analyzed by Lexer. For e.g., (TokenType.Number, "23.45") or (TokenType.UnquotedString, "Factorial")  
/// </summary>
/// <param name="Type"></param>
/// <param name="Value"></param>
public record Token(TokenType Type, string Value)
{
    public override string ToString()
    {
        return $"{Type} {Value}";
    }
};
