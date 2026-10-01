using ARLang.Core;

ArlNumericExpression ast1 = new ArlNumericBinaryOperation(
    new ArlNumericConstant(1),
    new Add(),
    new ArlNumericBinaryOperation(
        new ArlNumericConstant(2),
        new Mul(),
        new ArlNumericConstant(3)
    )
);

ArlNumericExpression ast2 = new ArlNumericUnaryOperation(new Sub(), ast1);

Console.WriteLine($"1+2*3={ast1.Evaluate()}; Neg(7)={ast2.Evaluate()}");
