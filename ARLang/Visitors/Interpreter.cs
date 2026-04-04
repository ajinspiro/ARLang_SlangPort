using Antlr4.Runtime.Misc;
using ARLang.Internals;
using OneOf.Types;

namespace ARLang.Visitors;

public class Interpreter(RuntimeContext runtimeContext) : ARLangBaseVisitor<ErrorOrSuccess>
{
    private readonly RuntimeContext runtimeContext = runtimeContext;

    public override ErrorOrSuccess VisitModule([NotNull] ARLangParser.ModuleContext context)
    {
        var list = context.procedure().Select(Visit).ToList();
        var mainResult = list.FirstOrDefault(x => x.IsSuccessWithValue);
        return mainResult is not null ? mainResult.AsSuccessWithValue : new Error();
    }

    public override ErrorOrSuccess VisitProcedure([NotNull] ARLangParser.ProcedureContext context)
    {
        string functionName = context.IDENTIFIER().GetText();
        if (functionName == "MAIN")
        {
            Scope mainScope = new("MAIN");
            mainScope.ParentScope = runtimeContext.Scope;
            runtimeContext.Scope = mainScope;
            var mainResult = Visit(context.statements());
            runtimeContext.Scope = mainScope.ParentScope!;
            return mainResult;
        }
        string returnType = context.TYPE().GetText();
        Dictionary<string, string> parameters;
        if (context.arglist() is null)
        {
            parameters = [];
        }
        else
        {
            var visitResult = Visit(context.arglist());
            if (visitResult.IsSuccessWithDictionary)
            {
                parameters = visitResult.AsSuccessWithDictionary;
            }
            else
            {
                return new Error();
            }
        }
        var body = context.statements();
        runtimeContext.FunctionDefinitions.Add(new(functionName, parameters, body, returnType));

        return new Success();
    }

    public override ErrorOrSuccess VisitArglist([NotNull] ARLangParser.ArglistContext context)
    {
        var visitedArgs = context.arg().Select(Visit).ToList();
        if (visitedArgs.All(x => !x.IsSuccessWithKeyValuePair)) return new Error();
        return visitedArgs.Select(x => x.AsSuccessWithKeyValuePair).ToDictionary();
    }

    public override ErrorOrSuccess VisitArg([NotNull] ARLangParser.ArgContext context)
    {
        return new KeyValuePair<string, string>(context.TYPE().GetText(), context.IDENTIFIER().GetText());
    }

    public override ErrorOrSuccess VisitIfstatement([NotNull] ARLangParser.IfstatementContext context)
    {
        Scope ifScope = new("IF");
        ifScope.ParentScope = runtimeContext.Scope;
        runtimeContext.Scope = ifScope;

        var visitedExprResult = Visit(context.expr());
        if (!(visitedExprResult.IsSuccessWithValue && visitedExprResult.AsSuccessWithValue.IsBoolean))
        {
            return new Error();
        }
        if (visitedExprResult.AsSuccessWithValue.AsBoolean)
        {
            var mainResult = Visit(context.statements()[0]);
            runtimeContext.Scope = ifScope.ParentScope!;
            return mainResult;
        }
        else
        {
            var mainResult = Visit(context.statements()[1]);
            runtimeContext.Scope = ifScope.ParentScope!;
            return mainResult;
        }
    }

    public override ErrorOrSuccess VisitWhilestatement([NotNull] ARLangParser.WhilestatementContext context)
    {
        while (true)
        {
            var visitedExprResult = Visit(context.expr());
            if (!(visitedExprResult.IsSuccessWithValue && visitedExprResult.AsSuccessWithValue.IsBoolean))
            {
                return new Error();
            }
            if (!visitedExprResult.AsSuccessWithValue.AsBoolean)
            {
                return new Success();
            }
            Visit(context.statements());
        }
    }

    public override ErrorOrSuccess VisitReturnstatement([NotNull] ARLangParser.ReturnstatementContext context)
    {
        return Visit(context.expr());
    }

    public override ErrorOrSuccess VisitExpr([NotNull] ARLangParser.ExprContext context)
    {
        return Visit(context.bexpr());
    }

    public override ErrorOrSuccess VisitCallexpr([NotNull] ARLangParser.CallexprContext context)
    {
        string nameOfFunctionToCall = context.IDENTIFIER().GetText();
        var fnDef = runtimeContext.FunctionDefinitions.FirstOrDefault(x => x.Name == nameOfFunctionToCall);
        if (fnDef is null)
        {
            return new Error();
        }
        var visitedActualsResult = context.actuals() is null ? Array.Empty<Value>() : Visit(context.actuals());
        if (!(visitedActualsResult.IsSuccessWithValueArray || visitedActualsResult.IsSuccess)) return new Error();
        // TODO: Type check

        if (fnDef.Params.Count != visitedActualsResult.AsSuccessWithValueArray.Length)
        {
            // argument count != parameter count
            return new Error();
        }
        Scope innerScope = new("Inner-" + nameOfFunctionToCall);
        var parameters = fnDef.Params.ToList();
        for (int i = 0; i < parameters.Count; i++)
        {
            string parameterType = parameters[i].Key;
            string parameterName = parameters[i].Value;
            Value parameterValue = visitedActualsResult.AsSuccessWithValueArray[i];
            innerScope.Declare(parameterName, parameterType);
            innerScope.Assign(parameterName, parameterValue);
        }
        Scope parentScopeRef = runtimeContext.Scope;
        innerScope.ParentScope = parentScopeRef;
        runtimeContext.Scope = innerScope;

        var result = Visit(fnDef.Body);
        runtimeContext.Scope = parentScopeRef;
        return result;
    }

