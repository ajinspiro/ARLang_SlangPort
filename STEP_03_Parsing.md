# Parsing

Lexical analysis will identify lexemes from source code and produce a list of tokens. The next stage in the compilation pipeline is conversion of this list of tokens to a data structure that the rest of the pipeline can work with - and that data structure is AST. The module that converts the list of tokens into an AST is called a `parser`. 

The parser parsers the list of tokens by following a formal grammar. Formal grammar (or simply grammar) is a set of rules (we will see how to write these rules shortly) that describe which lists of tokens are valid in a language. For example, `1` `+` `2` `*` `3` is a valid sequence in our language, while `+` `1` `2` `*` `3` is not. A grammar describes only the form of a language, that is, which lists of tokens are well formed and how they are structured. It says nothing about what those token lists mean; that is the job of the later stages that interpret or compile the program. For example, if we try to evaluate the source code `1/0` in our language, the token list produced by the lexical analyzer will be `1` `/` `0`, which is a totally valid token list; which means, the parser will be able to create a valid AST from this token list. Checking whether that AST can be evaluated to produce a result is not the responsibility of the parser; that lies with the `Evaluate` function.

As we saw earlier, if a user tried to execute/compile for example, the expression `(*)76.23/11+-`, the list of tokens produced by lexical analyzer will not have any invalid token. But that doesnt mean there is no problem with that token list. The ordering of the tokens within the list is incorrect. The sequence is not in valid infix notation and thus ARLang's parser cannot generate an AST from this token list. To represent this type of scenario where the user input was lexically valid but structurally incorrect, we are going to introduce a type called `ArlParseError`. The parser will be returning a union `ArlParseResult` which can be either an instance of `ArlNumericExpression` or `ArlParseError`.

```csharp
public record ArlParseError(string Message);

[GenerateOneOf]
public partial class ArlParseResult : OneOfBase<ArlNumericExpression, ArlParseError>
{
    public bool IsResult => IsT0;
    public bool IsError => IsT1;
    public ArlNumericExpression AsResult => AsT0;
    public ArlParseError AsError => AsT1;
};
```

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
        if (tokens.Count == 0)
        {
            return new ArlParseError("Empty source code.");
        }
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

## Step 1 - Implementing production rule `factor`

<h3 align="center"><code>factor = NUMBER | "(" expression ")" ;</code></h3>


Lets start off by implementing the 4th production rule `factor` inside the `ParseFactor` method. The `factor` production has two "alternatives". Either a factor can be a `NUMBER` or it can be a parenthesised nested expression.

### Alternative 1 - `NUMBER`

Our `ParseFactor` method should first check if the current token is a number and if so, move to next token and return an instance of `ArlNumericConstant`. Lets implement that alternative before explaining furthur.

```csharp
private ArlParseResult ParseFactor()
{
    if (index < tokens.Count && tokens[index].IsTokenNumericConstant)
    {
        double value = tokens[index].AsTokenNumericConstant.Value;
        ArlNumericConstant astNode = new(value);
        ArlNumericExpression astNodeUnion = new(astNode);
        index++; // move to next token
        return astNodeUnion;
    }
    throw new NotImplementedException();
}
```

### Alternative 2 - `"(" expression ")"`
If current token was not a numeric constant, then according to our `factor` production, it must be a parenthesised nested expression. We will check that by checking if the current token is open parenthesis. If so, we will move to the next token and try to parse an expression by calling `ParseExpression`. After we parsed the nested expression, we should be left with a close parenthesis token. If we encounter a close parenthesis token, the parse operation was successful and we will simply move to the next token and return an instance of `ArlNumericExpression` wrapped in `ArlParseResult` union. Lets see that. If not, the programmer didn't properly close the nested expression using `)` so we will produce an `ArlParseError`.

```csharp
private ArlParseResult ParseFactor()
{
    if (index < tokens.Count && tokens[index].IsTokenNumericConstant)
    {
        double value = tokens[index].AsTokenNumericConstant.Value;
        ArlNumericConstant astNode = new(value);
        ArlNumericExpression astNodeUnion = new(astNode);
        index++;
        return astNodeUnion;
    }
    if (index < tokens.Count && tokens[index].IsTokenOpenParenthesis) // nested expression
    {
        index++;
        ArlParseResult nestedExpressionResult = ParseExpression();
        if (nestedExpressionResult.IsError)
        {
            return nestedExpressionResult;
        }
        if (!(index < tokens.Count && tokens[index].IsTokenCloseParenthesis))
        {
            return new ArlParseError("Parse error - close parenthesis missing.");
        }
        index++;
        return nestedExpressionResult;
    }
    throw new NotImplementedException();
}
```

If neither of these alternatives of `factor` was not satisfied, then there is bad news, the user input does not follow our grammer. We will reject it with an instance of `ArlParseError`.

