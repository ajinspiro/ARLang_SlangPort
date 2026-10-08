# Lexical Analysis

Lexical analysis is the first process that happens in the compilation of source code. Lets recall that source code is a sequence of characters. If so, lexical analysis is the process of grouping these characters into meaningful groups called `lexemes` and categorizing each of them as `tokens`. For example, if the source code was `3` `4` `+` `2` `5` `7` `*` `3`, the lexical analyzer needs to create lexemes as `34`, `+`, `257`, `*`, and `3`. Note that the lexical analyzer identified the occurance of characters `3` and `4` in sequence and produced the lexeme `34`. Similarly, `2`, `5` and `7` occuring in sequence was identified as the token `257`. But `*` and `3` occuring in sequence was not grouped as a single lexeme in a similar way because it doesnt make sense to do so in ARLang. After doing this grouping, each lexeme is categorized. In the above example, upon encountering `34`, lexical analyzer will create a token, categorize it as something like `NumericConstant` and attach the numeric value __34__ to it and adds it to the output. Lets look at a tiny lexical analyzer that accepts a numeric math expression as source code and produces a list of tokens as output.

Lets first introduce records that represents each type of token lexical analyzer will produce.

```csharp
public record TokenPlus;
public record TokenMinus;
public record TokenStar;
public record TokenSlash;
public record TokenOpenParenthesis;
public record TokenCloseParenthesis;
public record TokenNumericConstant(double Value);

// TokenInvalid is used to hold invalid lexemes 
// entered in source code (if they are present).
public record TokenInvalid(string Value);

// TokenTrivial is for modelling whitespaces, tabs etc. 
// Those that can be discarded as they dont need to be 
// used in AST building.
public record TokenTrivial; 
```

