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
    private readonly Dictionary<string, ARLangFunction> methodBuilders = [];
    private readonly Dictionary<string, Variable> variables = [];
    private Dictionary<string, Type> parameters = [];
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
    {   // 'FUNCTION' TYPE IDENTIFIER '(' arglist? ')' statements 'END'
        string functionName = context.IDENTIFIER().GetText();
        bool isMain = functionName == "Main";
        MethodAttributes methAttrs = MethodAttributes.Static;
        methAttrs = isMain ? methAttrs | MethodAttributes.Public : methAttrs | MethodAttributes.Private;
        var returnType = context.TYPE().GetText() switch { "NUMERIC" => isMain ? typeof(int) : typeof(double), "BOOLEAN" => typeof(bool), "STRING" => typeof(string), "VOID" => typeof(void), _ => typeof(void) };
        var returnTypeAsEValueType = context.TYPE().GetText() switch { "NUMERIC" => EValueType.Numeric, "BOOLEAN" => EValueType.Boolean, "STRING" => EValueType.String, "VOID" => EValueType.None, _ => EValueType.None };
        Type[]? parameterTypes = null;
        if (!isMain && context.arglist() is not null)
        {
            var argListResult = Visit(context.arglist());
            if (!argListResult.IsSuccessWithDic) return new Error();
            parameterTypes = argListResult.AsSuccessWithDic.Values.ToArray();
            parameters = argListResult.AsSuccessWithDic;
        }
        var functionBuilder = typeBuilder.DefineMethod(functionName, methAttrs, returnType, parameterTypes);
        methodBuilders[functionName] = new(functionBuilder, returnTypeAsEValueType);
        ilGenerator = functionBuilder.GetILGenerator();
        var result = Visit(context.statements());
        parameters.Clear();
        variables.Clear();
        if (isMain && (result.IsSuccess || result.IsSuccessWithType))
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
    public override CompilationResult VisitArglist([NotNull] ARLangParser.ArglistContext context)
    {
        // arglist: arg (',' arg)*;
        // arg: TYPE IDENTIFIER;
        var result = context.arg().Select(Visit).Select(x => x.AsSuccessWithArgs).ToList();
        return result.ToDictionary();
    }
    public override CompilationResult VisitArg([NotNull] ARLangParser.ArgContext context)
    {   // arg: TYPE IDENTIFIER;
        Type argType = context.TYPE().GetText() switch { "NUMERIC" => typeof(double), "BOOLEAN" => typeof(bool), "STRING" => typeof(string), "VOID" => typeof(void), _ => typeof(void) };
        return new KeyValuePair<string, Type>(context.IDENTIFIER().GetText(), argType);
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
    public override CompilationResult VisitPrintstatement([NotNull] ARLangParser.PrintstatementContext context)
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
        var wl = typeof(Console).GetMethod("Write", [type]);
        if (wl is null) throw new Exception();
        ilGenerator.AsILGenerator.Emit(OpCodes.Call, wl);
        return new Success();
    }
    public override CompilationResult VisitVardeclstatement([NotNull] ARLangParser.VardeclstatementContext context)
    {
        if (!ilGenerator.IsILGenerator) return new Error();
        string variableName = context.IDENTIFIER().GetText();
        var type = context.TYPE().GetText() switch { "NUMERIC" => typeof(double), "BOOLEAN" => typeof(bool), "STRING" => typeof(string), _ => typeof(void) };
        var localBuilder = ilGenerator.AsILGenerator.DeclareLocal(type);
        variables[variableName] = new(localBuilder, context.TYPE().GetText());
        return new Success();
    }
    public override CompilationResult VisitAssignmentstatement([NotNull] ARLangParser.AssignmentstatementContext context)
    {
        if (!ilGenerator.IsILGenerator) return new Error();
        string variableName = context.IDENTIFIER().GetText();
        var result = Visit(context.expr());
        if (!result.IsSuccessWithType) return new Error();
        ilGenerator.AsILGenerator.Emit(OpCodes.Stloc, variables[variableName].LocalBuilder);
        return new Success();
    }
    public override CompilationResult VisitIfstatement([NotNull] ARLangParser.IfstatementContext context)
    {
        // 'IF' expr 'THEN' statements ('ELSE' statements)? 'ENDIF'
        if (!ilGenerator.IsILGenerator) return new Error();
        Label trueLabel = ilGenerator.AsILGenerator.DefineLabel();
        Label falseLabel = ilGenerator.AsILGenerator.DefineLabel();
        var result = Visit(context.expr());
        if (!result.IsSuccessWithType) return new Error();
        ilGenerator.AsILGenerator.Emit(OpCodes.Ldc_I4, 1);
        ilGenerator.AsILGenerator.Emit(OpCodes.Ceq);
        ilGenerator.AsILGenerator.Emit(OpCodes.Brfalse, falseLabel);
        Visit(context.statements()[0]);
        ilGenerator.AsILGenerator.Emit(OpCodes.Br, trueLabel);
        ilGenerator.AsILGenerator.MarkLabel(falseLabel);
        if (context.statements().Length == 2)
        {
            Visit(context.statements()[1]);
        }
        ilGenerator.AsILGenerator.MarkLabel(trueLabel);
        return new Success();
    }
    public override CompilationResult VisitWhilestatement([NotNull] ARLangParser.WhilestatementContext context)
    {   // 'WHILE' expr statements 'WEND';
        if (!ilGenerator.IsILGenerator) return new Error();
        Label trueLabel = ilGenerator.AsILGenerator.DefineLabel();
        Label falseLabel = ilGenerator.AsILGenerator.DefineLabel();
        ilGenerator.AsILGenerator.MarkLabel(trueLabel);
        var result = Visit(context.expr());
        if (!result.IsSuccessWithType) return new Error();
        ilGenerator.AsILGenerator.Emit(OpCodes.Ldc_I4, 1);
        ilGenerator.AsILGenerator.Emit(OpCodes.Ceq);
        ilGenerator.AsILGenerator.Emit(OpCodes.Brfalse, falseLabel);
        Visit(context.statements());
        ilGenerator.AsILGenerator.Emit(OpCodes.Br, trueLabel);
        ilGenerator.AsILGenerator.MarkLabel(falseLabel);
        return new Success();
    }
    public override CompilationResult VisitReturnstatement([NotNull] ARLangParser.ReturnstatementContext context)
    {
        if (!ilGenerator.IsILGenerator) return new Error();
        if (context.expr() is null)
        {
            ilGenerator.AsILGenerator.Emit(OpCodes.Ret);
            return new Success();
        }
        CompilationResult result = Visit(context.expr());
        if (!result.IsSuccessWithType) return new Error();
        ilGenerator.AsILGenerator.Emit(OpCodes.Ret);
        return result;
    }
    public override CompilationResult VisitCallexpr([NotNull] ARLangParser.CallexprContext context)
    {   // callexpr: IDENTIFIER '(' actuals? ')';
        // actuals: expr (',' expr)*;
        if (!ilGenerator.IsILGenerator) return new Error();
        string functionName = context.IDENTIFIER().GetText();
        bool isSuccess = methodBuilders.TryGetValue(functionName, out ARLangFunction? func);
        if (!isSuccess)
        {
            return new Error();
        }
        if (context.actuals() is not null)
        {
            Visit(context.actuals());
        }
        ilGenerator.AsILGenerator.Emit(OpCodes.Call, func!.MethodBuilder);
        return new Success<EValueType>(func!.ReturnType);
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
            case ">=":
                {
                    ilGenerator.AsILGenerator.Emit(OpCodes.Clt);
                    ilGenerator.AsILGenerator.Emit(OpCodes.Ldc_I4, 0);
                    ilGenerator.AsILGenerator.Emit(OpCodes.Ceq);
                    break;
                }
            case "<=":
                {
                    ilGenerator.AsILGenerator.Emit(OpCodes.Cgt);
                    ilGenerator.AsILGenerator.Emit(OpCodes.Ldc_I4, 0);
                    ilGenerator.AsILGenerator.Emit(OpCodes.Ceq);
                    break;
                }
            case "<>":
                {
                    ilGenerator.AsILGenerator.Emit(OpCodes.Ceq);
                    ilGenerator.AsILGenerator.Emit(OpCodes.Ldc_I4, 0);
                    ilGenerator.AsILGenerator.Emit(OpCodes.Ceq);
                    break;
                }
            case "==": { ilGenerator.AsILGenerator.Emit(OpCodes.Ceq); break; }
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
        if (!ilGenerator.IsILGenerator) return new Error();
        bool isSuccess1 = variables.TryGetValue(context.IDENTIFIER().GetText(), out Variable? variable);
        if (isSuccess1)
        {
            ilGenerator.AsILGenerator.Emit(OpCodes.Ldloc, variable!.LocalBuilder);
            string typeStr = variable.Type;
            EValueType type = typeStr switch { "NUMERIC" => EValueType.Numeric, "BOOLEAN" => EValueType.Boolean, "STRING" => EValueType.String, _ => EValueType.None };
            return new Success<EValueType>(type);
        }

        try
        {
            var parameterWithIndex = parameters.Index().First(x => x.Item.Key == context.IDENTIFIER().GetText());
            ilGenerator.AsILGenerator.Emit(OpCodes.Ldarg, parameterWithIndex.Index);
            Type doubleType = typeof(double);
            if (typeof(double) == parameterWithIndex.Item.Value)
            {
                return new Success<EValueType>(EValueType.Numeric);
            }
            else if (typeof(string) == parameterWithIndex.Item.Value)
            {
                return new Success<EValueType>(EValueType.String);
            }
            else if (typeof(bool) == parameterWithIndex.Item.Value)
            {
                return new Success<EValueType>(EValueType.Boolean);
            }
            else
            {
                return new Error();
            }
        }
        catch
        {
            return new Error();
        }
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
        return Visit(context.callexpr());
    }
}

public record Variable(LocalBuilder LocalBuilder, string Type);
public record ARLangFunction(MethodBuilder MethodBuilder, EValueType ReturnType);