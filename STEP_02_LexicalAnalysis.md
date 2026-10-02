# Lexical Analysis

Lexical analysis is the first process that happens in the compilation of source code. Source code is a sequence of characters and lexical analysis is the process of grouping these characters into meaningful groups. For example, if the source code was `3` `4` `+` `2` `5` `7` `*` `3`, the lexer needs to create these groups as `34`, `+`, `257`, `*`, and `3`. Note that 34 and 257 are grouped together. Such groups are called lexemes or tokens. We will pause the talk and resume coding - lets write a tiny lexical analyzer that accepts a numeric math expression string and produces an array of tokens. 




<hr/>

### TODO

Any input has to be parsed before the object code translation. To Parse means to understand.
The Parsing process works as follows
The output from the Lexer is passed into a module which identifies whether
a group of tokens form a valid expression or a statement in the program. The module which determines
the validity of expressions is called a parser. 
To put everything together let us write a small program which acts a four function calculator
The calculator is capable of evaluating mathematical expressions which contains four basic
 arithmetical operators , paranthesis to group the expression and unary operators.
