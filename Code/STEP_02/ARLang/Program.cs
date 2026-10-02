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
Console.WriteLine($"Lexical analysis of expression [{expression1}]:");
var tokens1 = LexicalAnalyzer.Instance.ProduceTokens(expression1);
foreach (var item in tokens1)
{
    Console.WriteLine(item);
}
Console.WriteLine("=== ===");

string expression2 = " 34 + 2.3 * 0.55";
Console.WriteLine($"Lexical analysis of expression [{expression2}]:");
var tokens2 = LexicalAnalyzer.Instance.ProduceTokens(expression2);
foreach (var item in tokens2)
{
    Console.WriteLine(item);
}
Console.WriteLine("=== ===");

string expression3 = "\t 34   + 2.3\t  \n * 0.55\t";
Console.WriteLine($"Lexical analysis of expression [{expression3}]:");
var tokens3 = LexicalAnalyzer.Instance.ProduceTokens(expression3);
foreach (var item in tokens3)
{
    Console.WriteLine(item);
}
