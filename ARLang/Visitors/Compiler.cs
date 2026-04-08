using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Antlr4.Runtime.Misc;
using ARLang.Internals;
using OneOf;
using OneOf.Types;

namespace ARLang.Visitors;

public class Compiler : ARLangBaseVisitor<CompilationResult>
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
    public override CompilationResult VisitModule([NotNull] ARLangParser.ModuleContext context)
    {
        return context.procedure().Select(Visit).ToList().Last();
    }
    public override CompilationResult VisitProcedure([NotNull] ARLangParser.ProcedureContext context)
    {
        string functionName = context.IDENTIFIER().GetText();
        bool isMain = functionName == "Main";
        MethodAttributes methAttrs = MethodAttributes.Static;
        methAttrs = isMain ? methAttrs | MethodAttributes.Public : methAttrs | MethodAttributes.Private;
        var returnType = context.TYPE().GetText() switch { "NUMERIC" => typeof(double), "BOOLEAN" => typeof(bool), "STRING" => typeof(string), _ => typeof(void) };
        var functionBuilder = typeBuilder.DefineMethod(functionName, methAttrs, returnType, null);
        methodBuilders[functionName] = functionBuilder;
        ilGenerator = functionBuilder.GetILGenerator();
        var result = Visit(context.statements());

        if (result.IsSuccessWithType)
        {
            typeBuilder.CreateType();
            // 🔥 KEY PART: Generate metadata instead of Save()
            var metadata = assemblyBuilder.GenerateMetadata(out var ilStream, out var fieldData);

            // 🔥 Build PE with entry point
            var peBuilder = new ManagedPEBuilder(
                header: new PEHeaderBuilder(
                    imageCharacteristics: Characteristics.ExecutableImage,
                    subsystem: Subsystem.WindowsCui // Console app
                ),
                metadataRootBuilder: new MetadataRootBuilder(metadata),
                ilStream: ilStream,
                mappedFieldData: fieldData,
                entryPoint: MetadataTokens.MethodDefinitionHandle(functionBuilder.MetadataToken)
            );

            var peBlob = new BlobBuilder();
            peBuilder.Serialize(peBlob);

            using var fs = new FileStream("DynamicTest.dll", FileMode.Create, FileAccess.Write);
            peBlob.WriteContentTo(fs);
            return new Success();
        }
        return new Error();
    }
    public override CompilationResult VisitStatements([NotNull] ARLangParser.StatementsContext context)
    {
        return context.statement().Select(Visit).ToList().Last();
    }
    public override CompilationResult VisitPrintlinestatement([NotNull] ARLangParser.PrintlinestatementContext context)
    {
        if (!ilGenerator.IsILGenerator) return new Error();
        var result = Visit(context.expr());
        if (!result.IsSuccessWithType) return new Error();
        var type = result.AsSuccessWithType switch
        {
            EValueType.Numeric => typeof(double),
            EValueType.String => typeof(string),
            EValueType.Boolean => typeof(bool),
            _ => typeof(void)
        };
        var wl = typeof(Console).GetMethod("WriteLine", [type]);
        if (wl is null) throw new Exception();
        ilGenerator.AsILGenerator.Emit(OpCodes.Call, wl);
        return new Success();
    }
    public override CompilationResult VisitReturnstatement([NotNull] ARLangParser.ReturnstatementContext context)
    {
        if (!ilGenerator.IsILGenerator) return new Error();
        var result = Visit(context.expr());
        if (!result.IsSuccessWithType) return new Error();
        ilGenerator.AsILGenerator.Emit(OpCodes.Ret);
        return result;
    }
    public override CompilationResult VisitExpr([NotNull] ARLangParser.ExprContext context)
    {   // expr: bexpr;
        if (!ilGenerator.IsILGenerator) return new Error();
        return Visit(context.bexpr());
    }
    public override CompilationResult VisitBexpr([NotNull] ARLangParser.BexprContext context)
    {   // bexpr: lexpr (LOGICOP lexpr)*;
        if (!ilGenerator.IsILGenerator) return new Error();
        var list = context.lexpr().Select(Visit).ToList();
        if (context.LOGICOP() is { Length: 0 })
        {
            return list[0];
        }
        context.LOGICOP().Reverse().ToList().ForEach(x =>
        {
            switch (x.GetText())
            {
                case "&&": { ilGenerator.AsILGenerator.Emit(OpCodes.And); break; }
                case "||": { ilGenerator.AsILGenerator.Emit(OpCodes.Or); break; }
                default: throw new NotImplementedException();
            }
        });
        return new Success<EValueType>(EValueType.Boolean);
    }
    public override CompilationResult VisitLexpr([NotNull] ARLangParser.LexprContext context)
    {   // lexpr: rexpr (RELOP rexpr)?;
        // RELOP: '>' | '<' | '>=' | '<=' | '<>' | '==';
        if (!ilGenerator.IsILGenerator) return new Error();
        var list = context.rexpr().Select(Visit).ToList();
        if (context.RELOP() is null)
        {
            return list[0];
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
            default: { throw new Exception(); }
        }

        return new Success<EValueType>(EValueType.Boolean);
    }
    public override CompilationResult VisitRexpr([NotNull] ARLangParser.RexprContext context)
    {   // rexpr: term (ADDOP term)*;
        if (!ilGenerator.IsILGenerator) return new Error();
        var list = context.term().Select(Visit).ToList();
        if (context.ADDOP().Length == 0)
        {
            return list[0];
        }
        context.ADDOP().Reverse().ToList().ForEach(op =>
        {
            switch (op.GetText())
            {
                case "+": { ilGenerator.AsILGenerator.Emit(OpCodes.Add); break; }
                case "-": { ilGenerator.AsILGenerator.Emit(OpCodes.Sub); break; }
                default: throw new Exception();
            }
            ;
        });
        return new Success<EValueType>(EValueType.Numeric);
    }
    public override CompilationResult VisitTerm([NotNull] ARLangParser.TermContext context)
    {   //term: factor (MULOP factor)*;
        if (!ilGenerator.IsILGenerator) return new Error();
        var list = context.factor().Select(Visit).ToList();
        if (context.MULOP().Length == 0)
        {
            return list[0];
        }
        // 1+2*3 => 1 2 3 * +
        context.MULOP().Reverse().ToList().ForEach(op =>
        {
            switch (op.GetText())
            {
                case "*": { ilGenerator.AsILGenerator.Emit(OpCodes.Mul); break; }
                case "/": { ilGenerator.AsILGenerator.Emit(OpCodes.Div); break; }
                default: throw new Exception();
            }
        });
        return new Success<EValueType>(EValueType.Numeric);
    }
    public override CompilationResult VisitFactor_Number([NotNull] ARLangParser.Factor_NumberContext context)
    {
        if (!ilGenerator.IsILGenerator) return new Error();
        ilGenerator.AsILGenerator.Emit(OpCodes.Ldc_R8, double.Parse(context.NUMBER().GetText()));
        return new Success<EValueType>(EValueType.Numeric);
    }
    public override CompilationResult VisitFactor_String([NotNull] ARLangParser.Factor_StringContext context)
    {
        if (!ilGenerator.IsILGenerator) return new Error();
        ilGenerator.AsILGenerator.Emit(OpCodes.Ldstr, context.STRING().GetText());
        return new Success<EValueType>(EValueType.String);
    }
    public override CompilationResult VisitFactor_BoolTrue([NotNull] ARLangParser.Factor_BoolTrueContext context)
    {
        if (!ilGenerator.IsILGenerator) return new Error();
        ilGenerator.AsILGenerator.Emit(OpCodes.Ldc_I4, 1);
        return new Success<EValueType>(EValueType.Boolean);
    }
    public override CompilationResult VisitFactor_BoolFalse([NotNull] ARLangParser.Factor_BoolFalseContext context)
    {
        if (!ilGenerator.IsILGenerator) return new Error();
        ilGenerator.AsILGenerator.Emit(OpCodes.Ldc_I4, 0);
        return new Success<EValueType>(EValueType.Boolean);
    }
    public override CompilationResult VisitFactor_IDENTIFIER([NotNull] ARLangParser.Factor_IDENTIFIERContext context)
    {
        return base.VisitFactor_IDENTIFIER(context);
    }
    public override CompilationResult VisitFactor_NestedExpr([NotNull] ARLangParser.Factor_NestedExprContext context)
    {
        if (!ilGenerator.IsILGenerator) return new Error();
        return Visit(context.expr());
    }
    public override CompilationResult VisitFactor_UnaryFactor([NotNull] ARLangParser.Factor_UnaryFactorContext context)
    {
        if (!ilGenerator.IsILGenerator) return new Error();
        var result = Visit(context.factor());
        if (!result.IsSuccess) return new Error();
        ilGenerator.AsILGenerator.Emit(OpCodes.Neg);
        return new Success<EValueType>(EValueType.Numeric);
    }
    public override CompilationResult VisitFactor_BoolNotOperation([NotNull] ARLangParser.Factor_BoolNotOperationContext context)
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
        return new Success<EValueType>(EValueType.Boolean);
    }
    public override CompilationResult VisitFactor_CallExpr([NotNull] ARLangParser.Factor_CallExprContext context)
    {
        return base.VisitFactor_CallExpr(context);
    }
}
