


using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Compiler.Diagnostics;
using Compiler.Syntax;
using static Compiler.Binding.BindFacts;

namespace Compiler.Binding
{

    internal sealed class Binder
    {
        private readonly DiagnosticBag diagnostics;
        private readonly Compilation previous;
        private BoundScope scope;

        public Binder(Compilation previous)
        {
            diagnostics = new DiagnosticBag();
            var parentScope = CreateBoundScopes(previous);
            scope = new BoundScope(parentScope);
            this.previous = previous;
        }

        public IEnumerable<Diagnostic> GetDiagnostics() => diagnostics;

        private BoundScope CreateBoundScopes(Compilation previous)
        {
            var stack = new Stack<Compilation>();

            while (previous != null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }

            BoundScope current = null;

            while (stack.Count > 0)
            {
                var global = stack.Pop();
                var scope = new BoundScope(current);
                foreach (var variable in global.Root.DeclaredVariables)
                    scope.TryDeclare(variable);

                current = scope;
            }

            return current;
        }

        public BoundCompilationUnit BindCompilationUnit(CompilationUnitSyntax unit)
        {
            var stmt = BindStatement(unit.Statement);
            var variables = scope.GetDeclaredVariables();
            return new BoundCompilationUnit(stmt, variables, unit.Span);
        }

        private BoundStatement BindStatement(StatementSyntax statement)
        {
            if (statement is ExpressionStatement es)
                return BindExpressionStatement(es);
            else if (statement is BlockStatment bs)
                return BindBlockStatement(bs);
            else if (statement is VariableDeclerationStatement vs)
                return BindVariableDeclerationStatement(vs);
            else if (statement is IfStatement ifs)
                return BindIfStatement(ifs);
            else if (statement is InvalidStatementSyntax invalid)
                return new BoundInvalidStatement(invalid.Span);
            else throw new Exception($"Unexpected StatementSyntax <{statement}>");
        }

        private BoundStatement BindIfStatement(IfStatement ifs)
        {
            var condition = BindExpression(ifs.Expression);

            if (condition.ResultType != TypeSymbol.Bool)
            {
                diagnostics.ReportTypeError(ErrorMessage.IncompatibleTypes, condition.Span, TypeSymbol.Bool, condition.ResultType);
                return new BoundInvalidStatement(condition.Span);
            }

            var stmt = BindStatement(ifs.ThenStatement);
            var elseStmt = ifs.ElseStatement == null ? null : BindStatement(ifs.ElseStatement.ThenStatement);
            return new BoundIfStatement(condition, stmt, elseStmt, ifs.IfToken.Span);
        }

        private BoundStatement BindVariableDeclerationStatement(VariableDeclerationStatement vs)
        {
            var expr = BindExpression(vs.Expression);

            TypeSymbol type;

            if (vs.TypeToken.Kind == SyntaxTokenKind.VarKeyword) type = expr.ResultType;
            else type = BindFacts.GetTypeSymbol(vs.TypeToken.Kind);

            if (expr.ResultType != type)
            {
                diagnostics.ReportTypeError(ErrorMessage.IncompatibleTypes, expr.Span, type, expr.ResultType);
                return new BoundInvalidStatement(expr.Span);
            }
            var name = vs.Identifier.Value;

            if (!(name is string))
                return new BoundInvalidStatement(vs.Identifier.Span);

            var variable = new VariableSymbol((string)vs.Identifier.Value, type, null);
            if (!scope.TryDeclare(variable))
            {
                diagnostics.ReportIdentifierError(ErrorMessage.VariableAlreadyDeclared, vs.Identifier.Span, variable.Identifier);
                return new BoundInvalidStatement(vs.Identifier.Span);
            }
            return new BoundVariableDeclerationStatement(variable, expr, vs.TypeToken.Span, vs.Identifier.Span, vs.EqualToken.Span, expr.Span);
        }

        private BoundStatement BindBlockStatement(BlockStatment bs)
        {
            var builder = ImmutableArray.CreateBuilder<BoundStatement>();
            scope = new BoundScope(scope);

            foreach (var stmt in bs.Statements)
                builder.Add(BindStatement(stmt));

            scope = scope.Parent;

            return new BoundBlockStatement(bs.OpenCurly.Span, builder.ToImmutable(), bs.CloseCurly.Span);
        }

        private BoundStatement BindExpressionStatement(ExpressionStatement es)
        {
            var expr = BindExpression(es.Expression);
            return new BoundExpressionStatement(expr, es.Span);
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax)
        {
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
            else if (syntax is InvalidExpressionSyntax ie)
                return new BoundInvalidExpression(ie.Span);
            else throw new Exception($"Unknown Syntax kind <{syntax}>");
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax ee)
        {
            var expr = BindExpression(ee.Expression);
            if (!scope.TryLookUp((string)ee.Identifier.Value, out VariableSymbol variable))
            {
                diagnostics.ReportIdentifierError(ErrorMessage.UnresolvedIdentifier, ee.Identifier.Span, (string)ee.Identifier.Value);
                return new BoundInvalidExpression(ee.Identifier.Span);
            }
            else if (variable.Type != expr.ResultType)
            {
                diagnostics.ReportTypeError(ErrorMessage.IncompatibleTypes, ee.EqualToken.Span, variable.Type, expr.ResultType);
                return new BoundInvalidExpression(ee.EqualToken.Span);
            }
            else return new BoundAssignementExpression(variable, expr, ee.Span, ee.EqualToken.Span);

        }

        private BoundExpression BindVariableExpression(VariableExpressionSyntax ve)
        {
            var identifier = (string)ve.Name.Value;
            if (!scope.TryLookUp(identifier, out VariableSymbol variable))
            {
                diagnostics.ReportIdentifierError(ErrorMessage.UnresolvedIdentifier, ve.Name.Span, ve.Name.Value);
                return new BoundInvalidExpression(ve.Name.Span);
            }
            return new BoundVariableExpression(variable, ve.Span);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax be)
        {
            var left = BindExpression(be.Left);
            var right = BindExpression(be.Right);
            var boundOperator = BindBinaryOperator(be.Op.Kind);

            var resultType = ResolveBinaryType(boundOperator, left.ResultType, right.ResultType);

            if (boundOperator == null || resultType == null)
            {
                diagnostics.ReportTypeError(ErrorMessage.UnsupportedBinaryOperator, be.Op.Span, be.Op.Value.ToString(), left.ResultType, right.ResultType);
                return new BoundInvalidExpression(be.Op.Span);
            }


            return new BoundBinaryExpression((BoundBinaryOperator)boundOperator, be.Op.Span, left, right, (TypeSymbol)resultType);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax ue)
        {
            var right = BindExpression(ue.Expression);
            var boundOperator = BindUnaryOperator(ue.Op.Kind);

            var resultType = ResolveUnaryType(boundOperator, right.ResultType);

            if (boundOperator == null || resultType == null)
            {
                diagnostics.ReportTypeError(ErrorMessage.UnsupportedUnaryOperator, ue.Op.Span, ue.Op.Value.ToString(), right.ResultType);
                return new BoundInvalidExpression(ue.Op.Span);
            }

            return new BoundUnaryExpression((BoundUnaryOperator)boundOperator, ue.Op.Span, right, (TypeSymbol)resultType);
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax le)
        {
            var value = le.Literal.Value;
            var type = BindFacts.GetTypeSymbol(le.Literal.Kind);
            return new BoundLiteralExpression(le.Span, value, type);
        }
    }
}