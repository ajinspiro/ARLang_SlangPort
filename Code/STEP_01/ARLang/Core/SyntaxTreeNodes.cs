using OneOf;

namespace ARLang.Core;

public record ArlNumericConstant(double Value);

public record Add;
public record Sub;
public record Mul;
public record Div;

[GenerateOneOf]
public partial class ArlNumericBinaryOperator : OneOfBase<Add, Sub, Mul, Div>;

public record ArlNumericBinaryOperation(ArlNumericExpression Lhs, ArlNumericBinaryOperator Operation, ArlNumericExpression Rhs);

[GenerateOneOf]
public partial class ArlNumericUnaryOperator : OneOfBase<Add, Sub>;

public record ArlNumericUnaryOperation(ArlNumericUnaryOperator Operation, ArlNumericExpression Operand);

[GenerateOneOf]
public partial class ArlNumericExpression : OneOfBase<ArlNumericConstant, ArlNumericBinaryOperation, ArlNumericUnaryOperation>;

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
                ),
                unaryOp => unaryOp.Operation.Match(
                    add => unaryOp.Operand.Evaluate(),
                    sub => -unaryOp.Operand.Evaluate()
                )
            );
        }
    }
}