


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
        public DiagnosticBag Diagnostics { get; }
        private BoundScope scope;

        private Binder(DiagnosticBag diagnostics, BoundScope parentScope)
        {
            Diagnostics = diagnostics;
            scope = new BoundScope(parentScope);
        }

        private static BoundScope CreateBoundScopes(BoundGlobalScope previous)
        {
            var stack = new Stack<BoundGlobalScope>();

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
                foreach (var variable in global.Variables)
                    scope.TryDeclare(variable);

                current = scope;
            }

            return current;
        }

        public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, CompilationUnitSyntax unit, DiagnosticBag bag)
        {
            var parentScope = CreateBoundScopes(previous);
            var binder = new Binder(bag, parentScope);
            var stmt = binder.BindStatement(unit.Statement);
            var variables = binder.scope.GetDeclaredVariables();
            return new BoundGlobalScope(previous, bag, variables, stmt);
        }

        private BoundStatement BindStatement(StatementSyntax statement)
        {
            if (statement is ExpressionStatement es)
                return BindExpressionStatement(es);
            else if (statement is BlockStatment bs)
                return BindBlockStatement(bs);
            else if (statement is VariableDeclerationStatement vs)
                return BindVariableDeclerationStatement(vs);
            else if (statement is InvalidStatementSyntax)
                return new BoundInvalidStatement();
            else throw new Exception($"Unexpected StatementSyntax <{statement}>");
        }

        private BoundStatement BindVariableDeclerationStatement(VariableDeclerationStatement vs)
        {
            var expr = BindExpression(vs.Expression);

            TypeSymbol type;

            if (vs.TypeToken.Kind == SyntaxTokenKind.Var) type = expr.ResultType;
            else type = BindFacts.GetTypeSymbol(vs.TypeToken.Kind);

            if (expr.ResultType != type)
            {
                Diagnostics.ReportWrongType(type, expr.ResultType, expr.Span);
                return new BoundInvalidStatement();
            }
            var variable = new VariableSymbol(vs.Identifier.Value, type, null);
            if (!scope.TryDeclare(variable))
            {
                Diagnostics.ReportVariableAlreadyDeclared(variable.Identifier, vs.Identifier.Span);
                return new BoundInvalidStatement();
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
                return new BoundInvalidExpression();
            else throw new Exception($"Unknown Syntax kind <{syntax}>");
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax ee)
        {
            var expr = BindExpression(ee.Expression);
            if (!scope.TryLookUp(ee.Identifier.Value, out VariableSymbol variable))
            {
                Diagnostics.ReportVariableNotDeclared(ee.Identifier.Value, ee.Identifier.Span);
                return new BoundInvalidExpression();
            }
            else if (variable.Type != expr.ResultType)
            {
                Diagnostics.ReportWrongType(variable.Type, expr.ResultType, ee.EqualToken.Span);
                return new BoundInvalidExpression();
            }
            else return new BoundAssignementExpression(variable, expr, ee.Span, ee.EqualToken.Span);

        }

        private BoundExpression BindVariableExpression(VariableExpressionSyntax ve)
        {
            string identifier = ve.Name.Value;
            if (!scope.TryLookUp(identifier, out VariableSymbol variable))
            {
                Diagnostics.ReportVariableNotDeclared(ve.Name.Value, ve.Name.Span);
                return new BoundInvalidExpression();
            }
            return new BoundVariableExpression(variable, ve.Span);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax be)
        {
            var left = BindExpression(be.Left);
            var right = BindExpression(be.Right);
            var boundOperator = BindBinaryOperator(be.Op);

            var resultType = ResolveBinaryType(boundOperator, left.ResultType, right.ResultType);

            if (boundOperator == null || resultType == null)
            {
                Diagnostics.ReportUnsupportedBinaryOperator(be.Op.Value, left.ResultType, right.ResultType, be.Op.Span);
                return new BoundInvalidExpression();
            }


            return new BoundBinaryExpression((BoundBinaryOperator)boundOperator, be.Op.Span, left, right, (TypeSymbol)resultType);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax ue)
        {
            var right = BindExpression(ue.Expression);
            var boundOperator = BindUnaryOperator(ue.Op);

            var resultType = ResolveUnaryType(boundOperator, right.ResultType);

            if (boundOperator == null || resultType == null)
            {
                Diagnostics.ReportUnsupportedUnaryOperator(ue.Op.Value, right.ResultType, ue.Op.Span);
                return new BoundInvalidExpression();
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