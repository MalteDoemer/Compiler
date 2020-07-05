using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Compiler.Diagnostics;
using Compiler.Symbols;
using Compiler.Syntax;

namespace Compiler.Binding
{

    internal sealed class Binder : IDiagnostable
    {
        private readonly DiagnosticBag diagnostics;
        private readonly Compilation previous;
        private readonly bool isScript;

        private bool isTreeValid;
        private BoundScope scope;

        public Binder(Compilation previous, bool isScript)
        {
            this.previous = previous;
            this.isScript = isScript;
            this.isTreeValid = true;
            this.diagnostics = new DiagnosticBag();
            var parentScope = CreateBoundScopes(previous);

            if (parentScope == null)
                scope = CreateRootScope();
            else
                scope = new BoundScope(parentScope);
        }

        private BoundScope CreateRootScope()
        {
            var scope = new BoundScope(null);
            foreach (var b in BuiltInFunctions.GetAll())
                scope.TryDeclareFunction(b);
            return scope;
        }

        public IEnumerable<Diagnostic> GetDiagnostics() => diagnostics;

        public BoundCompilationUnit BindCompilationUnit(CompilationUnitSyntax unit)
        {
            foreach (var func in unit.Members.OfType<FunctionDeclarationSyntax>())
                DeclareFunction(func);

            var stmtBuilder = ImmutableArray.CreateBuilder<BoundStatement>();

            foreach (var globalStmt in unit.Members.OfType<GlobalStatementSynatx>())
            {
                var stmt = BindStatement(globalStmt.Statement);
                stmtBuilder.Add(stmt);
            }

            var globalStatements = new BoundBlockStatement(stmtBuilder.ToImmutable(), isTreeValid);


            var variables = scope.GetDeclaredVariables();
            var functions = scope.GetDeclaredFunctions();
            return new BoundCompilationUnit(globalStatements, variables, functions);
        }

        private void DeclareFunction(FunctionDeclarationSyntax func)
        {
            var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
            var seenParameters = new HashSet<string>();

            foreach (var parameterSyntax in func.Parameters)
            {
                var type = BindFacts.GetTypeSymbol(parameterSyntax.TypeClause.TypeToken.Kind);
                var name = (string)parameterSyntax.Identifier.Value;
                if (!seenParameters.Add(name))
                {
                    if (isTreeValid)
                        diagnostics.ReportIdentifierError(ErrorMessage.DuplicatedParameters, parameterSyntax.Span, name);
                    isTreeValid = false;
                }
                parameters.Add(new ParameterSymbol(name, type));
            }

            var returnType = BindFacts.GetTypeSymbol(func.ReturnType.TypeToken.Kind);
            var symbol = new FunctionSymbol((string)func.Identifier.Value, parameters.ToImmutable(), returnType);
            if (!scope.TryDeclareFunction(symbol))
            {
                if (isTreeValid)
                    diagnostics.ReportIdentifierError(ErrorMessage.FunctionAlreadyDeclared, func.Identifier.Span, func.Identifier.Value);
                isTreeValid = false;
            }
        }

        private BoundScope CreateBoundScopes(Compilation previous)
        {
            var stack = new Stack<Compilation>();

            while (previous != null)
            {
                if (previous.Root != null)
                    stack.Push(previous);
                previous = previous.Previous;
            }

            BoundScope current = null;

            while (stack.Count > 0)
            {
                var global = stack.Pop();
                var scope = new BoundScope(current);
                foreach (var variable in global.Root.DeclaredVariables)
                    scope.TryDeclareVariable(variable);

                foreach (var function in global.Root.DeclaredFunctions)
                    scope.TryDeclareFunction(function);

                current = scope;
            }

            return current;
        }

        private BoundStatement BindStatement(StatementSyntax syntax)
        {
            if (!syntax.IsValid)
                isTreeValid = false;

            if (syntax is ExpressionStatement es)
                return BindExpressionStatement(es);
            else if (syntax is BlockStatment bs)
                return BindBlockStatement(bs);
            else if (syntax is VariableDeclarationStatement vs)
                return BindVariableDeclarationStatement(vs);
            else if (syntax is IfStatementSyntax ifs)
                return BindIfStatement(ifs);
            else if (syntax is WhileStatementSyntax ws)
                return BindWhileStatement(ws);
            else if (syntax is DoWhileStatementSyntax dws)
                return BindDoWhileStatement(dws);
            else if (syntax is ForStatementSyntax fs)
            {
                scope = new BoundScope(scope);
                var res = BindForStatement(fs);
                scope = scope.Parent;
                return res;
            }
            else throw new Exception($"Unexpected StatementSyntax <{syntax}>");
        }

