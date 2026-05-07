# Chapter 4 - Types, Variables and Assignment Statement

In this step , we will support data types and variables to SLANG. Assignment statement will also be
implemented in this step.
The language supports only three data types viz
- NUMERIC
- STRING
- BOOLEAN

---
    EBNF 

    stmtlist       ::= statement { statement }

    statement      ::= printstmt | printlinestmt

    printstmt      ::= "print" expr ";"
    printlinestmt  ::= "println" expr ";"

    expr           ::= term { ("+" | "-") term }

    term           ::= factor { ("*" | "/") factor }

    factor         ::= number
                     | "(" expr ")"
                     | ("+" | "-") factor
---
We will add BooleanConstantExpression node and StringLiteralExpression in this step. We will also introduce RuntimeContext class and CompilationContext class.  We will add type checking and Variable support. Also we will add SymbolName to SymbolInfo. We will also introduce type checking in this step so that we will error out properly when user tries to evaluate invalid operands together like 
10 + true

Lexer and parser needs to be modified to support the new data types. 
Inside lexer, we will also support decimal part for numeric constants.
Also the lexer will support variable names using unquoted string type.