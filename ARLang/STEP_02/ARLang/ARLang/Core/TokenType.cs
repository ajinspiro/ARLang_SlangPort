namespace ARLang.Core;

public enum TokenType
{
    // Step 2 Start
    ILLEGAL_TOKEN = -1,     // Not a Token
    PLUS = 1,               // '+'
    MINUS,                  // '-'
    STAR,                   // '*'
    SLASH,                  // '/'
    OPEN_PARENTHESIS,       // '('
    CLOSE_PARENTHESIS,      // ')'
    NUMBER,
    END_OF_STRING
    // Step 2 End
}