    public override ErrorOrSuccess VisitActuals([NotNull] ARLangParser.ActualsContext context)
    {
        var visitedArguments = context.expr().Select(Visit);
        if (!visitedArguments.All(x => x.IsSuccessWithValue))
        {
            return new Error();
        }
        return visitedArguments.Select(x => x.AsSuccessWithValue).ToArray();
    }

    public override ErrorOrSuccess VisitStatements([NotNull] ARLangParser.StatementsContext context)
    {
        ErrorOrSuccess last = new Success();
        foreach (var statement in context.statement())
        {
            last = Visit(statement);
            if (last.IsSuccessWithValue) return last; // RETURN statement — stop executing
        }
        return last;
    }

    public override ErrorOrSuccess VisitStatement([NotNull] ARLangParser.StatementContext context)
    {
        if (context.vardeclstatement() is not null)
        {
            return Visit(context.vardeclstatement());
        }
        else if (context.printstatement() is not null)
        {
            return Visit(context.printstatement());
        }
        else if (context.printlinestatement() is not null)
        {
            return Visit(context.printlinestatement());
        }
        else if (context.assignmentstatement() is not null)
        {
            return Visit(context.assignmentstatement());
        }
        else if (context.callstatement() is not null)
        {
            return Visit(context.callstatement());
        }
        else if (context.ifstatement() is not null)
        {
            return Visit(context.ifstatement());
        }
        else if (context.whilestatement() is not null)
        {
            return Visit(context.whilestatement());
        }
        else if (context.returnstatement() is not null)
        {
            return Visit(context.returnstatement());
        }
        else
        {
            return new Error();
        }
    }

    public override ErrorOrSuccess VisitAssignmentstatement([NotNull] ARLangParser.AssignmentstatementContext context)
    {
        var exprResult = Visit(context.expr());
        if (!exprResult.IsSuccessWithValue) return new Error();
        runtimeContext.Scope.Assign(context.IDENTIFIER().GetText(), exprResult.AsSuccessWithValue);
        return new Success();
    }

    public override ErrorOrSuccess VisitVardeclstatement([NotNull] ARLangParser.VardeclstatementContext context)
    {
        runtimeContext.Scope.Declare(context.IDENTIFIER().GetText(), context.TYPE().GetText());
        return new Success();
    }

    public override ErrorOrSuccess VisitPrintlinestatement([NotNull] ARLangParser.PrintlinestatementContext context)
    {
        var result = Visit(context.expr());
        Console.WriteLine(result.Match(
            error => "Error occured while evaluating expression",
            success => "Expression evaluation succeded without producing any results",
            successWithValue =>
            {
                return successWithValue.Match(
                    d => d.ToString(), s => s, b => b.ToString(), n => "<None>"
                );
            },
            x => "Error occured while evaluating expression",
            x => "Error occured while evaluating expression",
            x => "Error occured while evaluating expression"
        ));
        return new Success();
    }

    public override ErrorOrSuccess VisitPrintstatement([NotNull] ARLangParser.PrintstatementContext context)
    {
        var result = Visit(context.expr());
        Console.Write(result.Match(
            error => "Error occured while evaluating expression",
            success => "Expression evaluation succeded without producing any results",
            successWithValue =>
            {
                return successWithValue.Match(
                    d => d.ToString(), s => s, b => b.ToString(), n => "<None>"
                );
            },
            x => "Error occured while evaluating expression",
            x => "Error occured while evaluating expression",
            x => "Error occured while evaluating expression"
        ));
        return new Success();
    }

    public override ErrorOrSuccess VisitBexpr([NotNull] ARLangParser.BexprContext context)
    {
        var left = Visit(context.lexpr()[0]);
        if (!left.IsSuccessWithValue) return left;

        var ops = context.LOGICOP();
        if (ops.Length == 0)
        {
            return left;
        }
        var lv = left.AsSuccessWithValue.AsBoolean;
        for (int i = 0; i < ops.Length; i++)
        {
            var right = Visit(context.lexpr()[i + 1]);
            if (!right.IsSuccessWithValue) return right;
            var rv = right.AsSuccessWithValue.AsBoolean;
            lv = ops[i].GetText() switch
            {
                "&&" => lv && rv,
                "||" => lv || rv,
                _ => throw new InvalidProgramException()
            };
        }
        return new Value(lv);
    }

