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
