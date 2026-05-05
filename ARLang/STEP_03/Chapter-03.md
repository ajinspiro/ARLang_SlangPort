# Chapter 3 - Statements

The crux of the SLANG4.net can be summed up in two sentences.
 - Expression is what you evaluate for it's value
 - Statement is what you execute for it's effect 
---


    EBNF of expression evaluator

    Expr   ::= Term { ("+" | "-") Term }
    Term   ::= Factor { ("*" | "/") Factor }
    Factor ::= Number | "(" Expr ")" | ("+" | "-") Factor


Lets implement print and printline statements for ARLang.