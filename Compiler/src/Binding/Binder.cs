using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        private bool isTreeValid;
        private int labelCounter;
        private BoundScope scope;

        public IEnumerable<Diagnostic> GetDiagnostics() => diagnostics;

        private void ReportError(ErrorMessage message, TextSpan span, params object[] values)
        {
            if (isTreeValid)
                diagnostics.ReportError(message, span, values);
            isTreeValid = false;
        }

        private Binder(SourceText source, BoundScope parentScope, bool isScript, FunctionSymbol function)
        {
            this.isScript = isScript;
            this.function = function;
            this.isTreeValid = true;
            this.labelStack = new Stack<(BoundLabel breakLabel, BoundLabel continueLabel)>();
            this.scope = new BoundScope(parentScope);
            this.diagnostics = new DiagnosticBag(source);

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
            var globalBinder = new Binder(unit.Text, parentScope, isScript, function: null);
            var isProgramValid = true;

            foreach (var func in functionSyntax)
                globalBinder.DeclareFunction(func);

            var globalStatementBuilder = ImmutableArray.CreateBuilder<BoundStatement>();
            foreach (var stmt in statementSyntax)
            {
                var boundStmt = globalBinder.BindStatement(stmt.Statement);
                globalStatementBuilder.Add(boundStmt);
            }

            diagnostics.AddRange(globalBinder.GetDiagnostics());
            isProgramValid = isProgramValid && globalBinder.isTreeValid;

            var globalBlockStatement = Lowerer.Lower(new BoundBlockStatement(globalStatementBuilder.ToImmutable(), isProgramValid));
            var declaredVariables = globalBinder.scope.GetDeclaredVariables();
            var declaredFunctions = globalBinder.scope.GetDeclaredFunctions();

            var currentScope = globalBinder.scope;
            var functions = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();

            foreach (var symbol in declaredFunctions)
            {
                var binder = new Binder(unit.Text, currentScope, isScript, symbol);
                var body = binder.BindBlockStatmentSyntax(symbol.Syntax.Body);
                var loweredBody = Lowerer.Lower(body);

                if (!ControlFlowGraph.AllPathsReturn(symbol, loweredBody))
                    binder.ReportError(ErrorMessage.AllPathsMustReturn, symbol.Syntax.Identifier.Span);

                functions.Add(symbol, loweredBody);
                diagnostics.AddRange(binder.GetDiagnostics());
                isProgramValid = isProgramValid && binder.isTreeValid;
            }


            FunctionSymbol mainFunc = null;

            if (!isScript && !globalBinder.scope.TryLookUpFunction("main", out mainFunc))
            {
                diagnostics.Add(new Diagnostic("Program doesn't define main fuction.", TextLocation.Undefined, ErrorLevel.Error));
                isProgramValid = false;
            }
            if (mainFunc != null && mainFunc.Parameters.Length > 0)
            {
                diagnostics.Add(new Diagnostic("Main function cannot have arguments.", new TextLocation(unit.Text, mainFunc.Syntax.Parameters.Span), ErrorLevel.Error));
                isProgramValid = false;
            }
            if (mainFunc != null && mainFunc.ReturnType != TypeSymbol.Void)
            {
                diagnostics.Add(new Diagnostic("Main function must return void.", new TextLocation(unit.Text, mainFunc.Syntax.ReturnType.Span), ErrorLevel.Error));
                isProgramValid = false;
            }

            return new BoundProgram(previous, globalBlockStatement, declaredVariables, mainFunc, functions.ToImmutable(), new DiagnosticReport(diagnostics.ToImmutable()), isProgramValid);
        }

        private static BoundScope CreateBoundScopes(BoundProgram previous)
        {
            var stack = new Stack<BoundProgram>();


            while (previous != null)
            {
                if (previous.IsValid)
                    stack.Push(previous);
                previous = previous.Previous;
            }

            var current = CreateRootScope();

            while (stack.Count > 0)
            {
                var global = stack.Pop();
                var scope = new BoundScope(current);
                foreach (var variable in global.GlobalVariables)
                    scope.TryDeclareVariable(variable);

                foreach (var function in global.Functions)
                    scope.TryDeclareFunction(function.Key);

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
                    diagnostics.ReportError(ErrorMessage.DuplicatedParameters, parameterSyntax.Span, name);
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
                diagnostics.ReportError(ErrorMessage.FunctionAlreadyDeclared, func.Identifier.Span, func.Identifier.Value);
        }

        private BoundStatement BindStatement(StatementSyntax syntax)
        {
            if (!syntax.IsValid)
                isTreeValid = false;

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
                case SyntaxNodeKind.ReturnStatementSyntax:
                    return BindReturnStatementSyntax((ReturnStatementSyntax)syntax);
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
                builder.Add(bound);
            }
            scope = scope.Parent;
            return new BoundBlockStatement(builder.ToImmutable(), isTreeValid);
        }

        private BoundStatement BindExpressionStatementSyntax(ExpressionStatementSyntax syntax)
        {
            var expr = BindExpression(syntax.Expression, true);
            return new BoundExpressionStatement(expr, isTreeValid);
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

            bool isConst = syntax.VarKeyword.Kind == SyntaxTokenKind.ConstKeyword;

            VariableSymbol variable;
            if (function == null)
                variable = new GlobalVariableSymbol(syntax.Identifier.Value.ToString(), type, isConst ? VariableModifier.Constant : VariableModifier.None);
            else
                variable = new LocalVariableSymbol(syntax.Identifier.Value.ToString(), type, isConst ? VariableModifier.Constant : VariableModifier.None);

            if (!scope.TryDeclareVariable(variable))
                ReportError(ErrorMessage.VariableAlreadyDeclared, syntax.Identifier.Span, variable.Name);
            return new BoundVariableDeclarationStatement(variable, expr, isTreeValid);
        }

        private BoundStatement BindIfStatementSyntax(IfStatementSyntax syntax)
        {
            var condition = CheckTypeAndConversion(TypeSymbol.Bool, syntax.Condition);
            var stmt = BindStatement(syntax.Body);
            var elseStmt = syntax.ElseStatement == null ? null : BindStatement(syntax.ElseStatement.Body);
            return new BoundIfStatement(condition, stmt, elseStmt, isTreeValid);
        }

        private BoundStatement BindWhileStatementSyntax(WhileStatementSyntax syntax)
        {
            var condition = CheckTypeAndConversion(TypeSymbol.Bool, syntax.Condition);
            var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
            return new BoundWhileStatement(condition, body, breakLabel, continueLabel, isTreeValid);
        }

        private BoundStatement BindForStatementSyntax(ForStatementSyntax syntax)
        {
            var variableDecl = BindStatement(syntax.VariableDeclaration);
            var condition = CheckTypeAndConversion(TypeSymbol.Bool, syntax.Condition);
            var increment = BindExpression(syntax.Increment);
            var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
            return new BoundForStatement(variableDecl, condition, increment, body, breakLabel, continueLabel, isTreeValid);
        }

        private BoundStatement BindDoWhileStatementSyntax(DoWhileStatementSyntax syntax)
        {
            var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
            var condition = CheckTypeAndConversion(TypeSymbol.Bool, syntax.Condition);
            return new BoundDoWhileStatement(body, condition, breakLabel, continueLabel, isTreeValid);
        }

        private BoundStatement BindBreakStatementSyntax(BreakStatementSyntax syntax)
        {
            BoundLabel label;

            if (labelStack.Count == 0)
            {
                ReportError(ErrorMessage.InvalidBreakOrContinue, syntax.Span, "break");
                label = new BoundLabel("Invalid break");
            }
            else
                label = labelStack.Peek().breakLabel;

            return new BoundGotoStatement(label, isTreeValid);
        }

        private BoundStatement BindContinueStatementSyntax(ContinueStatementSyntax syntax)
        {
            BoundLabel label;

            if (labelStack.Count == 0)
            {
                ReportError(ErrorMessage.InvalidBreakOrContinue, syntax.Span, "continue");
                label = new BoundLabel("Invalid continue");
            }
            else
                label = labelStack.Peek().continueLabel;

            return new BoundGotoStatement(label, isTreeValid);
        }

        private BoundStatement BindReturnStatementSyntax(ReturnStatementSyntax syntax)
        {
            BoundExpression expr;

            if (function == null)
            {
                ReportError(ErrorMessage.ReturnOnlyInFunction, syntax.Span);
                expr = null;
            }
            else if (syntax.ReturnExpression == null)
            {
                expr = null;
                if (function.ReturnType != TypeSymbol.Void)
                    ReportError(ErrorMessage.IncompatibleTypes, syntax.VoidToken.Span, function.ReturnType, TypeSymbol.Void);
            }
            else
            {
                expr = CheckTypeAndConversion(function.ReturnType, syntax.ReturnExpression);
            }

            return new BoundReturnStatement(expr, isTreeValid);
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
                ReportError(ErrorMessage.CannotBeVoid, syntax.Span);
                return new BoundInvalidExpression();
            }
            return res;
        }

        private BoundExpression BindExpressionInternal(ExpressionSyntax syntax)
        {
            if (!syntax.IsValid)
                isTreeValid = false;

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
            return new BoundLiteralExpression(value, type, isTreeValid);
        }

        private BoundExpression BindVariableExpressionSyntax(VariableExpressionSyntax syntax)
        {
            var identifier = syntax.Name.Value.ToString();
            if (!scope.TryLookUpVariable(identifier, out VariableSymbol variable))
                ReportError(ErrorMessage.UnresolvedIdentifier, syntax.Name.Span, syntax.Name.Value);
            return new BoundVariableExpression(variable, isTreeValid);
        }

        private BoundExpression BindUnaryExpressionSyntax(UnaryExpressionSyntax syntax)
        {
            var right = BindExpression(syntax.Expression);
            var boundOperator = BindUnaryOperator(syntax.Op.Kind);
            var resultType = BindFacts.ResolveUnaryType(boundOperator, right.ResultType);
            if (boundOperator == BoundUnaryOperator.Invalid || resultType == null)
                ReportError(ErrorMessage.UnsupportedUnaryOperator, syntax.Op.Span, syntax.Op.Value.ToString(), right.ResultType);
            return new BoundUnaryExpression(boundOperator, right, resultType, isTreeValid);
        }

        private BoundExpression BindBinaryExpressionSyntax(BinaryExpressionSyntax syntax)
        {
            var left = BindExpression(syntax.Left);
            var right = BindExpression(syntax.Right);
            var boundOperator = BindBinaryOperator(syntax.Op.Kind);
            var resultType = BindFacts.ResolveBinaryType(boundOperator, left.ResultType, right.ResultType);

            if (boundOperator == BoundBinaryOperator.Invalid || resultType == null)
                ReportError(ErrorMessage.UnsupportedBinaryOperator, syntax.Op.Span, syntax.Op.Value.ToString(), left.ResultType, right.ResultType);
            return new BoundBinaryExpression(boundOperator, left, right, resultType, isTreeValid);
        }

        private BoundExpression BindCallExpressionSyntax(CallExpressionSyntax syntax)
        {
            if (syntax.Arguments.Length == 1 && TypeSymbol.Lookup(syntax.Identifier.Value.ToString()) is TypeSymbol type)
                return BindExplicitConversion(type, syntax.Arguments[0]);

            if (!scope.TryLookUpFunction(syntax.Identifier.Value.ToString(), out var symbol))
                ReportError(ErrorMessage.UnresolvedIdentifier, syntax.Identifier.Span, syntax.Identifier.Value.ToString());

            if (syntax.Arguments.Length != symbol.Parameters.Length)
                ReportError(ErrorMessage.WrongAmountOfArguments, syntax.LeftParenthesis.Span + syntax.RightParenthesis.Span, symbol.Name, symbol.Parameters.Length, syntax.Arguments.Length);

            var len = Math.Min(syntax.Arguments.Length, symbol.Parameters.Length);

            var argBuilder = ImmutableArray.CreateBuilder<BoundExpression>(len);

            for (int i = 0; i < len; i++)
            {
                var arg = syntax.Arguments[i];
                var param = symbol.Parameters[i];

                var boundArg = CheckTypeAndConversion(param.Type, arg);
                argBuilder.Add(boundArg);
            }

            return new BoundCallExpression(symbol, argBuilder.MoveToImmutable(), isTreeValid);
        }

        private BoundExpression BindAssignmentExpressionSyntax(AssignmentExpressionSyntax syntax)
        {
            if (!scope.TryLookUpVariable(syntax.Identifier.Value.ToString(), out var variable))
                ReportError(ErrorMessage.UnresolvedIdentifier, syntax.Identifier.Span, syntax.Identifier.Value.ToString());

            if (variable.Modifiers == VariableModifier.Constant)
                ReportError(ErrorMessage.CannotAssignToConst, syntax.Identifier.Span, syntax.Identifier.Value);

            var expr = CheckTypeAndConversion(variable.Type, syntax.Expression);
            return new BoundAssignmentExpression(variable, expr, isTreeValid);
        }

        private BoundExpression BindAdditionalAssignmentExpressionSyntax(AdditionalAssignmentExpressionSyntax syntax)
        {
            if (!scope.TryLookUpVariable(syntax.Identifier.Value.ToString(), out VariableSymbol variable))
                ReportError(ErrorMessage.UnresolvedIdentifier, syntax.Identifier.Span, syntax.Identifier.Value.ToString());

            if (variable.Modifiers == VariableModifier.Constant)
                ReportError(ErrorMessage.CannotAssignToConst, syntax.Identifier.Span, syntax.Identifier.Value);


            var left = new BoundVariableExpression(variable, isTreeValid);
            var right = BindExpression(syntax.Expression);

            var op = BindBinaryOperator(syntax.Op.Kind);
            var resultType = BindFacts.ResolveBinaryType(op, left.ResultType, right.ResultType);

            if (op == BoundBinaryOperator.Invalid || resultType == null)
                ReportError(ErrorMessage.UnsupportedBinaryOperator, syntax.Op.Span, syntax.Op.Value.ToString(), left.ResultType, right.ResultType);

            var binaryExpression = new BoundBinaryExpression(op, left, right, resultType, isTreeValid);
            return new BoundAssignmentExpression(variable, binaryExpression, isTreeValid);
        }

        private BoundExpression BindPostIncDecExpressionSyntax(PostIncDecExpressionSyntax syntax)
        {
            if (!scope.TryLookUpVariable((string)syntax.Identifier.Value, out VariableSymbol variable))
                ReportError(ErrorMessage.UnresolvedIdentifier, syntax.Identifier.Span, (string)syntax.Identifier.Value);

            if (variable.Modifiers == VariableModifier.Constant)
                ReportError(ErrorMessage.CannotAssignToConst, syntax.Identifier.Span, syntax.Identifier.Value);

            var left = new BoundVariableExpression(variable, isTreeValid);
            var right = new BoundLiteralExpression(1L, TypeSymbol.Int, isTreeValid);
            var op = BindBinaryOperator(syntax.Op.Kind);
            var resultType = BindFacts.ResolveBinaryType(op, left.ResultType, right.ResultType);

            if (op == BoundBinaryOperator.Invalid || resultType == null)
                ReportError(ErrorMessage.UnsupportedBinaryOperator, syntax.Op.Span, syntax.Op.Value.ToString(), left.ResultType, right.ResultType);

            var binaryExpression = new BoundBinaryExpression(op, left, right, resultType, isTreeValid);
            return new BoundAssignmentExpression(variable, binaryExpression, isTreeValid);
        }

        private BoundBinaryOperator BindBinaryOperator(SyntaxTokenKind op)
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
                default: return BoundBinaryOperator.Invalid;
            }
        }

        private BoundUnaryOperator BindUnaryOperator(SyntaxTokenKind op)
        {
            switch (op)
            {
                case SyntaxTokenKind.Plus: return BoundUnaryOperator.Identety;
                case SyntaxTokenKind.Minus: return BoundUnaryOperator.Negation;
                case SyntaxTokenKind.Bang: return BoundUnaryOperator.LogicalNot;
                case SyntaxTokenKind.Tilde: return BoundUnaryOperator.BitwiseNot;
                default: return BoundUnaryOperator.Invalid;
            }
        }

        private BoundExpression CheckTypeAndConversion(TypeSymbol type, ExpressionSyntax expression)
        {
            var expr = BindExpression(expression);
            var conversionType = BindFacts.ClassifyConversion(expr.ResultType, type);

            if (conversionType == ConversionType.Identety)
                return expr;
            else if (conversionType == ConversionType.Explicit)
                ReportError(ErrorMessage.MissingExplicitConversion, expression.Span, type, expr.ResultType);
            else if (conversionType == ConversionType.None)
                ReportError(ErrorMessage.IncompatibleTypes, expression.Span, type, expr.ResultType);

            return new BoundConversionExpression(type, expr, isTreeValid);
        }

        private BoundExpression BindExplicitConversion(TypeSymbol type, ExpressionSyntax syntax)
        {
            var expr = BindExpression(syntax);
            var conversion = BindFacts.ClassifyConversion(expr.ResultType, type);

            if (conversion == ConversionType.None)
                ReportError(ErrorMessage.CannotConvert, syntax.Span, expr.ResultType, type);

            return new BoundConversionExpression(type, expr, isTreeValid);
        }

    }
}
