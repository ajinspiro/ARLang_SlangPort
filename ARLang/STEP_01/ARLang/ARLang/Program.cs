
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

EvaluateExpression(expression1, expression2, expression3);

static void EvaluateExpression(params ARLangExpressionBase[] expressions)
{
    IVisitorBase interpreter = new Interpreter();
    foreach (var expression in expressions)
    {
        var result = interpreter.Visit(expression);
        Console.WriteLine(result);
    }
}
