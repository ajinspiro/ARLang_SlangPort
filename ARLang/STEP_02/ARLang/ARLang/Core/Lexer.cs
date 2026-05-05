using System.Collections.Immutable;
using OneOf.Types;

namespace ARLang.Core;

public class Lexer
{
    private readonly string expressionString;
    private int index = 0;
    public double Number { get; private set; }
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
        // Skip white spaces
        while (index < expressionString.Length && (expressionString[index] == ' ' || expressionString[index] == '\t'))
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
                    Number = Convert.ToDouble(str);
                    tok = TokenType.NUMBER;
                }
                break;
            default:
                tok = TokenType.ILLEGAL_TOKEN;
                break;
        }
        return new SymbolInfo(tok, tok == TokenType.NUMBER ? Number : new None());
    }
}
