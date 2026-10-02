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

Console.WriteLine("===STEP 02===");
string expression1 = "1+2*3";
Console.WriteLine($"Lexical analysis of expression {expression1}:");
var tokens = LexicalAnalyzer.Instance.ProduceTokens(expression1);
foreach (var item in tokens)
{
    Console.WriteLine(item);
}
