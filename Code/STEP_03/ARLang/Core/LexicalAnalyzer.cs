using System.Collections.Immutable;
using OneOf;

namespace ARLang.Core;

public record TokenPlus;
public record TokenMinus;
public record TokenStar;
public record TokenSlash;
public record TokenOpenParenthesis;
public record TokenCloseParenthesis;
public record TokenNumericConstant(double Value);

// TokenInvalid is used to hold invalid characters 
// entered in source code (if they are present).
public record TokenInvalid(string Value);

// TokenTrivial is for modelling whitespaces, tabs etc. 
// Those that can be discarded as they dont need to be 
// used in AST building.
public record TokenTrivial;

[GenerateOneOf]
public partial class Token : OneOfBase<
    TokenPlus,
    TokenMinus,
    TokenStar,
    TokenSlash,
    TokenOpenParenthesis,
    TokenCloseParenthesis,
    TokenNumericConstant,
    TokenInvalid,
    TokenTrivial
>
{
    public bool IsTokenTrivial => IsT8;
};
public class LexicalAnalyzer
{
    private int index = 0;
    private LexicalAnalyzer() { }
    public static LexicalAnalyzer Instance { get; } = new();
    public ImmutableList<Token> ProduceTokens(string sourceCode)
    {
        if (sourceCode.Trim().Length == 0)
        {
            return [];
        }
        List<Token> tokens = [];
        Token tempToken;
        do
        {
            tempToken = GetToken(sourceCode);
            if (!tempToken.IsTokenTrivial)
            {
                tokens.Add(tempToken);
            }
        } while (index + 1 <= sourceCode.Length);
        index = 0;
        return [.. tokens];
    }

    private Token GetToken(string sourceCode)
    {
        switch (sourceCode[index])
        {
            case '+': { index++; return new TokenPlus(); }
            case '-': { index++; return new TokenMinus(); }
            case '*': { index++; return new TokenStar(); }
            case '/': { index++; return new TokenSlash(); }
            case '(': { index++; return new TokenOpenParenthesis(); }
            case ')': { index++; return new TokenCloseParenthesis(); }
            case ' ':
            case '\t':
            case '\r':
            case '\n': { index++; return new TokenTrivial(); }
            default:
                {
                    if (!IsDigit(sourceCode[index]))
                    {
                        return new TokenInvalid($"Invalid character {sourceCode[index++]}");
                    }
                    return GetDigitToken(sourceCode);
                }
        }
    }

    private Token GetDigitToken(string sourceCode)
    {
        string numberValueString = string.Empty;
        // index + 1 <= sourceCode.Length => checks if end of source code is reached
        while (index + 1 <= sourceCode.Length && (IsDigit(sourceCode[index]) || sourceCode[index] == '.'))
        {
            numberValueString += sourceCode[index];
            index++;
        }
        bool isParseSuccess = double.TryParse(numberValueString, out double result);
        return isParseSuccess ? new TokenNumericConstant(result) : new TokenInvalid($"Invalid token {numberValueString}");
    }

    private static bool IsDigit(char character)
    {
        List<char> digits = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
        return digits.Contains(character);
    }
}