    public override ErrorOrSuccess VisitLexpr([NotNull] ARLangParser.LexprContext context)
    {
        var left = Visit(context.rexpr()[0]);
        if (!left.IsSuccessWithValue) return left;

        var op = context.RELOP();

        if (op is null)
        {
            return left;
        }

        var lv = left.AsSuccessWithValue.AsNumeric;
        var right = Visit(context.rexpr()[1]);
        if (!right.IsSuccessWithValue) return right;

        var rv = right.AsSuccessWithValue.AsNumeric;

        bool comparison = op.GetText() switch
        {
            ">" => lv > rv,
            "<" => lv < rv,
            ">=" => lv >= rv,
            "<=" => lv <= rv,
            "==" => lv == rv,
            "<>" => lv != rv,
            _ => throw new InvalidProgramException()
        };

        return new Value(comparison);
    }

    public override ErrorOrSuccess VisitRexpr([NotNull] ARLangParser.RexprContext context)
    {
        var result = Visit(context.term()[0]);
        if (!result.IsSuccessWithValue) return result;

        var ops = context.ADDOP();
        if (ops.Length == 0) return result;

        for (int i = 0; i < ops.Length; i++)
        {
            var right = Visit(context.term()[i + 1]);
            if (!right.IsSuccessWithValue) return right;

            var lv = result.AsSuccessWithValue.AsNumeric;
            var rv = right.AsSuccessWithValue.AsNumeric;

            result = ops[i].GetText() switch
            {
                "+" => new Value(lv + rv),
                "-" => new Value(lv - rv),
                _ => throw new InvalidProgramException()
            };
        }

        return result;
    }

    public override ErrorOrSuccess VisitTerm([NotNull] ARLangParser.TermContext context)
    {
        var result = Visit(context.factor()[0]);
        if (!result.IsSuccessWithValue) return result;

        var ops = context.MULOP();
        if (ops.Length == 0) return result;

        for (int i = 0; i < ops.Length; i++)
        {
            var right = Visit(context.factor()[i + 1]);
            if (!right.IsSuccessWithValue) return right;

            var lv = result.AsSuccessWithValue.AsNumeric;
            var rv = right.AsSuccessWithValue.AsNumeric;

            result = ops[i].GetText() switch
            {
                "*" => new Value(lv * rv),
                "/" => new Value(lv / rv),
                _ => throw new InvalidProgramException()
            };
        }

        return result;
    }

    public override ErrorOrSuccess VisitFactor_Number([NotNull] ARLangParser.Factor_NumberContext context)
    {
        return new Value(decimal.Parse(context.NUMBER().GetText()));
    }

    public override ErrorOrSuccess VisitFactor_String([NotNull] ARLangParser.Factor_StringContext context)
    {
        string value = context.STRING().GetText();
        return new Value(value.Substring(1, value.Length - 2));
    }

    public override ErrorOrSuccess VisitFactor_BoolTrue([NotNull] ARLangParser.Factor_BoolTrueContext context)
    {
        return new Value(true);
    }

    public override ErrorOrSuccess VisitFactor_BoolFalse([NotNull] ARLangParser.Factor_BoolFalseContext context)
    {
        return new Value(false);
    }

    public override ErrorOrSuccess VisitFactor_IDENTIFIER([NotNull] ARLangParser.Factor_IDENTIFIERContext context)
    {
        var variable = runtimeContext.Scope.Resolve(context.IDENTIFIER().GetText());
        return variable.Match<ErrorOrSuccess>(
            error => error,
            success => new Error(),
            successWithValue => successWithValue,
            x => new Error(),
            x => new Error(),
            x => new Error()
        );
    }

    public override ErrorOrSuccess VisitFactor_NestedExpr([NotNull] ARLangParser.Factor_NestedExprContext context)
    {
        return Visit(context.expr());
    }

    public override ErrorOrSuccess VisitFactor_UnaryFactor([NotNull] ARLangParser.Factor_UnaryFactorContext context)
    {
        var right = Visit(context.factor());
        if (!right.IsSuccessWithValue)
        {
            throw new InvalidProgramException();
        }
        if (right.AsSuccessWithValue.IsNumeric && context.ADDOP().GetText() == "+")
        {
            return new Value(+right.AsSuccessWithValue.AsNumeric);
        }
        if (right.AsSuccessWithValue.IsNumeric && context.ADDOP().GetText() == "-")
        {
            return new Value(-right.AsSuccessWithValue.AsNumeric);
        }
        throw new InvalidProgramException();
    }

    public override ErrorOrSuccess VisitFactor_BoolNotOperation([NotNull] ARLangParser.Factor_BoolNotOperationContext context)
    {
        var right = Visit(context.factor());
        return new Value(!right.AsSuccessWithValue.AsBoolean);
    }

    public override ErrorOrSuccess VisitFactor_CallExpr([NotNull] ARLangParser.Factor_CallExprContext context)
    {
        return Visit(context.callexpr());
    }
}
