using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Compiler.Diagnostics;
using Compiler.Symbols;
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

        public BoundCompilationUnit BindCompilationUnit(CompilationUnitSyntax unit)
        {
            var stmt = BindStatement(unit.Statement);

            if (stmt is BoundInvalidStatement inv)
                return null;

            var variables = scope.GetDeclaredVariables();
            return new BoundCompilationUnit(stmt, variables);
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
                    scope.TryDeclare(variable);

                current = scope;
            }

            return current;
        }

        private BoundStatement BindStatement(StatementSyntax statement)
        {
            if (!statement.IsValid)
                return new BoundInvalidStatement();
            else if (statement is ExpressionStatement es)
                return BindExpressionStatement(es);
            else if (statement is BlockStatment bs)
                return BindBlockStatement(bs);
            else if (statement is VariableDeclerationStatement vs)
                return BindVariableDeclerationStatement(vs);
            else if (statement is IfStatementSyntax ifs)
                return BindIfStatement(ifs);
            else if (statement is WhileStatementSyntax ws)
                return BindWhileStatement(ws);
            else if (statement is PrintStatementSyntax ps)
                return BindPrintStatement(ps);
            else if (statement is ForStatementSyntax fs)
            {
                scope = new BoundScope(scope);
                var res = BindForStatement(fs);
                scope = scope.Parent;
                return res;
            }
            else throw new Exception($"Unexpected StatementSyntax <{statement}>");
        }

        private BoundStatement BindForStatement(ForStatementSyntax fs)
        {
            var variableDecl = BindStatement(fs.VariableDecleration);

            if (variableDecl is BoundInvalidStatement)
                return new BoundInvalidStatement();

            var condition = BindExpression(fs.Condition);

            if (condition is BoundInvalidExpression)
                return new BoundInvalidStatement();

            if (condition.ResultType != TypeSymbol.Bool)
            {
                diagnostics.ReportTypeError(ErrorMessage.IncompatibleTypes, fs.Condition.Span, TypeSymbol.Bool, condition.ResultType);
                return new BoundInvalidStatement();
            }

            var increment = BindExpression(fs.Increment);

            if (increment is BoundInvalidExpression)
                return new BoundInvalidStatement();

            var body = BindStatement(fs.Body);

            if (body is BoundInvalidStatement)
                return new BoundInvalidStatement();

            return new BoundForStatement(variableDecl, condition, increment, body);
        }

        private BoundStatement BindPrintStatement(PrintStatementSyntax ps)
        {
            var expr = BindExpression(ps.Expression);

            if (expr is BoundInvalidExpression)
                return new BoundInvalidStatement();
            return new BoundPrintStatement(expr);
        }

        private BoundStatement BindWhileStatement(WhileStatementSyntax ws)
        {
            var condition = BindExpression(ws.Condition);

            if (condition is BoundInvalidExpression)
                return new BoundInvalidStatement();

            if (condition.ResultType != TypeSymbol.Bool)
            {
                diagnostics.ReportTypeError(ErrorMessage.IncompatibleTypes, ws.Condition.Span, TypeSymbol.Bool, condition.ResultType);
                return new BoundInvalidStatement();
            }

            var body = BindStatement(ws.Body);

            if (body is BoundInvalidStatement)
                return new BoundInvalidStatement();

            return new BoundWhileStatement(condition, body);
        }

        private BoundStatement BindIfStatement(IfStatementSyntax ifs)
        {
            var condition = BindExpression(ifs.Condition);

            if (condition is BoundInvalidExpression)
                return new BoundInvalidStatement();

            if (condition.ResultType != TypeSymbol.Bool)
            {
                diagnostics.ReportTypeError(ErrorMessage.IncompatibleTypes, ifs.Condition.Span, TypeSymbol.Bool, condition.ResultType);
                return new BoundInvalidStatement();
            }

            var stmt = BindStatement(ifs.Body);

            if (stmt is BoundInvalidStatement)
                return new BoundInvalidStatement();

            var elseStmt = ifs.ElseStatement == null ? null : BindStatement(ifs.ElseStatement.Body);

            if (elseStmt != null && elseStmt is BoundInvalidStatement)
                return new BoundInvalidStatement();

            return new BoundIfStatement(condition, stmt, elseStmt);
        }

        private BoundStatement BindVariableDeclerationStatement(VariableDeclerationStatement vs)
        {
            var expr = BindExpression(vs.Expression);

            if (expr is BoundInvalidExpression)
                return new BoundInvalidStatement();

            TypeSymbol type;

            if (vs.TypeToken.Kind == SyntaxTokenKind.VarKeyword) type = expr.ResultType;
            else type = BindFacts.GetTypeSymbol(vs.TypeToken.Kind);

            if (type != TypeSymbol.Object && expr.ResultType != type)
            {
                diagnostics.ReportTypeError(ErrorMessage.IncompatibleTypes, vs.Expression.Span, type, expr.ResultType);
                return new BoundInvalidStatement();
            }

            var variable = new VariableSymbol((string)vs.Identifier.Value, type, null);
            if (!scope.TryDeclare(variable))
            {
                diagnostics.ReportIdentifierError(ErrorMessage.VariableAlreadyDeclared, vs.Identifier.Span, variable.Identifier);
                return new BoundInvalidStatement();
            }
            return new BoundVariableDecleration(variable, expr);
        }

        private BoundStatement BindBlockStatement(BlockStatment bs)
        {
            var builder = ImmutableArray.CreateBuilder<BoundStatement>();
            scope = new BoundScope(scope);

            foreach (var stmt in bs.Statements)
            {
                if (!stmt.IsValid)
                {
                    scope = scope.Parent;
                    return new BoundInvalidStatement();
                }
                builder.Add(BindStatement(stmt));
            }

            scope = scope.Parent;

            return new BoundBlockStatement(builder.ToImmutable());
        }

        private BoundStatement BindExpressionStatement(ExpressionStatement es)
        {
            var expr = BindExpression(es.Expression);
            if (expr is BoundInvalidExpression)
                return new BoundInvalidStatement();

            return new BoundExpressionStatement(expr);
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax)
        {
            if (!syntax.IsValid)
                return new BoundInvalidExpression();
            else if (syntax is LiteralExpressionSyntax le)
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
            else throw new Exception($"Unknown Syntax kind <{syntax}>");
        }

        private BoundExpression BindPostIncDecExpression(PostIncDecExpression ide)
        {
            if (!scope.TryLookUp((string)ide.Identifier.Value, out VariableSymbol variable))
            {
                diagnostics.ReportIdentifierError(ErrorMessage.UnresolvedIdentifier, ide.Identifier.Span, (string)ide.Identifier.Value);
                return new BoundInvalidExpression();
            }

            var left = new BoundVariableExpression(variable);
            var right = new BoundLiteralExpression(1, TypeSymbol.Int);

            var op = BindBinaryOperator(ide.Op.Kind);

            var resultType = ResolveBinaryType(op, left.ResultType, right.ResultType);

            if (op == null || resultType == null)
            {
                diagnostics.ReportTypeError(ErrorMessage.UnsupportedBinaryOperator, ide.Op.Span, ide.Op.Value.ToString(), left.ResultType, right.ResultType);
                return new BoundInvalidExpression();
            }

            var binaryExpression = new BoundBinaryExpression((BoundBinaryOperator)op, left, right, (TypeSymbol)resultType);
            return new BoundAssignementExpression(variable, binaryExpression);
        }

        private BoundExpression BindAdditioalAssignmentExpression(AdditionalAssignmentExpression ae)
        {
            if (!scope.TryLookUp((string)ae.Identifier.Value, out VariableSymbol variable))
            {
                diagnostics.ReportIdentifierError(ErrorMessage.UnresolvedIdentifier, ae.Identifier.Span, (string)ae.Identifier.Value);
                return new BoundInvalidExpression();
            }

            var left = new BoundVariableExpression(variable);
            var right = BindExpression(ae.Expression);

            if (right is BoundInvalidExpression)
                return new BoundInvalidExpression();

            var op = BindBinaryOperator(ae.Op.Kind);
            var resultType = ResolveBinaryType(op, left.ResultType, right.ResultType);

            if (op == null || resultType == null)
            {
                diagnostics.ReportTypeError(ErrorMessage.UnsupportedBinaryOperator, ae.Op.Span, ae.Op.Value.ToString(), left.ResultType, right.ResultType);
                return new BoundInvalidExpression();
            }
            var binaryExpression = new BoundBinaryExpression((BoundBinaryOperator)op, left, right, (TypeSymbol)resultType);
            return new BoundAssignementExpression(variable, binaryExpression);
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax ee)
        {
            var expr = BindExpression(ee.Expression);

            if (expr is BoundInvalidExpression)
                return new BoundInvalidExpression();

            if (!scope.TryLookUp((string)ee.Identifier.Value, out VariableSymbol variable))
            {
                diagnostics.ReportIdentifierError(ErrorMessage.UnresolvedIdentifier, ee.Identifier.Span, (string)ee.Identifier.Value);
                return new BoundInvalidExpression();
            }
            else if (variable.Type != expr.ResultType)
            {
                diagnostics.ReportTypeError(ErrorMessage.IncompatibleTypes, ee.EqualToken.Span, variable.Type, expr.ResultType);
                return new BoundInvalidExpression();
            }
            else return new BoundAssignementExpression(variable, expr);

        }

        private BoundExpression BindVariableExpression(VariableExpressionSyntax ve)
        {
            var identifier = (string)ve.Name.Value;
            if (!scope.TryLookUp(identifier, out VariableSymbol variable))
            {
                diagnostics.ReportIdentifierError(ErrorMessage.UnresolvedIdentifier, ve.Name.Span, ve.Name.Value);
                return new BoundInvalidExpression();
            }
            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax be)
        {
            var left = BindExpression(be.Left);
            var right = BindExpression(be.Right);

            if (left is BoundInvalidExpression || right is BoundInvalidExpression)
                return new BoundInvalidExpression();

            var boundOperator = BindBinaryOperator(be.Op.Kind);
            var resultType = ResolveBinaryType(boundOperator, left.ResultType, right.ResultType);

            if (boundOperator == null || resultType == null)
            {
                diagnostics.ReportTypeError(ErrorMessage.UnsupportedBinaryOperator, be.Op.Span, be.Op.Value.ToString(), left.ResultType, right.ResultType);
                return new BoundInvalidExpression();
            }


            return new BoundBinaryExpression((BoundBinaryOperator)boundOperator, left, right, (TypeSymbol)resultType);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax ue)
        {
            var right = BindExpression(ue.Expression);
            if (right is BoundInvalidExpression)
                return new BoundInvalidExpression();

            var boundOperator = BindUnaryOperator(ue.Op.Kind);

            var resultType = ResolveUnaryType(boundOperator, right.ResultType);

            if (boundOperator == null || resultType == null)
            {
                diagnostics.ReportTypeError(ErrorMessage.UnsupportedUnaryOperator, ue.Op.Span, ue.Op.Value.ToString(), right.ResultType);
                return new BoundInvalidExpression();
            }

            return new BoundUnaryExpression((BoundUnaryOperator)boundOperator, right, (TypeSymbol)resultType);
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax le)
        {
            var value = le.Literal.Value;
            var type = BindFacts.GetTypeSymbol(le.Literal.Kind);

            return new BoundLiteralExpression(value, type);
        }
    }
}