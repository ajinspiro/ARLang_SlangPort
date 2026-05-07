using System.Collections.Immutable;
using OneOf.Types;

namespace ARLang.Core;

public class Lexer
{
    private readonly string expressionString;
    private int index = 0;
    private readonly List<KeywordEntry> keywords = [
      new (TokenType.PRINT, "PRINT"),
      new (TokenType.PRINTLN, "PRINTLINE"),
      new (TokenType.VARIABLE_NUMBER, "NUMERIC"),
      new (TokenType.VARIABLE_STRING, "STRING"),
      new (TokenType.VARIABLE_BOOL, "BOOLEAN"),
      new (TokenType.BOOL_TRUE, "TRUE"),
      new (TokenType.BOOL_FALSE, "FALSE"),
    ];

    public Lexer(string expressionString)
    {
        this.expressionString = expressionString;
    }

    public ImmutableList<SymbolInfo> Tokenize()
    {
        List<SymbolInfo> symbols = [];
        SymbolInfo temp;
        do
        {
            temp = GetToken();
            symbols.Add(temp);
        } while (temp.TokenType != TokenType.END_OF_STRING);
        return symbols.ToImmutableList();
    }

    private SymbolInfo GetToken()
    {
        double valueOfTokenizedNumber = 0;
        string valueOfTokenizedString = string.Empty;
        // Skip white spaces
        while (index < expressionString.Length && (expressionString[index] == ' ' || expressionString[index] == '\t' || expressionString[index] == '\r' || expressionString[index] == '\n'))
        {
            index++;
        }

        // if end of string is reached, return
        if (index == expressionString.Length)
        {
            return new SymbolInfo(TokenType.END_OF_STRING, new None());
        }

        TokenType tok;
        switch (expressionString[index])
        {
            case '+':
                tok = TokenType.PLUS;
                index++;
                break;
            case '-':
                tok = TokenType.MINUS;
                index++;
                break;
            case '/':
                tok = TokenType.SLASH;
                index++;
                break;
            case '*':
                tok = TokenType.STAR;
                index++;
                break;
            case '(':
                tok = TokenType.OPEN_PARENTHESIS;
                index++;
                break;
            case ')':
                tok = TokenType.CLOSE_PARENTHESIS;
                index++;
                break;
            case ';':
                tok = TokenType.SEMICOLON;
                index++;
                break;
            case '=':
                tok = TokenType.ASSIGN;
                index++;
                break;
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                {
                    string str = "";
                    while (index < expressionString.Length &&
                    (expressionString[index] == '0' ||
                    expressionString[index] == '1' ||
                    expressionString[index] == '2' ||
                    expressionString[index] == '3' ||
                    expressionString[index] == '4' ||
                    expressionString[index] == '5' ||
                    expressionString[index] == '6' ||
                    expressionString[index] == '7' ||
                    expressionString[index] == '8' ||
                    expressionString[index] == '9'))
                    {
                        str += Convert.ToString(expressionString[index]);
                        index++;
                    }
                    if (expressionString[index] == '.')
                    {
                        str += '.';
                        index++;
                        while (index < expressionString.Length &&
                       (expressionString[index] == '0' ||
                       expressionString[index] == '1' ||
                       expressionString[index] == '2' ||
                       expressionString[index] == '3' ||
                       expressionString[index] == '4' ||
                       expressionString[index] == '5' ||
                       expressionString[index] == '6' ||
                       expressionString[index] == '7' ||
                       expressionString[index] == '8' ||
                       expressionString[index] == '9'))
                        {
                            str += Convert.ToString(expressionString[index]);
                            index++;
                        }
                    }
                    valueOfTokenizedNumber = Convert.ToDouble(str);
                    tok = TokenType.NUMBER;
                }
                break;
            case '"':
                {
                    string x = string.Empty;
                    index++;
                    while (index < expressionString.Length && expressionString[index] != '"')
                    {
                        x = x + expressionString[index];
                        index++;
                    }

                    if (index == expressionString.Length)
                    {
                        tok = TokenType.ILLEGAL_TOKEN;
                    }
                    else
                    {
                        index++;
                        valueOfTokenizedString = x;
                        tok = TokenType.STRING;
                    }
                    break;
                }
            default:
                {
                    if (char.IsLetter(expressionString[index]))
                    {
                        string temp = Convert.ToString(expressionString[index]);
                        index++;
                        while (index < expressionString.Length && (char.IsLetterOrDigit(expressionString[index]) || expressionString[index] == '_'))
                        {
                            temp += expressionString[index];
                            index++;
                        }

                        KeywordEntry? matchedKeyword = keywords.FirstOrDefault(x => temp.ToUpperInvariant() == x.Value);
                        if (matchedKeyword is null)
                        {
                            // Match is an unquoted string, like the name of a variable.
                            tok = TokenType.UNQUOTED_STRING;
                            valueOfTokenizedString = temp;
                        }
                        else
                        {
                            // Keyword matched. 
                            tok = matchedKeyword.Token;
                        }
                        break;
                    }
                    else
                    {
                        tok = TokenType.ILLEGAL_TOKEN;
                        break;
                    }
                }
        }
        ARLangValue value = tok switch
        {
            TokenType.NUMBER => valueOfTokenizedNumber,
            TokenType.STRING => valueOfTokenizedString,
            TokenType.UNQUOTED_STRING => valueOfTokenizedString,
            _ => new None(),
            // Each boolean value (true and false) has dedicated token type. So, the parser can identify the value directly from token type.
        };
        return new SymbolInfo(tok, value);
    }
}