```csharp
private ArlParseResult ParseFactor()
{
    if (index < tokens.Count && tokens[index].IsTokenNumericConstant)
    {
        double value = tokens[index].AsTokenNumericConstant.Value;
        ArlNumericConstant astNode = new(value);
        ArlNumericExpression astNodeUnion = new(astNode);
        index++;
        return astNodeUnion;
    }
    if (index < tokens.Count && tokens[index].IsTokenOpenParenthesis) // nested expression
    {
        index++;
        ArlParseResult nestedExpressionResult = ParseExpression();
        if (nestedExpressionResult.IsError)
        {
            return nestedExpressionResult;
        }
        if (!(index < tokens.Count && tokens[index].IsTokenCloseParenthesis))
        {
            return new ArlParseError("Parse error - close parenthesis missing.");
        }
        index++;
        return nestedExpressionResult;
    }
    return new ArlParseError("Something went wrong - control flow should never reach here");
}
```

## Step 2 - Implementing production rule `unary`

<h3 align="center"><code>unary = ("+" | "-") unary | factor ;</code></h3>

Lets now implement our grammar's `unary` production inside the `ParseUnaryExpression` method. As per its definition, there are two alternatives for it.

### Alternative 1 - `("+" | "-") unary`
If the current token matches a `+` or `-`, then the `ParseUnaryExpression` method will save this operator in memory, move token poiner to next token and recurse (call itself). This recursed execution of `ParseUnaryExpression` will match with the second alternative `factor`. The result of `ParseFactor` and the previous operator that was kept in memory togeher will be used to create a `ArlNumericUnaryOperation` instance and then will be returned. 

```csharp
private ArlParseResult ParseUnaryExpression()
{
    if (index < tokens.Count && (tokens[index].IsTokenPlus || tokens[index].IsTokenMinus))
    {
        ArlNumericUnaryOperator unaryOperatorUnion = tokens[index].IsTokenPlus ? new Add() : new Sub();
        index++;
        ArlParseResult unaryOperandResult = ParseUnaryExpression();
        if (unaryOperandResult.IsError)
        {
            return unaryOperandResult;
        }
        ArlNumericUnaryOperation unaryOperation = new(unaryOperatorUnion, unaryOperandResult.AsResult);
        return new ArlNumericExpression(unaryOperation);
    }
    throw new NotImplementedException();
}
```

### Alternative 2 - `factor`

If the current token does not match `+` or `-`, then alternative 2 will be used, which is `factor`. `ParseFactor` method will be invoked and whatever returned will be returned by `ParseUnaryExpression` directly.

```csharp
private ArlParseResult ParseUnaryExpression()
{
    if (index < tokens.Count && (tokens[index].IsTokenPlus || tokens[index].IsTokenMinus))
    {
        ArlNumericUnaryOperator unaryOperatorUnion = tokens[index].IsTokenPlus ? new Add() : new Sub();
        index++;
        ArlParseResult unaryOperandResult = ParseUnaryExpression();
        if (unaryOperandResult.IsError)
        {
            return unaryOperandResult;
        }
        ArlNumericUnaryOperation unaryOperation = new(unaryOperatorUnion, unaryOperandResult.AsResult);
        return new ArlNumericExpression(unaryOperation);
    }
    return ParseFactor();
}
```

## Step 3 - Implementing production rule `term`

<h3 align="center"><code>term = unary { ("*" | "/") unary } ;</code></h3>

Lets now implement our grammar's `term` production inside the `ParseTerm` method. Unlike the productions we saw earlier, this one only has one alternative. For this, first we will try to parse a `unary` and keep it in an `lhsResult` variable.

```csharp
private ArlParseResult ParseTerm()
{
    ArlParseResult lhsResult = ParseUnaryExpression();
    if (lhsResult.IsError)
    {
        return lhsResult.AsError;
    }
    // TODO
    return lhsResult;
}
```

As per our production, after parsing unary, we will check if current token is `*` or `/`. If not so, we reached the end of the term and `lhsResult` will be returned. But if currnet token is `*` or `/`, we will save it in memory and try to parse another unary and save it in `rhsResult`. On successful parse, we will replace the `lhsResult` with a new `ArlNumericBinaryOperation` where `Lhs` is current value of `lhsResult`, operator is the previously saved operator and `Rhs` will be `rhsResult`. The entire thing is looped to parse all the unaries in the expression.

```csharp
private ArlParseResult ParseTerm()
{
    ArlParseResult lhsResult = ParseUnaryExpression();
    if (lhsResult.IsError)
    {
        return lhsResult.AsError;
    }
    while (index < tokens.Count && (tokens[index].IsTokenStar || tokens[index].IsTokenSlash))
    {
        ArlNumericBinaryOperator operatorUnion = tokens[index].IsTokenStar ? new Mul() : new Div();
        index++;
        ArlParseResult rhsResult = ParseUnaryExpression();
        if (rhsResult.IsError)
        {
            return rhsResult;
        }
        ArlNumericExpression binaryOperation = new ArlNumericBinaryOperation(lhsResult.AsResult, operatorUnion, rhsResult.AsResult);
        lhsResult = binaryOperation;
    }
    return lhsResult;
}
```

