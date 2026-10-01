using ARLang.Core;

ArlExpression ast1 = new ArlAddition(
    new ArlConstant(1), new ArlConstant(2)
);

Console.WriteLine($"1+2={ast1.Evaluate()}");

ArlExpression ast2 = new ArlMultiplication(
                        new ArlAddition(
                            new ArlConstant(1), new ArlConstant(2)
                        ),
                        new ArlConstant(3)
                    );

Console.WriteLine($"1+2*3={ast2.Evaluate()}");