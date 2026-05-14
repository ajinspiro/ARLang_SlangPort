
using ARLang.SyntaxTree;
using ARLang.Visitors;
using ARLang.Visitors.Interpreter;

// 1+2
ARLangExpressionBase expression1 = new AdditionExpression(
    new NumericConstantExpression(1),
    new NumericConstantExpression(2)
);

// 5*10
ARLangExpressionBase expression2 = new MultiplicationExpression(
    new NumericConstantExpression(5),
    new NumericConstantExpression(10)
);

// -(10 + (30+50)) = -90
ARLangExpressionBase expression3 = new UnaryMinusExpression(
    new AdditionExpression(
        new NumericConstantExpression(10),
        new AdditionExpression(
            new NumericConstantExpression(30),
            new NumericConstantExpression(50)
        )
    )
);

// Testing unary plus. Result must be -90
ARLangExpressionBase expression4 = new UnaryPlusExpression(expression3);

// Testing unary minus. Result must be 90
ARLangExpressionBase expression5 = new UnaryMinusExpression(expression3);

// Testing double application of unary minus. Result must be -90
ARLangExpressionBase expression6 = new UnaryMinusExpression(expression5);

// Testing division by zero
ARLangExpressionBase expression7 = new DivisionExpression(
    new NumericConstantExpression(1),
    new NumericConstantExpression(0)
);

// Testing division by zero
ARLangExpressionBase expression8 = new AdditionExpression(
    new DivisionExpression(
        new NumericConstantExpression(1),
        new NumericConstantExpression(0)
    ),
    new NumericConstantExpression(1)
);

EvaluateExpression(expression1, expression2, expression3, expression4, expression5, expression6, expression7, expression8);

static void EvaluateExpression(params ARLangExpressionBase[] expressions)
{
    IVisitorBase interpreter = new Interpreter();
    foreach (var expression in expressions)
    {
        var result = interpreter.Visit(expression);
        Console.WriteLine(result);
    }
}
