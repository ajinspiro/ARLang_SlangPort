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
                binOp => binOp.Operation.Match(
                    add => binOp.Lhs.Evaluate() + binOp.Rhs.Evaluate(),
                    sub => binOp.Lhs.Evaluate() - binOp.Rhs.Evaluate(),
                    mul => binOp.Lhs.Evaluate() * binOp.Rhs.Evaluate(),
                    div => binOp.Lhs.Evaluate() / binOp.Rhs.Evaluate()
                )
            );
        }
    }
}