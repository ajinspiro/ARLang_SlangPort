# Chapter 2 - Input Analysis (Adding lexer)

We hard coded the syntax tree in step 1 for learning how to make a syntax tree. Now that we have done that lets understand the implications of this. 

The interpreter is not very useful because evaluating an expression of user's choice is not possible (Because the compiler is not sophisticated enough to accept any expression from the user). To evaluate a different expression, user need to take the source code of the compiler, write the syntax tree by hand without error and run it. Using a smartphone calculator is much easier. 

To solve this problem we need to accept expressions in string form like "1+2" or "-(1+2*3)" and evaluate them. For this we need to introduce a lexical analyzer.

TODO: Explain lexical analysis