Now lets create a union for representing a token. A token will be one instance of either of the above listed record (token) types. For ease of coding, we will create a `IsTokenTrivial` and other such custom properties that wraps `IsT8` and other such properties of the union simply because it reads better. If we went with `IsT8` everyone will need to lookup what 8th type is and its not fun in a union where there are a lot of members.

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
    public bool IsTokenPlus => IsT0;
    public bool IsTokenMinus => IsT1;
    public bool IsTokenStar => IsT2;
    public bool IsTokenSlash => IsT3;
    public bool IsTokenOpenParenthesis => IsT4;
    public bool IsTokenCloseParenthesis => IsT5;
    public bool IsTokenNumericConstant => IsT6;
    public bool IsTokenInvalid => IsT7;
    public bool IsTokenTrivial => IsT8;

    public TokenPlus AsTokenPlus => AsT0;
    public TokenMinus AsTokenMinus => AsT1;
    public TokenStar AsTokenStar => AsT2;
    public TokenSlash AsTokenSlash => AsT3;
    public TokenOpenParenthesis AsTokenOpenParenthesis => AsT4;
    public TokenCloseParenthesis AsTokenCloseParenthesis => AsT5;
    public TokenNumericConstant AsTokenNumericConstant => AsT6;
    public TokenInvalid AsTokenInvalid => AsT7;
    public TokenTrivial AsTokenTrivial => AsT8;
};
```

Now lets look at our lexical analyzer at a high level.

```csharp
public class LexicalAnalyzer
{
    private int index = 0;
    public ImmutableList<Token> ProduceTokens(string sourceCode)
    {
        throw new NotImplementedException();
    }
}
```

`LexicalAnalyzer` has a public `ProduceTokens(string)` method. This method is the whole lexical analysis process. We will decompose this into sub-processes shortly. LexicalAnalyzer also has a private `index` property. This property helps us to keep track of which character of the source code we are trying to identify a token using `sourceCode[index]`.

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
    bool isParseSuccess = double.TryParse(numberValueString, CultureInfo.InvariantCulture, out double result);
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
                    return new TokenInvalid($"Invalid character {sourceCode[index++]}");
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
    public void ExpressionContainsNoInvalidCharacters_SuccessfulLexicalOutput()
    {
        string expression1 = "1+2*3";
        var actuals = new LexicalAnalyzer().ProduceTokens(expression1);

        List<Token> expected = [
            new TokenNumericConstant(1),
            new TokenPlus(),
            new TokenNumericConstant(2),
            new TokenStar(),
            new TokenNumericConstant(3)
            ];
        Assert.Equal(expected.Count, actuals.Count);
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }

    [Fact]
    public void ExpressionWithWhitespacesContainsNoInvalidCharacters_SuccessfulLexicalOutput()
    {
        string expression1 = " 34 + 2.3 * 0.55";
        var actuals = new LexicalAnalyzer().ProduceTokens(expression1);

        List<Token> expected = [
            new TokenNumericConstant(34),
            new TokenPlus(),
            new TokenNumericConstant(2.3),
            new TokenStar(),
            new TokenNumericConstant(0.55)
            ];
        Assert.Equal(expected.Count, actuals.Count);
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }

    [Fact]
    public void ExpressionWithWhitespacesAndTabsContainsNoInvalidCharacters_SuccessfulLexicalOutput()
    {
        string expression1 = "\t 34   + 2.3\t  \n * 0.55\t";
        var actuals = new LexicalAnalyzer().ProduceTokens(expression1);

        List<Token> expected = [
            new TokenNumericConstant(34),
            new TokenPlus(),
            new TokenNumericConstant(2.3),
            new TokenStar(),
            new TokenNumericConstant(0.55)
            ];
        Assert.Equal(expected.Count, actuals.Count);
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }

    [Fact]
    public void EmptyExpression_EmptyTokenResult()
    {
        string expression1 = "";
        var actuals = new LexicalAnalyzer().ProduceTokens(expression1);

        Assert.NotNull(actuals);
        Assert.Empty(actuals);
    }

    [Fact]
    public void IncorrectExpressionButWithoutAnyInvalidCharacters_SuccessfulLexicalOutput()
    {
        string expression1 = "(*)76.23/11+-";
        var actuals = new LexicalAnalyzer().ProduceTokens(expression1);

        List<Token> expected = [
            new TokenOpenParenthesis(),
            new TokenStar(),
            new TokenCloseParenthesis(),
            new TokenNumericConstant(76.23),
            new TokenSlash(),
            new TokenNumericConstant(11),
            new TokenPlus(),
            new TokenMinus()
            ];

        Assert.Equal(expected.Count, actuals.Count);
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }

    [Fact]
    public void IncorrectExpressionWithInvalidCharacter_ContainsInvalidToken()
    {
        string expression1 = "6..2";
        var actuals = new LexicalAnalyzer().ProduceTokens(expression1);

        List<Token> expected = [
            new TokenInvalid($"Invalid token {expression1}")
            ];

        Assert.Equal(expected.Count, actuals.Count);
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }

    [Fact]
    public void IncorrectExpressionWithInvalidCharacter2_ContainsInvalidToken()
    {
        string expression1 = "1~2";
        var actuals = new LexicalAnalyzer().ProduceTokens(expression1);

        List<Token> expected = [
            new TokenNumericConstant(1),
            new TokenInvalid($"Invalid character ~"),
            new TokenNumericConstant(2)
            ];

        Assert.Equal(expected.Count, actuals.Count);
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }

    [Fact]
    public void InvalidExpressionWithoutAnyInvalidCharacter()
    {
        string expression1 = "1 2";
        var actuals = new LexicalAnalyzer().ProduceTokens(expression1);

        List<Token> expected = [
            new TokenNumericConstant(1),
            new TokenNumericConstant(2)
        ];

        Assert.Equal(expected.Count, actuals.Count);
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }

    [Fact]
    public void InvalidExpressionWithAnyInvalidCharacter()
    {
        string expression1 = "1)";
        var actuals = new LexicalAnalyzer().ProduceTokens(expression1);

        List<Token> expected = [
            new TokenNumericConstant(1),
            new TokenCloseParenthesis()
        ];

        Assert.Equal(expected.Count, actuals.Count);
        for (int i = 0; i < actuals.Count; i++)
        {
            Assert.Equal(expected[i], actuals[i]);
        }
    }
}
```

You are encouraged to put breakpoints and inspect the value of `actuals` variable to see the tokens generated.