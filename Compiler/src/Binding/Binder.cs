


using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Compiler.Diagnostics;
using Compiler.Syntax;
using static Compiler.Binding.BindFacts;

namespace Compiler.Binding
{
    public class VariableSymbol
    {
        public VariableSymbol(string identifier, TypeSymbol type, dynamic value)
        {
            Identifier = identifier;
            Type = type;
            Value = value;
        }

        public string Identifier { get; }
        public TypeSymbol Type { get; }
        public dynamic Value { get; }
    }

    internal sealed class BoundScope
    {
        private Dictionary<string, VariableSymbol> variables;

        public BoundScope Parent { get; }

        public BoundScope(BoundScope parent)
        {
            Parent = parent;
            variables = new Dictionary<string, VariableSymbol>();
        }

        public bool TryLookUp(string identifier, out VariableSymbol value)
        {
            if (variables.TryGetValue(identifier, out value))
                return true;

            if (Parent == null) return false;

            return Parent.TryLookUp(identifier, out value);

        }

        public bool TryDeclare(VariableSymbol variable)
        {
            if (variables.ContainsKey(variable.Identifier))
                return false;
            variables.Add(variable.Identifier, variable);
            return true;
        }

        public ImmutableArray<VariableSymbol> GetDeclaredVariables()
        {
            return variables.Values.ToImmutableArray();
        }
    }

    internal sealed class Binder
    {
        public DiagnosticBag Diagnostics { get; }
        public BoundScope Scope { get; }

        private Binder(DiagnosticBag diagnostics, BoundScope parentScope)
        {
            Diagnostics = diagnostics;
            Scope = new BoundScope(parentScope);
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

        public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, CompilationUnitSyntax unit)
        {
            var bag = new DiagnosticBag();

            var parentScope = CreateBoundScopes(previous);
            var binder = new Binder(bag, parentScope);
            var expression = binder.BindExpression(unit.Expression);
            var variables = binder.Scope.GetDeclaredVariables();
            return new BoundGlobalScope(previous, bag, variables, expression);
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
            if (Scope.TryLookUp(ee.Identifier.Value, out VariableSymbol variable))
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
            if (!Scope.TryLookUp(identifier, out VariableSymbol variable))
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