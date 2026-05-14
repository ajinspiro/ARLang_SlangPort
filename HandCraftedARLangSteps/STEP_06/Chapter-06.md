# Chapter 6 - Control structures (IF, WHILE) and comments

---
    EBNF
    stmtlist ::= statement { statement }
    
    statement ::= printstmt
                  | printlinestmt
                  | vardeclstmt
    
    printstmt ::= "print" expr ";"
    
    printlinestmt ::= "printline" expr ";"
    
    vardeclstmt ::= "STRING" varname ";"
                    | "NUMERIC" varname ";"
                    | "BOOLEAN" varname ";"
    
    expr ::= term { ("+" | "-") term }
    
    term ::= factor { ("*" | "/") factor }
    
    factor ::= number
               | variable
               | "TRUE"
               | "FALSE"
               | "(" expr ")"
               | ("+" | "-") factor
    
    varname ::= identifier
    
    variable ::= identifier
---