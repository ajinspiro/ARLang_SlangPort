using OneOf;

namespace ARLang.Core;

[GenerateOneOf]
public partial class ArlExpression : OneOfBase<ArlConstant, ArlAddition, ArlSubtraction, ArlMultiplication, ArlDivision>;
public record ArlConstant(double Value);
public record ArlAddition(ArlExpression E1, ArlExpression E2);
public record ArlSubtraction(ArlExpression E1, ArlExpression E2);
public record ArlMultiplication(ArlExpression E1, ArlExpression E2);
public record ArlDivision(ArlExpression E1, ArlExpression E2);

public static class ArlExpressionExtensions
{
    extension(ArlExpression arlExpression)
    {
        public double Evaluate()
        {
            return arlExpression.Match(
                arlConstant => arlConstant.Value,
                arlAddition => Evaluate(arlAddition.E1) + Evaluate(arlAddition.E2),
                arlSubtraction => Evaluate(arlSubtraction.E1) - Evaluate(arlSubtraction.E2),
                arlMultiplication => Evaluate(arlMultiplication.E1) * Evaluate(arlMultiplication.E2),
                arlDivision => Evaluate(arlDivision.E1) / Evaluate(arlDivision.E2)
            );
        }
    }
}