using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Compiler.Diagnostics;
using Compiler.Lowering;
using Compiler.Symbols;
using Compiler.Syntax;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class Binder : IDiagnostable
    {
        private readonly DiagnosticBag diagnostics;
        private readonly FunctionSymbol function;
        private readonly bool isScript;
        private readonly Stack<(BoundLabel breakLabel, BoundLabel continueLabel)> labelStack;
        private int labelCounter;
        private BoundScope scope;

        public IEnumerable<Diagnostic> GetDiagnostics() => diagnostics;

        private Binder(BoundScope parentScope, bool isScript, FunctionSymbol function)
        {
            this.isScript = isScript;
            this.function = function;
            this.labelStack = new Stack<(BoundLabel breakLabel, BoundLabel continueLabel)>();
            this.scope = new BoundScope(parentScope);
            this.diagnostics = new DiagnosticBag();

            if (function != null)
                foreach (var param in function.Parameters)
                    scope.TryDeclareVariable(param);
        }

        public static BoundProgram BindProgram(BoundProgram previous, bool isScript, CompilationUnitSyntax unit)
        {
            var parentScope = CreateBoundScopes(previous);
            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
            var functionSyntax = unit.Members.OfType<FunctionDeclarationSyntax>();
            var statementSyntax = unit.Members.OfType<GlobalStatementSynatx>();
            var globalBinder = new Binder(parentScope, isScript, function: null);

            foreach (var func in functionSyntax)
                globalBinder.DeclareFunction(func);

            var globalStatementBuilder = ImmutableArray.CreateBuilder<BoundStatement>();
            foreach (var stmt in statementSyntax)
            {
                var boundStmt = globalBinder.BindStatement(stmt.Statement);
                if (boundStmt.Kind == BoundNodeKind.BoundInvalidStatement)
                    continue;
                globalStatementBuilder.Add(boundStmt);
            }

            diagnostics.AddRange(globalBinder.GetDiagnostics());

            var globalBlockStatement = Lowerer.Lower(new BoundBlockStatement(globalStatementBuilder.ToImmutable()));
            var declaredVariables = globalBinder.scope.GetDeclaredVariables();
            var declaredFunctions = globalBinder.scope.GetDeclaredFunctions();

            var currentScope = globalBinder.scope;
            var functions = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();

            foreach (var symbol in declaredFunctions)
            {
                var binder = new Binder(currentScope, isScript, symbol);
                var body = binder.BindBlockStatmentSyntax(symbol.Syntax.Body);

                if (!(body.Kind == BoundNodeKind.BoundInvalidStatement))
                {
                    var loweredBody = Lowerer.Lower(body);
                    functions.Add(symbol, loweredBody);
                }

                diagnostics.AddRange(binder.GetDiagnostics());
            }


            FunctionSymbol mainFunc = null;

            if (!isScript && !globalBinder.scope.TryLookUpFunction("main", out mainFunc))
                diagnostics.Add(new Diagnostic(ErrorKind.IdentifierError, "Program doesn't define main fuction.", TextSpan.Undefined));

            if (mainFunc != null && mainFunc.Parameters.Length > 0)
                diagnostics.Add(new Diagnostic(ErrorKind.IdentifierError, "Main function cannot have arguments.", mainFunc.Syntax.Parameters.Span));

            if (mainFunc != null && mainFunc.ReturnType != TypeSymbol.Void)
                diagnostics.Add(new Diagnostic(ErrorKind.IdentifierError, "Main function must return void.", mainFunc.Syntax.ReturnType.Span));

            return new BoundProgram(previous, globalBlockStatement, declaredVariables, mainFunc, functions.ToImmutable(), diagnostics.ToImmutable());
        }

        private static BoundScope CreateBoundScopes(BoundProgram previous)
        {
            var stack = new Stack<BoundProgram>();


            while (previous != null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }

            BoundScope current = CreateRootScope();

            while (stack.Count > 0)
            {
                var global = stack.Pop();
                var scope = new BoundScope(current);
                foreach (var variable in global.GlobalVariables)
                    scope.TryDeclareVariable(variable);

                foreach (var function in global.Functions.Keys)
                    scope.TryDeclareFunction(function);

                current = scope;
            }

            return current;
        }

        private static BoundScope CreateRootScope()
        {
            var scope = new BoundScope(null);
            foreach (var b in BuiltInFunctions.GetAll())
                scope.TryDeclareFunction(b);
            return scope;
        }

        private void DeclareFunction(FunctionDeclarationSyntax func)
        {
            var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
            var seenParameters = new HashSet<string>();

            foreach (var parameterSyntax in func.Parameters)
            {
                var type = BindFacts.GetTypeSymbol(parameterSyntax.TypeClause.TypeToken.Kind);
                var name = parameterSyntax.Identifier.Value.ToString();
                if (!seenParameters.Add(name))
                    diagnostics.ReportIdentifierError(ErrorMessage.DuplicatedParameters, parameterSyntax.Span, name);
                else parameters.Add(new ParameterSymbol(name, type));
            }

            TypeSymbol returnType;
            if (func.ReturnType.IsExplicit)
                returnType = BindFacts.GetTypeSymbol(func.ReturnType.TypeToken.Kind);
            else
                returnType = TypeSymbol.Void;
            // TODO infer return type

            var symbol = new FunctionSymbol(func.Identifier.Value.ToString(), parameters.ToImmutable(), returnType, func);

            if (!scope.TryDeclareFunction(symbol))
                diagnostics.ReportIdentifierError(ErrorMessage.FunctionAlreadyDeclared, func.Identifier.Span, func.Identifier.Value);
        }

        private BoundStatement BindStatement(StatementSyntax syntax)
        {
            if (!syntax.IsValid)
                return new BoundInvalidStatement();

            switch (syntax.Kind)
            {
                case SyntaxNodeKind.BlockStatmentSyntax:
                    return BindBlockStatmentSyntax((BlockStatmentSyntax)syntax);
                case SyntaxNodeKind.ExpressionStatementSyntax:
                    return BindExpressionStatementSyntax((ExpressionStatementSyntax)syntax);
                case SyntaxNodeKind.VariableDeclarationStatementSyntax:
                    return BindVariableDeclarationStatementSyntax((VariableDeclarationStatementSyntax)syntax);
                case SyntaxNodeKind.IfStatementSyntax:
                    return BindIfStatementSyntax((IfStatementSyntax)syntax);
                case SyntaxNodeKind.WhileStatementSyntax:
                    return BindWhileStatementSyntax((WhileStatementSyntax)syntax);
                case SyntaxNodeKind.ForStatementSyntax:
                    return BindForStatementSyntax((ForStatementSyntax)syntax);
                case SyntaxNodeKind.DoWhileStatementSyntax:
                    return BindDoWhileStatementSyntax((DoWhileStatementSyntax)syntax);
                case SyntaxNodeKind.BreakStatementSyntax:
                    return BindBreakStatementSyntax((BreakStatementSyntax)syntax);
                case SyntaxNodeKind.ContinueStatementSyntax:
                    return BindContinueStatementSyntax((ContinueStatementSyntax)syntax);
                default: throw new Exception($"Unexpected SyntaxKind <{syntax.Kind}>");
            }
        }

        private BoundStatement BindBlockStatmentSyntax(BlockStatmentSyntax syntax)
        {
            var builder = ImmutableArray.CreateBuilder<BoundStatement>();
            scope = new BoundScope(scope);

            foreach (var stmt in syntax.Statements)
            {
                var bound = BindStatement(stmt);
                if (bound.Kind == BoundNodeKind.BoundInvalidStatement)
                {
                    scope = scope.Parent;
                    return new BoundInvalidStatement();
                }
                builder.Add(bound);
            }

            scope = scope.Parent;

            return new BoundBlockStatement(builder.ToImmutable());
        }

        private BoundStatement BindExpressionStatementSyntax(ExpressionStatementSyntax syntax)
        {
            var expr = BindExpression(syntax.Expression, true);
            if (expr.Kind == BoundNodeKind.BoundInvalidExpression)
                return new BoundInvalidStatement();

            return new BoundExpressionStatement(expr);
        }

        private BoundStatement BindVariableDeclarationStatementSyntax(VariableDeclarationStatementSyntax syntax)
        {
            TypeSymbol type;
            BoundExpression expr;

            if (syntax.TypeClause.IsExplicit)
            {
                type = BindFacts.GetTypeSymbol(syntax.TypeClause.TypeToken.Kind);
                expr = CheckTypeAndConversion(type, syntax.Expression);
            }
            else
            {
                expr = BindExpression(syntax.Expression, canBeVoid: false);
                type = expr.ResultType;
            }

            if (expr.Kind == BoundNodeKind.BoundInvalidExpression)
                return new BoundInvalidStatement();


            VariableSymbol variable;
            if (function == null)
                variable = new GlobalVariableSymbol(syntax.Identifier.Value.ToString(), type, syntax.VarKeyword.Kind == SyntaxTokenKind.ConstKeyword ? VariableModifier.Constant : VariableModifier.None);
            else
                variable = new LocalVariableSymbol(syntax.Identifier.Value.ToString(), type);

            if (!scope.TryDeclareVariable(variable))
            {
                diagnostics.ReportIdentifierError(ErrorMessage.VariableAlreadyDeclared, syntax.Identifier.Span, variable.Name);
                return new BoundInvalidStatement();
            }
            return new BoundVariableDeclarationStatement(variable, expr);
        }

        private BoundStatement BindIfStatementSyntax(IfStatementSyntax syntax)
        {
            var condition = CheckTypeAndConversion(TypeSymbol.Bool, syntax.Condition);

            if (condition.Kind == BoundNodeKind.BoundInvalidExpression)
                return new BoundInvalidStatement();

            var stmt = BindStatement(syntax.Body);

            if (stmt.Kind == BoundNodeKind.BoundInvalidStatement)
                return new BoundInvalidStatement();

            var elseStmt = syntax.ElseStatement == null ? null : BindStatement(syntax.ElseStatement.Body);

            if (elseStmt != null && elseStmt.Kind == BoundNodeKind.BoundInvalidStatement)
                return new BoundInvalidStatement();

            return new BoundIfStatement(condition, stmt, elseStmt);
        }

        private BoundStatement BindWhileStatementSyntax(WhileStatementSyntax syntax)
        {
            var condition = CheckTypeAndConversion(TypeSymbol.Bool, syntax.Condition);

            if (condition.Kind == BoundNodeKind.BoundInvalidExpression)
                return new BoundInvalidStatement();

            var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);

            if (body.Kind == BoundNodeKind.BoundInvalidStatement)
                return new BoundInvalidStatement();

            return new BoundWhileStatement(condition, body, breakLabel, continueLabel);
        }

        private BoundStatement BindForStatementSyntax(ForStatementSyntax syntax)
        {
            var variableDecl = BindStatement(syntax.VariableDeclaration);

            if (variableDecl.Kind == BoundNodeKind.BoundInvalidStatement)
                return new BoundInvalidStatement();

            var condition = CheckTypeAndConversion(TypeSymbol.Bool, syntax.Condition);

            if (condition.Kind == BoundNodeKind.BoundInvalidExpression)
                return new BoundInvalidStatement();

            var increment = BindExpression(syntax.Increment);

            if (increment.Kind == BoundNodeKind.BoundInvalidExpression)
                return new BoundInvalidStatement();

            var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);

            if (body.Kind == BoundNodeKind.BoundInvalidStatement)
                return new BoundInvalidStatement();

            return new BoundForStatement(variableDecl, condition, increment, body, breakLabel, continueLabel);
        }

        private BoundStatement BindDoWhileStatementSyntax(DoWhileStatementSyntax syntax)
        {
            var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
            if (body.Kind == BoundNodeKind.BoundInvalidStatement)
                return new BoundInvalidStatement();

            var condition = CheckTypeAndConversion(TypeSymbol.Bool, syntax.Condition);

            if (condition.Kind == BoundNodeKind.BoundInvalidExpression)
                return new BoundInvalidStatement();

            return new BoundDoWhileStatement(body, condition, breakLabel, continueLabel);
        }

        private BoundStatement BindBreakStatementSyntax(BreakStatementSyntax syntax)
        {
            if (labelStack.Count == 0)
            {
                diagnostics.ReportSyntaxError(ErrorMessage.InvalidBreakOrContinue, syntax.Span, "break");
                return new BoundInvalidStatement();
            }

            return new BoundGotoStatement(labelStack.Peek().breakLabel);
        }

        private BoundStatement BindContinueStatementSyntax(ContinueStatementSyntax syntax)
        {
            if (labelStack.Count == 0)
            {
                diagnostics.ReportSyntaxError(ErrorMessage.InvalidBreakOrContinue, syntax.Span, "continue");
                return new BoundInvalidStatement();
            }

            return new BoundGotoStatement(labelStack.Peek().continueLabel);
        }

        private BoundStatement BindLoopBody(StatementSyntax syntax, out BoundLabel breakLabel, out BoundLabel continueLabel)
        {
            labelCounter++;
            breakLabel = new BoundLabel($"break{labelCounter}");
            continueLabel = new BoundLabel($"continue{labelCounter}");

            labelStack.Push((breakLabel, continueLabel));
            var res = BindStatement(syntax);
            labelStack.Pop();
            return res;
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax, bool canBeVoid = false)
        {
            var res = BindExpressionInternal(syntax);
            if (!canBeVoid && res.ResultType == TypeSymbol.Void)
            {
                diagnostics.ReportTypeError(ErrorMessage.CannotBeVoid, syntax.Span);
                return new BoundInvalidExpression();
            }
            return res;
        }

        private BoundExpression BindExpressionInternal(ExpressionSyntax syntax)
        {
            if (!syntax.IsValid)
                return new BoundInvalidExpression();

            switch (syntax.Kind)
            {
                case SyntaxNodeKind.LiteralExpressionSyntax:
                    return BindLiteralExpressionSyntax((LiteralExpressionSyntax)syntax);
                case SyntaxNodeKind.VariableExpressionSyntax:
                    return BindVariableExpressionSyntax((VariableExpressionSyntax)syntax);
                case SyntaxNodeKind.UnaryExpressionSyntax:
                    return BindUnaryExpressionSyntax((UnaryExpressionSyntax)syntax);
                case SyntaxNodeKind.BinaryExpressionSyntax:
                    return BindBinaryExpressionSyntax((BinaryExpressionSyntax)syntax);
                case SyntaxNodeKind.CallExpressionSyntax:
                    return BindCallExpressionSyntax((CallExpressionSyntax)syntax);
                case SyntaxNodeKind.AssignmentExpressionSyntax:
                    return BindAssignmentExpressionSyntax((AssignmentExpressionSyntax)syntax);
                case SyntaxNodeKind.AdditionalAssignmentExpressionSyntax:
                    return BindAdditionalAssignmentExpressionSyntax((AdditionalAssignmentExpressionSyntax)syntax);
                case SyntaxNodeKind.PostIncDecExpressionSyntax:
                    return BindPostIncDecExpressionSyntax((PostIncDecExpressionSyntax)syntax);
                default: throw new Exception($"Unexpected SyntaxKind <{syntax.Kind}>");
            }
        }

        private BoundExpression BindLiteralExpressionSyntax(LiteralExpressionSyntax syntax)
        {
            var value = syntax.Literal.Value;
            var type = BindFacts.GetTypeSymbol(syntax.Literal.Kind);
            return new BoundLiteralExpression(value, type);
        }

        private BoundExpression BindVariableExpressionSyntax(VariableExpressionSyntax syntax)
        {
            var identifier = (string)syntax.Name.Value;
            if (!scope.TryLookUpVariable(identifier, out VariableSymbol variable))
            {
                diagnostics.ReportIdentifierError(ErrorMessage.UnresolvedIdentifier, syntax.Name.Span, syntax.Name.Value);
                return new BoundInvalidExpression();
            }
            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindUnaryExpressionSyntax(UnaryExpressionSyntax syntax)
        {
            var right = BindExpression(syntax.Expression);
            if (right.Kind == BoundNodeKind.BoundInvalidExpression)
                return new BoundInvalidExpression();

            var boundOperator = BindUnaryOperator(syntax.Op.Kind);

            var resultType = BindFacts.ResolveUnaryType(boundOperator, right.ResultType);

            if (boundOperator == null || resultType == null)
            {
                diagnostics.ReportTypeError(ErrorMessage.UnsupportedUnaryOperator, syntax.Op.Span, syntax.Op.Value.ToString(), right.ResultType);
                return new BoundInvalidExpression();
            }

            return new BoundUnaryExpression((BoundUnaryOperator)boundOperator, right, (TypeSymbol)resultType);
        }

        private BoundExpression BindBinaryExpressionSyntax(BinaryExpressionSyntax syntax)
        {
            var left = BindExpression(syntax.Left);
            var right = BindExpression(syntax.Right);

            if (left.Kind == BoundNodeKind.BoundInvalidExpression || right.Kind == BoundNodeKind.BoundInvalidExpression)
                return new BoundInvalidExpression();

            var boundOperator = BindBinaryOperator(syntax.Op.Kind);
            var resultType = BindFacts.ResolveBinaryType(boundOperator, left.ResultType, right.ResultType);

            if (boundOperator == null || resultType == null)
            {
                diagnostics.ReportTypeError(ErrorMessage.UnsupportedBinaryOperator, syntax.Op.Span, syntax.Op.Value.ToString(), left.ResultType, right.ResultType);
                return new BoundInvalidExpression();
            }

            return new BoundBinaryExpression((BoundBinaryOperator)boundOperator, left, right, (TypeSymbol)resultType);
        }

        private BoundExpression BindCallExpressionSyntax(CallExpressionSyntax syntax)
        {
            if (syntax.Arguments.Length == 1 && TypeSymbol.Lookup(syntax.Identifier.Value.ToString()) is TypeSymbol type)
                return BindExplicitConversion(type, syntax.Arguments[0]);

            if (!scope.TryLookUpFunction(syntax.Identifier.Value.ToString(), out var symbol))
            {
                diagnostics.ReportIdentifierError(ErrorMessage.UnresolvedIdentifier, syntax.Identifier.Span, syntax.Identifier.Value.ToString());
                return new BoundInvalidExpression();
            }

            if (syntax.Arguments.Length != symbol.Parameters.Length)
            {
                diagnostics.ReportSyntaxError(ErrorMessage.WrongAmountOfArguments, syntax.LeftParenthesis.Span + syntax.RightParenthesis.Span, symbol.Name, symbol.Parameters.Length, syntax.Arguments.Length);
                return new BoundInvalidExpression();
            }


            var argBuilder = ImmutableArray.CreateBuilder<BoundExpression>(symbol.Parameters.Length);

            for (int i = 0; i < symbol.Parameters.Length; i++)
            {
                var arg = syntax.Arguments[i];
                var param = symbol.Parameters[i];

                var boundArg = CheckTypeAndConversion(param.Type, arg);

                if (boundArg.Kind == BoundNodeKind.BoundInvalidExpression)
                    return new BoundInvalidExpression();

                argBuilder.Add(boundArg);
            }

            return new BoundCallExpression(symbol, argBuilder.MoveToImmutable());
        }

        private BoundExpression BindAssignmentExpressionSyntax(AssignmentExpressionSyntax syntax)
        {
            if (!scope.TryLookUpVariable((string)syntax.Identifier.Value, out var variable))
            {
                diagnostics.ReportIdentifierError(ErrorMessage.UnresolvedIdentifier, syntax.Identifier.Span, (string)syntax.Identifier.Value);
                return new BoundInvalidExpression();
            }

            if (variable is GlobalVariableSymbol globalVariable && globalVariable.Modifier == VariableModifier.Constant)
            {
                diagnostics.ReportTypeError(ErrorMessage.CannotAssignToConst, syntax.Identifier.Span, syntax.Identifier.Value);
                return new BoundInvalidExpression();
            }

            var expr = CheckTypeAndConversion(variable.Type, syntax.Expression);

            if (expr.Kind == BoundNodeKind.BoundInvalidExpression)
                return new BoundInvalidExpression();

            else return new BoundAssignementExpression(variable, expr);

        }

        private BoundExpression BindAdditionalAssignmentExpressionSyntax(AdditionalAssignmentExpressionSyntax syntax)
        {
            if (!scope.TryLookUpVariable((string)syntax.Identifier.Value, out VariableSymbol variable))
            {
                diagnostics.ReportIdentifierError(ErrorMessage.UnresolvedIdentifier, syntax.Identifier.Span, (string)syntax.Identifier.Value);
                return new BoundInvalidExpression();
            }

            if (variable is GlobalVariableSymbol globalVariable && globalVariable.Modifier == VariableModifier.Constant)
            {
                diagnostics.ReportTypeError(ErrorMessage.CannotAssignToConst, syntax.Identifier.Span, syntax.Identifier.Value);
                return new BoundInvalidExpression();
            }

            var left = new BoundVariableExpression(variable);
            var right = BindExpression(syntax.Expression);

            if (right is BoundInvalidExpression)
                return new BoundInvalidExpression();

            var op = BindBinaryOperator(syntax.Op.Kind);
            var resultType = BindFacts.ResolveBinaryType(op, left.ResultType, right.ResultType);

            if (op == null || resultType == null)
            {
                diagnostics.ReportTypeError(ErrorMessage.UnsupportedBinaryOperator, syntax.Op.Span, syntax.Op.Value.ToString(), left.ResultType, right.ResultType);
                return new BoundInvalidExpression();
            }
            var binaryExpression = new BoundBinaryExpression((BoundBinaryOperator)op, left, right, (TypeSymbol)resultType);
            return new BoundAssignementExpression(variable, binaryExpression);
        }

        private BoundExpression BindPostIncDecExpressionSyntax(PostIncDecExpressionSyntax syntax)
        {
            if (!scope.TryLookUpVariable((string)syntax.Identifier.Value, out VariableSymbol variable))
            {
                diagnostics.ReportIdentifierError(ErrorMessage.UnresolvedIdentifier, syntax.Identifier.Span, (string)syntax.Identifier.Value);
                return new BoundInvalidExpression();
            }

            if (variable is GlobalVariableSymbol globalVariable && globalVariable.Modifier == VariableModifier.Constant)
            {
                diagnostics.ReportTypeError(ErrorMessage.CannotAssignToConst, syntax.Identifier.Span, syntax.Identifier.Value);
                return new BoundInvalidExpression();
            }

            var left = new BoundVariableExpression(variable);
            var right = new BoundLiteralExpression(1, TypeSymbol.Int);

            var op = BindBinaryOperator(syntax.Op.Kind);

            var resultType = BindFacts.ResolveBinaryType(op, left.ResultType, right.ResultType);

            if (op == null || resultType == null)
            {
                diagnostics.ReportTypeError(ErrorMessage.UnsupportedBinaryOperator, syntax.Op.Span, syntax.Op.Value.ToString(), left.ResultType, right.ResultType);
                return new BoundInvalidExpression();
            }

            var binaryExpression = new BoundBinaryExpression((BoundBinaryOperator)op, left, right, (TypeSymbol)resultType);
            return new BoundAssignementExpression(variable, binaryExpression);
        }

        private BoundBinaryOperator? BindBinaryOperator(SyntaxTokenKind op)
        {
            switch (op)
            {
                case SyntaxTokenKind.Plus: return BoundBinaryOperator.Addition;
                case SyntaxTokenKind.Minus: return BoundBinaryOperator.Subtraction;
                case SyntaxTokenKind.Star: return BoundBinaryOperator.Multiplication;
                case SyntaxTokenKind.Slash: return BoundBinaryOperator.Division;
                case SyntaxTokenKind.StarStar: return BoundBinaryOperator.Power;
                case SyntaxTokenKind.SlashSlah: return BoundBinaryOperator.Root;
                case SyntaxTokenKind.Percentage: return BoundBinaryOperator.Modulo;
                case SyntaxTokenKind.Ampersand: return BoundBinaryOperator.BitwiseAnd;
                case SyntaxTokenKind.Pipe: return BoundBinaryOperator.BitwiseOr;
                case SyntaxTokenKind.Hat: return BoundBinaryOperator.BitwiseXor;
                case SyntaxTokenKind.EqualEqual: return BoundBinaryOperator.EqualEqual;
                case SyntaxTokenKind.NotEqual: return BoundBinaryOperator.NotEqual;
                case SyntaxTokenKind.LessThan: return BoundBinaryOperator.LessThan;
                case SyntaxTokenKind.LessEqual: return BoundBinaryOperator.LessEqual;
                case SyntaxTokenKind.GreaterThan: return BoundBinaryOperator.GreaterThan;
                case SyntaxTokenKind.GreaterEqual: return BoundBinaryOperator.GreaterEqual;
                case SyntaxTokenKind.AmpersandAmpersand: return BoundBinaryOperator.LogicalAnd;
                case SyntaxTokenKind.PipePipe: return BoundBinaryOperator.LogicalOr;
                case SyntaxTokenKind.PlusEqual: return BoundBinaryOperator.Addition;
                case SyntaxTokenKind.MinusEqual: return BoundBinaryOperator.Subtraction;
                case SyntaxTokenKind.StarEqual: return BoundBinaryOperator.Multiplication;
                case SyntaxTokenKind.SlashEqual: return BoundBinaryOperator.Division;
                case SyntaxTokenKind.AmpersandEqual: return BoundBinaryOperator.BitwiseAnd;
                case SyntaxTokenKind.PipeEqual: return BoundBinaryOperator.BitwiseOr;
                case SyntaxTokenKind.PlusPlus: return BoundBinaryOperator.Addition;
                case SyntaxTokenKind.MinusMinus: return BoundBinaryOperator.Subtraction;
                default: return null;
            }
        }

        private BoundUnaryOperator? BindUnaryOperator(SyntaxTokenKind op)
        {
            switch (op)
            {
                case SyntaxTokenKind.Plus: return BoundUnaryOperator.Identety;
                case SyntaxTokenKind.Minus: return BoundUnaryOperator.Negation;
                case SyntaxTokenKind.Bang: return BoundUnaryOperator.LogicalNot;
                case SyntaxTokenKind.Tilde: return BoundUnaryOperator.BitwiseNot;
                default: return null;
            }
        }

        private BoundExpression CheckTypeAndConversion(TypeSymbol type, ExpressionSyntax expression)
        {
            var expr = BindExpression(expression);

            if (expr.Kind == BoundNodeKind.BoundInvalidExpression)
                return new BoundInvalidExpression();

            var conversionType = BindFacts.ClassifyConversion(expr.ResultType, type);

            if (conversionType == ConversionType.Identety)
                return expr;
            else if (conversionType == ConversionType.Implicit)
                return new BoundConversionExpression(type, expr);
            else if (conversionType == ConversionType.Explicit)
            {
                diagnostics.ReportTypeError(ErrorMessage.MissingExplicitConversion, expression.Span, type, expr.ResultType);
                return new BoundInvalidExpression();
            }

            diagnostics.ReportTypeError(ErrorMessage.IncompatibleTypes, expression.Span, type, expr.ResultType);
            return new BoundInvalidExpression();
        }

        private BoundExpression BindExplicitConversion(TypeSymbol type, ExpressionSyntax syntax)
        {
            var expr = BindExpression(syntax);

            if (expr.Kind == BoundNodeKind.BoundInvalidExpression)
                return new BoundInvalidExpression();

            var conversion = BindFacts.ClassifyConversion(expr.ResultType, type);

            if (conversion == ConversionType.None)
            {
                diagnostics.ReportTypeError(ErrorMessage.CannotConvert, syntax.Span, expr.ResultType, type);
                return new BoundInvalidExpression();
            }

            return new BoundConversionExpression(type, expr);
        }

    }
}