## Step 4 - Implementing production rule `expression`

<h3 align="center"><code>expression = term { ("+" | "-") term } ;</code></h3>

Lets now implement our grammar's `expression` production inside the `ParseExpression` method. Just like the production `term`, this one also only has one alternative. For this, first we will try to parse a `term` and keep it in an `lhsResult` variable.

```csharp
private ArlParseResult ParseExpression()
{
    ArlParseResult lhsResult = ParseTerm();
    if (lhsResult.IsError)
    {
        return lhsResult.AsError;
    }
    // TODO
    return lhsResult;
}
```

As per our production, after parsing term, we will check if current token is `+` or `-`. If not so, we reached the end of the expression and `lhsResult` will be returned. But if currnet token is `+` or `-`, we will save it in memory and try to parse another term and save it in `rhsResult`. On successful parse, we will replace the `lhsResult` with a new `ArlNumericBinaryOperation` where `Lhs` is current value of `lhsResult`, operator is the previously saved operator and `Rhs` will be `rhsResult`. The entire thing is looped to parse all the terms in the expression.

```csharp
private ArlParseResult ParseExpression()
{
    ArlParseResult lhsResult = ParseTerm();
    if (lhsResult.IsError)
    {
        return lhsResult.AsError;
    }
    while (index < tokens.Count && (tokens[index].IsTokenPlus || tokens[index].IsTokenMinus))
    {
        ArlNumericBinaryOperator operatorUnion = tokens[index].IsTokenPlus ? new Add() : new Sub();
        index++;
        ArlParseResult rhsResult = ParseTerm();
        if (rhsResult.IsError)
        {
            return rhsResult;
        }
        ArlNumericExpression binaryOperation = new ArlNumericBinaryOperation(lhsResult.AsResult, operatorUnion, rhsResult.AsResult);
        lhsResult = binaryOperation;
    }
    return lhsResult;
}
```

Finally lets conclude this step by implementing the top level `Parse` method. It just stores the token list produced by lexical analyzer in a `tokens` field, triggers the AST generation by calling off `ParseExpression`, stores the result in a `result` variable, resets the index used to scan the token list to 0 so that the parser instance can be resed if that is desired, then returns the result.

```csharp
public ArlParseResult Parse(ImmutableList<Token> tokens)
{
    if (tokens.Count == 0)
    {
        return new ArlParseError("Empty source code.");
    }
    this.tokens = tokens;
    var result = ParseExpression();
    // TODO
    index = 0;
    return result;
}
```

We have one more problem. If the user tries to parse something like `1 2` or `1)`, our `Parse` method will return `ArlNumericConstant(1)` instead of returning a `ArlParseError`. We need to fix that by checking if all tokens generated by the lexer has been consumed by the parser.

```csharp
if (index != tokens.Count) // before ParseExpression returns, index must become tokens.Count.
{
    result = new ArlParseError("Failed to consume all tokens because of malformed input.");
}
```

Our final `Parse` method will look like, 

```csharp
public ArlParseResult Parse(ImmutableList<Token> tokens)
{        
    if (tokens.Count == 0)
    {
        return new ArlParseError("Empty source code.");
    }
    this.tokens = tokens;
    var result = ParseExpression();
    if (index != tokens.Count) // before ParseExpression returns, index must become tokens.Count.
    {
        result = new ArlParseError("Failed to consume all tokens because of malformed input.");
    }
    index = 0;
    return result;
}
```

The unit tests for our parser will be:

```csharp
public class ParserTests
{
    [Theory(DisplayName = "Valid expressions produce result")]
    [InlineData("1+2*3", 7)]
    [InlineData("-2*(3+3)", -12)]
    [InlineData("-1 + 2", 1)]
    [InlineData("2 * -3 + 4", -2)]
    [InlineData("--(1 + 2)", 3)]
    public void Parser_Evaluation_Tests(string sourceCode, double expected)
    {
        var tokens = new LexicalAnalyzer().ProduceTokens(sourceCode);
        var parseResult = new Parser().Parse(tokens);
        tokens.ForEach(token => Assert.IsNotType<TokenInvalid>(token.Value));
        Assert.True(parseResult.IsResult);
        Assert.Equal(expected, parseResult.AsResult.Evaluate());
    }

    [Theory(DisplayName = "Valid token sequence that form an invalid expression returns parse error")]
    [InlineData("")]
    [InlineData("1 2")]
    [InlineData("(*)76.23/11+-")]
    [InlineData("1)")]
    [InlineData("1+")]
    [InlineData("1+(2*3")]
    public void InvalidExpression_ReturnsParseError(string sourceCode)
    {
        var tokens = new LexicalAnalyzer().ProduceTokens(sourceCode);
        var parseResult = new Parser().Parse(tokens);
        tokens.ForEach(token => Assert.IsNotType<TokenInvalid>(token.Value));
        Assert.IsType<ArlParseError>(parseResult.Value);
    }
}
```