        private BoundStatement BindDoWhileStatement(DoWhileStatementSyntax syntax)
        {
            var body = BindStatement(syntax.Body);
            var condition = CheckTypeAndConversion(TypeSymbol.Bool, syntax.Condition);
            return new BoundDoWhileStatement(body, condition, isTreeValid);
        }

        private BoundStatement BindForStatement(ForStatementSyntax syntax)
        {
            var variableDecl = BindStatement(syntax.VariableDeclaration);
            var condition = CheckTypeAndConversion(TypeSymbol.Bool, syntax.Condition);
            var increment = BindExpression(syntax.Increment);
            var body = BindStatement(syntax.Body);
            return new BoundForStatement(variableDecl, condition, increment, body, isTreeValid);
        }

        private BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
        {
            var condition = CheckTypeAndConversion(TypeSymbol.Bool, syntax.Condition);
            var body = BindStatement(syntax.Body);
            return new BoundWhileStatement(condition, body, isTreeValid);
        }

        private BoundStatement BindIfStatement(IfStatementSyntax syntax)
        {
            var condition = CheckTypeAndConversion(TypeSymbol.Bool, syntax.Condition);
            var stmt = BindStatement(syntax.Body);
            var elseStmt = syntax.ElseStatement == null ? null : BindStatement(syntax.ElseStatement.Body);
            return new BoundIfStatement(condition, stmt, elseStmt, isTreeValid);
        }

        private BoundStatement BindVariableDeclarationStatement(VariableDeclarationStatement syntax)
        {
            var type = BindFacts.GetTypeSymbol(syntax.TypeClause.TypeToken.Kind);
            var expr = CheckTypeAndConversion(type, syntax.Expression);
            var variable = new VariableSymbol((string)syntax.Identifier.Value, type);
            if (!scope.TryDeclareVariable(variable))
            {
                if (isTreeValid)
                    diagnostics.ReportIdentifierError(ErrorMessage.VariableAlreadyDeclared, syntax.Identifier.Span, variable.Name);
                isTreeValid = false;
            }
            return new BoundVariableDeclaration(variable, expr, isTreeValid);
        }

        private BoundStatement BindBlockStatement(BlockStatment syntax)
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

        private BoundStatement BindExpressionStatement(ExpressionStatement syntax)
        {
            var expr = BindExpression(syntax.Expression, true);
            return new BoundExpressionStatement(expr, isTreeValid);
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax, bool canBeVoid = false)
        {
            var res = BindExpressionInternal(syntax);
            if (!canBeVoid && res.ResultType == TypeSymbol.Void)
            {
                if (isTreeValid)
                    diagnostics.ReportTypeError(ErrorMessage.CannotBeVoid, syntax.Span);
                isTreeValid = false;
                return res;
            }
            return res;
        }

        private BoundExpression CheckTypeAndConversion(TypeSymbol type, ExpressionSyntax expression)
        {
            var expr = BindExpression(expression);
            var conversionType = BindFacts.ClassifyConversion(expr.ResultType, type);

            if (conversionType == ConversionType.Identety)
                return expr;
            else if (conversionType == ConversionType.Implicit)
                return new BoundConversionExpression(type, expr, isTreeValid);
            else if (conversionType == ConversionType.Explicit)
            {
                if (isTreeValid)
                    diagnostics.ReportTypeError(ErrorMessage.MissingExplicitConversion, expression.Span, type, expr.ResultType);
                isTreeValid = false;
                return expr;
            }
            if (isTreeValid)
                diagnostics.ReportTypeError(ErrorMessage.IncompatibleTypes, expression.Span, type, expr.ResultType);
            isTreeValid = false;
            return expr;
        }

