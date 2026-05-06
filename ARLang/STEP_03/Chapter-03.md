# Chapter 3 - Statements

The crux of the SLANG4.net can be summed up in two sentences.
 - Expression is what you evaluate for it's value
 - Statement is what you execute for it's effect 
---


    EBNF 

    stmtlist       ::= statement { statement }

    statement      ::= printstmt | printlinestmt

    printstmt      ::= "print" expr ";"
    printlinestmt  ::= "printline" expr ";"

    expr           ::= term { ("+" | "-") term }

    term           ::= factor { ("*" | "/") factor }

    factor         ::= number
                     | "(" expr ")"
                     | ("+" | "-") factor


Lets implement print and printline statements for ARLang.
We will change the IVisitorBase Visit method in this step to support statements.
We ll also change the lexer to split off PRINT and PRINTLN and semicolns. 
We ll change the parser to parse statement list and individual statements.