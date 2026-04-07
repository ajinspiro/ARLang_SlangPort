using System.Reflection;
using System.Reflection.Emit;
using Antlr4.Runtime.Misc;
using ARLang.Internals;
using OneOf.Types;

namespace ARLang.Visitors;

public class Compiler : ARLangBaseVisitor<ErrorOrSuccess>
{
    private readonly RuntimeContext runtimeContext;
    private readonly PersistedAssemblyBuilder assemblyBuilder;
    private readonly ModuleBuilder moduleBuilder;
    private readonly TypeBuilder typeBuilder;
    private readonly Dictionary<string, MethodBuilder> methodBuilders = [];
    private NoneOrILGenerator ilGenerator = new None();
    public Compiler(RuntimeContext runtimeContext, string outputAssemblyName)
    {
        this.runtimeContext = runtimeContext;
        AssemblyName assemblyName = new(outputAssemblyName)
        {
            Version = new(1, 0, 0, 0)
        };
        assemblyBuilder = new PersistedAssemblyBuilder(
           assemblyName,
           typeof(object).Assembly
        );
        moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
        typeBuilder = moduleBuilder.DefineType("Program");
    }
    public override ErrorOrSuccess VisitModule([NotNull] ARLangParser.ModuleContext context)
    {
        return context.procedure().Select(Visit).ToList().Last();
    }
    public override ErrorOrSuccess VisitProcedure([NotNull] ARLangParser.ProcedureContext context)
    {
        string functionName = context.IDENTIFIER().GetText();
        var methAttrs = MethodAttributes.Static;
        methAttrs = functionName == "MAIN" ? methAttrs | MethodAttributes.Public : MethodAttributes.Private;
        var functionBuilder = typeBuilder.DefineMethod(functionName, methAttrs, typeof(void), null);
        methodBuilders[functionName] = functionBuilder;
        ilGenerator = functionBuilder.GetILGenerator();
        var result = Visit(context.statements());
        return result.Match<ErrorOrSuccess>(error => error, success => success, value => new Error(), valueArr => new Error(), kvp => new Error(), dic => new Error());
    }
    public override ErrorOrSuccess VisitStatements([NotNull] ARLangParser.StatementsContext context)
    {
        return context.statement().Select(Visit).ToList().Last();
    }
    public override ErrorOrSuccess VisitPrintlinestatement([NotNull] ARLangParser.PrintlinestatementContext context)
    {
        return base.VisitPrintlinestatement(context);
    }
    public override ErrorOrSuccess VisitExpr([NotNull] ARLangParser.ExprContext context)
    {
        return base.VisitExpr(context);
    }
    public override ErrorOrSuccess VisitBexpr([NotNull] ARLangParser.BexprContext context)
    {
        return base.VisitBexpr(context);
    }
    public override ErrorOrSuccess VisitLexpr([NotNull] ARLangParser.LexprContext context)
    {   // lexpr: rexpr (RELOP rexpr)?;
        // RELOP: '>' | '<' | '>=' | '<=' | '<>' | '==';
        var list = context.rexpr().Select(Visit).ToList();
        if (context.RELOP() is null)
        {
            return new Success();
        }

        switch (context.RELOP().GetText())
        {
            case ">": { ilGenerator.AsILGenerator.Emit(OpCodes.Cgt); break; }
            case "<": { ilGenerator.AsILGenerator.Emit(OpCodes.Clt); break; }
            case ">=": { ilGenerator.AsILGenerator.Emit(OpCodes.Div); break; }
            case "<=": { ilGenerator.AsILGenerator.Emit(OpCodes.Div); break; }
            case "<>":
                {
                    ilGenerator.AsILGenerator.Emit(OpCodes.Div);
                    ilGenerator.AsILGenerator.Emit(OpCodes.Ceq);
                    ilGenerator.AsILGenerator.Emit(OpCodes.Ldc_I4, 0);
                    ilGenerator.AsILGenerator.Emit(OpCodes.Ceq);
                    break;
                }
            case "==": { ilGenerator.AsILGenerator.Emit(OpCodes.Div); break; }
            default: { throw new InvalidProgramException(); }
        }

        return new Success();
    }
    public override ErrorOrSuccess VisitRexpr([NotNull] ARLangParser.RexprContext context)
    {   // rexpr: term (ADDOP term)*;
        if (!ilGenerator.IsILGenerator) return new Error();
        var list = context.term().Select(Visit).ToList();
        if (context.ADDOP().Length == 0)
        {
            return new Success();
        }
        context.ADDOP().ToList().ForEach(op =>
        {
            switch (op.GetText())
            {
                case "+": { ilGenerator.AsILGenerator.Emit(OpCodes.Add); break; }
                case "-": { ilGenerator.AsILGenerator.Emit(OpCodes.Sub); break; }
                default: throw new InvalidProgramException();
            }
            ;
        });
        return new Success();
    }
    public override ErrorOrSuccess VisitTerm([NotNull] ARLangParser.TermContext context)
    {   //term: factor (MULOP factor)*;
        if (!ilGenerator.IsILGenerator) return new Error();
        var list = context.factor().Select(Visit).ToList();
        if (context.MULOP().Length == 0)
        {
            return new Success();
        }
        context.MULOP().ToList().ForEach(op =>
        {
            switch (op.GetText())
            {
                case "*": { ilGenerator.AsILGenerator.Emit(OpCodes.Mul); break; }
                case "/": { ilGenerator.AsILGenerator.Emit(OpCodes.Div); break; }
                default: throw new InvalidProgramException();
            }
            ;
        });
        return new Success();
    }
    public override ErrorOrSuccess VisitFactor_Number([NotNull] ARLangParser.Factor_NumberContext context)
    {
        if (!ilGenerator.IsILGenerator) return new Error();
        ilGenerator.AsILGenerator.Emit(OpCodes.Ldc_R8, double.Parse(context.NUMBER().GetText()));
        return new Success();
    }
    public override ErrorOrSuccess VisitFactor_String([NotNull] ARLangParser.Factor_StringContext context)
    {
        if (!ilGenerator.IsILGenerator) return new Error();
        ilGenerator.AsILGenerator.Emit(OpCodes.Ldstr, context.STRING().GetText());
        return new Success();
    }
    public override ErrorOrSuccess VisitFactor_BoolTrue([NotNull] ARLangParser.Factor_BoolTrueContext context)
    {
        if (!ilGenerator.IsILGenerator) return new Error();
        ilGenerator.AsILGenerator.Emit(OpCodes.Ldc_I4, 1);
        return new Success();
    }
    public override ErrorOrSuccess VisitFactor_BoolFalse([NotNull] ARLangParser.Factor_BoolFalseContext context)
    {
        if (!ilGenerator.IsILGenerator) return new Error();
        ilGenerator.AsILGenerator.Emit(OpCodes.Ldc_I4, 0);
        return new Success();
    }
    public override ErrorOrSuccess VisitFactor_IDENTIFIER([NotNull] ARLangParser.Factor_IDENTIFIERContext context)
    {
        return base.VisitFactor_IDENTIFIER(context);
    }
    public override ErrorOrSuccess VisitFactor_NestedExpr([NotNull] ARLangParser.Factor_NestedExprContext context)
    {
        if (!ilGenerator.IsILGenerator) return new Error();
        return Visit(context.expr());
    }
    public override ErrorOrSuccess VisitFactor_UnaryFactor([NotNull] ARLangParser.Factor_UnaryFactorContext context)
    {
        if (!ilGenerator.IsILGenerator) return new Error();
        var result = Visit(context.factor());
        if (!result.IsSuccess) return new Error();
        ilGenerator.AsILGenerator.Emit(OpCodes.Neg);
        return new Success();
    }
    public override ErrorOrSuccess VisitFactor_BoolNotOperation([NotNull] ARLangParser.Factor_BoolNotOperationContext context)
    {
        if (!ilGenerator.IsILGenerator) return new Error();
        var result = Visit(context.factor());
        if (!result.IsSuccess) return new Error();
        // Check whether top of the stack is 1 ( TRUE )
        // Check Whether the previous operation was successful
        // Functionally equivalent to Logical Not
        //
        // Case Top of Stack is 1 (TRUE )
        // ------------------------------
        // Top of Stack =>    [ 1 ]
        // LDC_I4 =>  [ 1 1 ] 
        // CEQ    =>  [ 1 ]
        // LDC_I4 =>  [ 1 0 ]
        // CEQ    =>  [ 0 ]
        //
        // Case Top of Stack is 0 (FALSE)
        // -----------------------------
        // Top of Stack =>    [ 0 ]
        // LDC_I4 =>  [ 0 1 ] 
        // CEQ    =>  [ 0 ]
        // LDC_I4 =>  [ 0 0 ]
        // CEQ    =>  [ 1 ]
        ilGenerator.AsILGenerator.Emit(OpCodes.Ldc_I4, 1);
        ilGenerator.AsILGenerator.Emit(OpCodes.Ceq);
        ilGenerator.AsILGenerator.Emit(OpCodes.Ldc_I4, 0);
        ilGenerator.AsILGenerator.Emit(OpCodes.Ceq);
        return new Success();
    }
    public override ErrorOrSuccess VisitFactor_CallExpr([NotNull] ARLangParser.Factor_CallExprContext context)
    {
        return base.VisitFactor_CallExpr(context);
    }
}