        private BoundExpression BindExpressionInternal(ExpressionSyntax syntax)
        {
            if (!syntax.IsValid)
                isTreeValid = false;


            if (syntax is LiteralExpressionSyntax le)
                return BindLiteralExpression(le);
            else if (syntax is UnaryExpressionSyntax ue)
                return BindUnaryExpression(ue);
            else if (syntax is BinaryExpressionSyntax be)
                return BindBinaryExpression(be);
            else if (syntax is VariableExpressionSyntax ve)
                return BindVariableExpression(ve);
            else if (syntax is AssignmentExpressionSyntax ee)
                return BindAssignmentExpression(ee);
            else if (syntax is AdditionalAssignmentExpression ae)
                return BindAdditioalAssignmentExpression(ae);
            else if (syntax is PostIncDecExpression ide)
                return BindPostIncDecExpression(ide);
            else if (syntax is CallExpressionSyntax cs)
                return BindCallExpession(cs);
            else throw new Exception($"Unknown Syntax kind <{syntax}>");
        }

        private BoundExpression BindCallExpession(CallExpressionSyntax syntax)
        {
            if (syntax.Arguments.Length == 1 && TypeSymbol.Lookup(syntax.Identifier.Value.ToString()) is TypeSymbol type)
                return BindExplicitConversion(type, syntax.Arguments[0]);

            if (!scope.TryLookUpFunction((string)syntax.Identifier.Value, out var symbol))
            {
                if (isTreeValid)
                    diagnostics.ReportIdentifierError(ErrorMessage.UnresolvedIdentifier, syntax.Identifier.Span, (string)syntax.Identifier.Value);
                isTreeValid = false;
            }

            if (syntax.Arguments.Length != symbol.Parameters.Length)
            {
                if (isTreeValid)
                    diagnostics.ReportSyntaxError(ErrorMessage.WrongAmountOfArguments, syntax.LeftParenthesis.Span + syntax.RightParenthesis.Span, symbol.Name, symbol.Parameters.Length, syntax.Arguments.Length);
                isTreeValid = false;
            }

            var argBuilder = ImmutableArray.CreateBuilder<BoundExpression>(symbol.Parameters.Length);

            for (int i = 0; i < symbol.Parameters.Length; i++)
            {
                var arg = syntax.Arguments[i];
                var param = symbol.Parameters[i];

                var boundArg = CheckTypeAndConversion(param.Type, arg);
                argBuilder.Add(boundArg);
            }

            return new BoundCallExpression(symbol, argBuilder.MoveToImmutable(), isTreeValid);
        }

        private BoundExpression BindExplicitConversion(TypeSymbol type, ExpressionSyntax syntax)
        {
            var expr = BindExpression(syntax);
            var conversion = BindFacts.ClassifyConversion(expr.ResultType, type);

            if (conversion == ConversionType.None)
            {
                if (isTreeValid)
                    diagnostics.ReportTypeError(ErrorMessage.CannotConvert, syntax.Span, expr.ResultType, type);
                isTreeValid = false;
            }

            return new BoundConversionExpression(type, expr, isTreeValid);
        }

        private BoundExpression BindPostIncDecExpression(PostIncDecExpression syntax)
        {
            if (!scope.TryLookUpVariable((string)syntax.Identifier.Value, out VariableSymbol variable))
            {
                if (isTreeValid)
                    diagnostics.ReportIdentifierError(ErrorMessage.UnresolvedIdentifier, syntax.Identifier.Span, (string)syntax.Identifier.Value);
                isTreeValid = false;
            }

            var left = new BoundVariableExpression(variable, isTreeValid);
            var right = new BoundLiteralExpression(1, TypeSymbol.Int, isTreeValid);

            var op = BindBinaryOperator(syntax.Op.Kind);
            var resultType = BindFacts.ResolveBinaryType(op, left.ResultType, right.ResultType);

            if (op == null || resultType == null)
            {
                if (isTreeValid)
                    diagnostics.ReportTypeError(ErrorMessage.UnsupportedBinaryOperator, syntax.Op.Span, syntax.Op.Value.ToString(), left.ResultType, right.ResultType);
                op = BoundBinaryOperator.Addition;
                resultType = TypeSymbol.ErrorType;
            }

            var binaryExpression = new BoundBinaryExpression((BoundBinaryOperator)op, left, right, resultType, isTreeValid);
            return new BoundAssignementExpression(variable, binaryExpression, isTreeValid);
        }

