using OneOf;

namespace ARLang.Core;

public record ArlNumericConstant(double Value);

[GenerateOneOf]
public partial class ArlNumericExpression : OneOfBase<ArlNumericConstant, ArlNumericBinaryOperation>;
public record Add;
public record Sub;
public record Mul;
public record Div;

[GenerateOneOf]
public partial class ArlNumericOperation : OneOfBase<Add, Sub, Mul, Div>;

public record ArlNumericBinaryOperation(ArlNumericExpression Lhs, ArlNumericOperation Operation, ArlNumericExpression Rhs);

public static class ArlangExtensions
{
    extension(ArlNumericExpression e)
    {
        public double Evaluate()
        {
            return e.Match(
                constant => constant.Value,
                binaryOperation => binaryOperation.Operation.Match(
                    add => binaryOperation.Lhs.Evaluate() + binaryOperation.Rhs.Evaluate(),
                    sub => binaryOperation.Lhs.Evaluate() - binaryOperation.Rhs.Evaluate(),
                    mul => binaryOperation.Lhs.Evaluate() * binaryOperation.Rhs.Evaluate(),
                    div => binaryOperation.Lhs.Evaluate() / binaryOperation.Rhs.Evaluate()
                )
            );
        }
    }
}