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
    END_OF_STRING,

    // Step 3 Start
    PRINT,                  // Print statement. Commented out in step 4. We will be using UNQUOTED_STRING
    PRINTLN,                // PrintLine statement. Commented out in step 4. We will be using UNQUOTED_STRING
    UNQUOTED_STRING,        // Identifiers (Variable name, Function name etc)
    SEMICOLON,

    // Step 4 Start
    VARIABLE_NUMBER,        // Variable names that store numeric value
    VARIABLE_STRING,        // Variable names that store string value 
    VARIABLE_BOOL,          // Variable names that store boolean value
    COMMENT,                // Comment Token ( presently not used )   
    BOOL_TRUE,              // Boolean TRUE
    BOOL_FALSE,             // Boolean FALSE
    STRING,                 // String Literal
    ASSIGN                  // Assignment Symbol =  
}
