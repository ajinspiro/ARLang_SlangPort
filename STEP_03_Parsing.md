# Parsing

Lexical analysis will identify lexemes from source code and produce tokens. The next stage in the compilation pipeline is conversion of this group of tokens to a data structure that the rest of the pipeline can work with - and that data structure is AST. The module that converts the tokens into an AST is called a `parser`. Enough talk, lets code.

