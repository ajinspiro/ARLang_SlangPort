# Parsing

Any input has to be parsed before the object code translation. To Parse means to understand.
The Parsing process works as follows
The output from the Lexer is passed into a module which identifies whether
a group of tokens form a valid expression or a statement in the program. The module which determines
the validity of expressions is called a parser. 
To put everything together let us write a small program which acts a four function calculator
The calculator is capable of evaluating mathematical expressions which contains four basic
 arithmetical operators , paranthesis to group the expression and unary operators.
