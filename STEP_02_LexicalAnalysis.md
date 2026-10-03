# Lexical Analysis

Lexical analysis is the first process that happens in the compilation of source code. Lets recall that source code is a sequence of characters. If so, lexical analysis is the process of grouping these characters into meaningful groups called tokens (also called lexemes). For example, if the source code was `3` `4` `+` `2` `5` `7` `*` `3`, the lexer needs to create tokens as `34`, `+`, `257`, `*`, and `3`. Note that the lexer identified the occurance of characters `3` and `4` in sequence and produced the token `34`. Similarly, `2`, `5` and `7` occuring in sequence was identified as the token `257`. But `*` and `3` occuring in sequence was not grouped as a single token in a similar way because it doesnt make sense to do so in ARLang. This naturally demands the question "how come the lexer know which characters to group together and which ones to separate ?". The answer is, the lexer's implementation/logic achieves precisely that - separating and grouping together characters in the source code. Lets look at a tiny lexical analyzer that accepts a numeric math expression as source code and produces an array of tokens as output. 

Lets first introduce records that represents each type of token lexical analyzer will produce.

```csharp
public record TokenPlus;
public record TokenMinus;
public record TokenStar;
public record TokenSlash;
public record TokenOpenParenthesis;
public record TokenCloseParenthesis;
public record TokenNumericConstant(double Value);

// TokenInvalid is used to hold invalid characters 
// entered in source code (if they are present).
public record TokenInvalid(string Value);

// TokenTrivial is for modelling whitespaces, tabs etc. 
// Those that can be discarded as they dont need to be 
// used in AST building.
public record TokenTrivial; 
```

Now lets create a union for representing a token. A token will be one instance of either of the above listed record (token) types. For ease of coding, we will create a `IsTokenTrivial` custom wrapper property that wraps `IsT8` property of the union simply because it reads better. If we went with `IsT8` everyone will need to lookup what 8th type is and its not fun in a union where there are a lot of members.

```csharp
[GenerateOneOf]
public partial class Token : OneOfBase<
    TokenPlus,
    TokenMinus,
    TokenStar,
    TokenSlash,
    TokenOpenParenthesis,
    TokenCloseParenthesis,
    TokenNumericConstant,
    TokenInvalid,
    TokenTrivial
>
{
    public bool IsTokenTrivial => IsT8;
};
```

Now lets see the nutshell of our lexical analyzer.

```csharp
public class LexicalAnalyzer
{
    private int index = 0;
    private LexicalAnalyzer() { }
    public static LexicalAnalyzer Instance { get; } = new();
    public ImmutableList<Token> ProduceTokens(string sourceCode)
    {
        throw new NotImplementedException();
    }
}
```

`LexicalAnalyzer` has a public `ProduceTokens(string)` method. This method is the whole lexical analysis process. We will decompose this into sub-processes shortly. LexicalAnalyzer also has a private `index` property. This property helps us to keep track of which character of the source code we are trying to identify a token using `sourceCode[length]`. There is also a private constructor defined and an `Instance` property. Together they acheive a small trick - the private constructor prevents object creation outside class and the `Instance` property instantiates a LexicalAnalyzer once and the developers can reuse this object many times. This is done to make LexicalAnalyzer a singleton - meaning a class with only one object in the entire lifetime of the application. 

As we said earlier, we need to decompose the `ProduceTokens(string)` method. Lets begin by creating an `IsDigit(char)` method. Why we need this function will make sense way before the end of this chapter. `IsDigit(char)` simply checks if the input character is a digit or not.

```csharp
private static bool IsDigit(char character)
{
    List<char> digits = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
    return digits.Contains(character);
}
```

Next we will create `GetDigitToken(string)`. This method's purpose is to scan the source code forward from left to right to find specifically a numeric constant value and emit the `TokenNumericConstant` record object with its value in it. It uses the `IsDigit(char)` function we defined earlier.

```csharp
private Token GetDigitToken(string sourceCode)
{
    string numberValueString = string.Empty;
    // index + 1 <= sourceCode.Length => checks if end of source code is reached
    while (index + 1 <= sourceCode.Length && (IsDigit(sourceCode[index]) || sourceCode[index] == '.'))
    {
        numberValueString += sourceCode[index];
        index++;
    }
    bool isParseSuccess = double.TryParse(numberValueString, out double result);
    return isParseSuccess ? new TokenNumericConstant(result) : new TokenInvalid($"Invalid token {numberValueString}");
}
```

The next piece we need is `GetToken(string)`. This method's purpose is to scan the source code forward to find any valid match and emit the corresponding `Token` record with optionally its value in it. It uses the `IsDigit(char)` function and `GetDigitToken(string)` we defined earlier.

```csharp
private Token GetToken(string sourceCode)
{
    switch (sourceCode[index])
    {
        case '+': { index++; return new TokenPlus(); }
        case '-': { index++; return new TokenMinus(); }
        case '*': { index++; return new TokenStar(); }
        case '/': { index++; return new TokenSlash(); }
        case '(': { index++; return new TokenOpenParenthesis(); }
        case ')': { index++; return new TokenCloseParenthesis(); }
        case ' ':
        case '\t':
        case '\r':
        case '\n': { index++; return new TokenTrivial(); }
        default:
            {
                if (!IsDigit(sourceCode[index]))
                {
                    return new TokenInvalid($"Invalid chatacter {sourceCode[index++]}");
                }
                return GetDigitToken(sourceCode);
            }
    }
}
```

Finally, to make everything tick, we need our final piece `ProduceTokens(string)` itself. Notice that it uses the `GetToken` method.

```csharp
public ImmutableList<Token> ProduceTokens(string sourceCode)
{
    if (sourceCode.Trim().Length == 0)
    {
        return [];
    }
    List<Token> tokens = [];
    Token tempToken;
    do
    {
        tempToken = GetToken(sourceCode);
        if (!tempToken.IsTokenTrivial)
        {
            tokens.Add(tempToken);
        }
    } while (index + 1 <= sourceCode.Length);
    index = 0;
    return [.. tokens];
}
```
_Check `STEP_02` in `Code` folder for full code._

Lets test it out. Lets create a `LexicalAnalyzerTests` class and write a `Test1` test method in our test project to see if correct tokens are generated for `1+2*3`.

Our test methods will look like this:
```csharp
public class LexicalAnalyzerTests
{
    [Fact]
    public void Test1()
    {
        string expression1 = "1+2*3";
        var actuals = LexicalAnalyzer.Instance.ProduceTokens(expression1);

        List<Token> expected = [
            new TokenNumericConstant(1),
            new TokenPlus(),
            new TokenNumericConstant(2),
            new TokenStar(),
            new TokenNumericConstant(3)
            ];
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }

    [Fact]
    public void Test2()
    {
        string expression1 = " 34 + 2.3 * 0.55";
        var actuals = LexicalAnalyzer.Instance.ProduceTokens(expression1);

        List<Token> expected = [
            new TokenNumericConstant(34),
            new TokenPlus(),
            new TokenNumericConstant(2.3),
            new TokenStar(),
            new TokenNumericConstant(0.55)
            ];
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }

    [Fact]
    public void Test3()
    {
        string expression1 = "\t 34   + 2.3\t  \n * 0.55\t";
        var actuals = LexicalAnalyzer.Instance.ProduceTokens(expression1);

        List<Token> expected = [
            new TokenNumericConstant(34),
            new TokenPlus(),
            new TokenNumericConstant(2.3),
            new TokenStar(),
            new TokenNumericConstant(0.55)
            ];
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }
}
```

You are encouraged to put breakpoints and inspect the value of `actuals` variable to see the tokens generated.