        private BoundExpression BindAdditioalAssignmentExpression(AdditionalAssignmentExpression syntax)
        {
            if (!scope.TryLookUpVariable((string)syntax.Identifier.Value, out VariableSymbol variable) && isTreeValid)
                diagnostics.ReportIdentifierError(ErrorMessage.UnresolvedIdentifier, syntax.Identifier.Span, (string)syntax.Identifier.Value);

            var left = new BoundVariableExpression(variable, isTreeValid);
            var right = BindExpression(syntax.Expression);
            var op = BindBinaryOperator(syntax.Op.Kind);
            var resultType = BindFacts.ResolveBinaryType(op, left.ResultType, right.ResultType);

            if (op == null || resultType == null)
            {
                if (isTreeValid)
                    diagnostics.ReportTypeError(ErrorMessage.UnsupportedBinaryOperator, syntax.Op.Span, syntax.Op.Value.ToString(), left.ResultType, right.ResultType);
                isTreeValid = false;
                op = BoundBinaryOperator.Addition;
                resultType = TypeSymbol.ErrorType;
            }
            var binaryExpression = new BoundBinaryExpression((BoundBinaryOperator)op, left, right, resultType, isTreeValid);
            return new BoundAssignementExpression(variable, binaryExpression, isTreeValid);
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
        {
            if (!scope.TryLookUpVariable((string)syntax.Identifier.Value, out var variable))
            {
                if (isTreeValid)
                    diagnostics.ReportIdentifierError(ErrorMessage.UnresolvedIdentifier, syntax.Identifier.Span, (string)syntax.Identifier.Value);
                isTreeValid = false;
            }
            var expr = CheckTypeAndConversion(variable.Type, syntax.Expression);
            return new BoundAssignementExpression(variable, expr, isTreeValid);

        }

        private BoundExpression BindVariableExpression(VariableExpressionSyntax syntax)
        {
            var identifier = (string)syntax.Name.Value;
            if (!scope.TryLookUpVariable(identifier, out VariableSymbol variable))
            {
                if (isTreeValid)
                    diagnostics.ReportIdentifierError(ErrorMessage.UnresolvedIdentifier, syntax.Name.Span, syntax.Name.Value);
                isTreeValid = false;
            }
            return new BoundVariableExpression(variable, isTreeValid);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var left = BindExpression(syntax.Left);
            var right = BindExpression(syntax.Right);
            var boundOperator = BindBinaryOperator(syntax.Op.Kind);
            var resultType = BindFacts.ResolveBinaryType(boundOperator, left.ResultType, right.ResultType);

            if (boundOperator == null || resultType == null)
            {
                if (isTreeValid)
                    diagnostics.ReportTypeError(ErrorMessage.UnsupportedBinaryOperator, syntax.Op.Span, syntax.Op.Value.ToString(), left.ResultType, right.ResultType);
                isTreeValid = false;
                resultType = TypeSymbol.ErrorType;
                boundOperator = BoundBinaryOperator.Addition;
            }


            return new BoundBinaryExpression((BoundBinaryOperator)boundOperator, left, right, resultType, isTreeValid);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var right = BindExpression(syntax.Expression);
            var boundOperator = BindUnaryOperator(syntax.Op.Kind);
            var resultType = BindFacts.ResolveUnaryType(boundOperator, right.ResultType);

            if (boundOperator == null || resultType == null)
            {
                if (isTreeValid)
                    diagnostics.ReportTypeError(ErrorMessage.UnsupportedUnaryOperator, syntax.Op.Span, syntax.Op.Value.ToString(), right.ResultType);
                isTreeValid = false;
                resultType = TypeSymbol.ErrorType;
                boundOperator = BoundUnaryOperator.Identety;
            }

            return new BoundUnaryExpression((BoundUnaryOperator)boundOperator, right, resultType, isTreeValid);
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
        {
            var value = syntax.Literal.Value;
            var type = BindFacts.GetTypeSymbol(syntax.Literal.Kind);
            return new BoundLiteralExpression(value, type, isTreeValid);
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
    }
}
