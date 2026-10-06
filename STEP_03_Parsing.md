# Parsing

Lexical analysis will identify lexemes from source code and produce a list of tokens. The next stage in the compilation pipeline is conversion of this list of tokens to a data structure that the rest of the pipeline can work with - and that data structure is AST. The module that converts the list of tokens into an AST is called a `parser`. 

The parser parsers the list of tokens by following a formal grammar. Formal grammar (or simply grammar) is a set of rules (we will see how to write these rules shortly) that describe which lists of tokens are valid in a language. For example, `1` `+` `2` `*` `3` is a valid sequence in our language, while `+` `1` `2` `*` `3` is not. A grammar describes only the form of a language, that is, which lists of tokens are well formed and how they are structured. It says nothing about what those token lists mean; that is the job of the later stages that interpret or compile the program. For example, if we try to evaluate the source code `1/0` in our language, the token list produced by the lexical analyzer will be `1` `/` `0`, which is a totally valid token list; which means, the parser will be able to create a valid AST from this token list. Checking whether that AST can be evaluated to produce a result is not the responsibility of the parser; that lies with the `Evaluate` function.

As we saw earlier, if a user tried to execute/compile for example, the expression `(*)76.23/11+-`, the list of tokens produced by lexical analyzer will not have any invalid token. But that doesnt mean there is no problem with that token list. The ordering of the tokens within the list is incorrect. The sequence is not in valid infix notation and thus ARLang's parser cannot generate an AST from this token list. To represent this type of scenario where the user input was lexically valid but structurally incorrect, we are going to introduce a type called `ArlParseError`. The parser will be returning a union `ArlParseResult` which can be either an instance of `ArlNumericExpression` or `ArlParseError`.

Before implementing the parser, lets see the "production rules" which collectively are what the grammar is in EBNF notation. It has been decided that EBNF is out of context for this book. (Might change later). 

```ebnf
expression = term { ("+" | "-") term } ;
term       = unary { ("*" | "/") unary } ;
unary      = ("+" | "-") unary | factor ;
factor     = NUMBER | "(" expression ")" ;
```

Each line in the above EBNF represents a single production rule of the grammar and each one translates to a single method inside the parser itself. Now lets look at our parser at a high level. You can see there is a one to one correspondance to the methods in the parser and the production rules in our EBNF. `expressison` in EBNF is implemented as `ParseExpression` method in our parser, `term` in EBNF is implemented as `ParseTerm`, `unary` is implemented as `ParseUnaryExpression` and `factor` is implemented as `ParseFactor`.

```csharp
public class Parser
{
    private int index = 0;
    private ImmutableList<Token> tokens = [];
    public ArlParseResult Parse(ImmutableList<Token> tokens)
    {
        this.tokens = tokens;
        throw new NotImplementedException();
    }
    private ArlParseResult ParseExpression()
    {
        throw new NotImplementedException();
    }
    private ArlParseResult ParseTerm()
    {
        throw new NotImplementedException();
    }
    private ArlParseResult ParseUnaryExpression()
    {
        throw new NotImplementedException();
    }
    private ArlParseResult ParseFactor()
    {
        throw new NotImplementedException();
    }
}
```
