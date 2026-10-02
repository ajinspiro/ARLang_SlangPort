# Lexical Analysis

Lexical analysis is the first process that happens in the compilation of source code. Lets recall that source code is a sequence of characters. If so, lexical analysis is the process of grouping these characters into meaningful groups called tokens (also called lexemes). For example, if the source code was `3` `4` `+` `2` `5` `7` `*` `3`, the lexer needs to create tokens as `34`, `+`, `257`, `*`, and `3`. Note that the lexer identified the occurance of characters `3` and `4` in sequence and produced the token `34`. Similarly, `2`, `5` and `7` occuring in sequence was identified as the token `257`. But `*` and `3` occuring in sequence was not grouped as a single token in a similar way because it doesnt make sense to do so in ARLang. This naturally demands the question "how come the lexer know which characters to group together and which ones to separate ?". The answer is, the lexer's implementation/logic achieves precisely that - separating and grouping together characters in the source code. Lets look at a tiny lexical analyzer that accepts a numeric math expression as source code and produces an array of tokens as output. _Check `STEP_02` in `Code` folder for full code._




<hr/>

### TODO : Parsing

Any input has to be parsed before the object code translation. To Parse means to understand.
The Parsing process works as follows
The output from the Lexer is passed into a module which identifies whether
a group of tokens form a valid expression or a statement in the program. The module which determines
the validity of expressions is called a parser. 
To put everything together let us write a small program which acts a four function calculator
The calculator is capable of evaluating mathematical expressions which contains four basic
 arithmetical operators , paranthesis to group the expression and unary operators.
