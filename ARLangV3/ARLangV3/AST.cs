// using OneOf;

// namespace ARLangV3;

// [GenerateOneOf]
// public partial class Expr : OneOfBase<Constant, Addition, Subtraction, Multiplication, Division>;
// public record Constant(double N);
// public record Addition(Expr E1, Expr E2);
// public record Subtraction(Expr E1, Expr E2);
// public record Multiplication(Expr E1, Expr E2);
// public record Division(Expr E1, Expr E2);

using OneOf;

namespace ARLangV3;

[GenerateOneOf]
public partial class Expr : OneOfBase<Constant, Addition, Subtraction, Multiplication, Division>;
public record Constant(double N);
public record Addition(params Expr[] E);
public record Subtraction(Expr E1, Expr E2);
public record Multiplication(Expr E1, Expr E2);
public record Division(Expr E1, Expr E2);