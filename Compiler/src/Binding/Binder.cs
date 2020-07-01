using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
        private readonly bool isScript;
        private BoundScope scope;

        public Binder(Compilation previous, bool isScript)
        {
            this.previous = previous;
            this.isScript = isScript;
            diagnostics = new DiagnosticBag();
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
            var stmt = BindStatement(unit.Statement);

            if (stmt is BoundInvalidStatement inv)
                return null;

            var variables = scope.GetDeclaredVariables();
            var functions = scope.GetDeclaredFunctions();
            return new BoundCompilationUnit(stmt, variables, functions);
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
            else if (statement is DoWhileStatementSyntax dws)
                return BindDoWhileStatement(dws);
            else if (statement is ForStatementSyntax fs)
            {
                scope = new BoundScope(scope);
                var res = BindForStatement(fs);
                scope = scope.Parent;
                return res;
            }
            else throw new Exception($"Unexpected StatementSyntax <{statement}>");
        }

        private BoundStatement BindDoWhileStatement(DoWhileStatementSyntax dws)
        {
            var body = BindStatement(dws.Body);
            if (body is BoundInvalidStatement)
                return new BoundInvalidStatement();

            var condition = CheckTypeAndConversion(TypeSymbol.Bool, dws.Condition);

            if (condition is BoundInvalidExpression)
                return new BoundInvalidStatement();

            return new BoundDoWhileStatement(body, condition);
        }

        private BoundStatement BindForStatement(ForStatementSyntax fs)
        {
            var variableDecl = BindStatement(fs.VariableDecleration);

            if (variableDecl is BoundInvalidStatement)
                return new BoundInvalidStatement();

            var condition = CheckTypeAndConversion(TypeSymbol.Bool, fs.Condition);

            if (condition is BoundInvalidExpression)
                return new BoundInvalidStatement();

            var increment = Fett(fs.Increment);

            if (increment is BoundInvalidExpression)
                return new BoundInvalidStatement();

            var body = BindStatement(fs.Body);

            if (body is BoundInvalidStatement)
                return new BoundInvalidStatement();

            return new BoundForStatement(variableDecl, condition, increment, body);
        }

        private BoundStatement BindWhileStatement(WhileStatementSyntax ws)
        {
            var condition = CheckTypeAndConversion(TypeSymbol.Bool, ws.Condition);

            if (condition is BoundInvalidExpression)
                return new BoundInvalidStatement();

            var body = BindStatement(ws.Body);

            if (body is BoundInvalidStatement)
                return new BoundInvalidStatement();

            return new BoundWhileStatement(condition, body);
        }

        private BoundStatement BindIfStatement(IfStatementSyntax ifs)
        {
            var condition = CheckTypeAndConversion(TypeSymbol.Bool, ifs.Condition);

            if (condition is BoundInvalidExpression)
                return new BoundInvalidStatement();

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
            TypeSymbol type;
            BoundExpression expr;

            if (vs.TypeToken.Kind == SyntaxTokenKind.VarKeyword)
            {
                expr = Fett(vs.Expression);
                type = expr.ResultType;
            }
            else
            {
                var declaredType = BindFacts.GetTypeSymbol(vs.TypeToken.Kind);
                expr = CheckTypeAndConversion(declaredType, vs.Expression);
                type = declaredType;
            }

            if (expr is BoundInvalidExpression)
                return new BoundInvalidStatement();



            var variable = new VariableSymbol((string)vs.Identifier.Value, type);
            if (!scope.TryDeclareVariable(variable))
            {
                diagnostics.ReportIdentifierError(ErrorMessage.VariableAlreadyDeclared, vs.Identifier.Span, variable.Name);
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
            var expr = Fett(es.Expression, true);
            if (expr is BoundInvalidExpression)
                return new BoundInvalidStatement();

            return new BoundExpressionStatement(expr);
        }

        private BoundExpression Fett(ExpressionSyntax syntax, bool canBeVoid = false)
        {
            var res = BindExpressionInternal(syntax);
            if (!canBeVoid && res.ResultType == TypeSymbol.Void)
            {
                diagnostics.ReportTypeError(ErrorMessage.CannotBeVoid, syntax.Span);
                return new BoundInvalidExpression();
            }
            return res;
        }

        private BoundExpression CheckTypeAndConversion(TypeSymbol type, ExpressionSyntax expression)
        {
            var expr = Fett(expression);

            if (expr is BoundInvalidExpression)
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

        private BoundExpression BindExpressionInternal(ExpressionSyntax syntax)
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
            else if (syntax is CallExpressionSyntax cs)
                return BindCallExpession(cs);
            else throw new Exception($"Unknown Syntax kind <{syntax}>");
        }

        private BoundExpression BindCallExpession(CallExpressionSyntax cs)
        {
            if (cs.Arguments.Arguments.Length == 1 && TypeSymbol.Lookup((string)cs.Identifier.Value) is TypeSymbol type)
                return BindExplicitConversion(type, cs.Arguments[0]);

            if (!scope.TryLookUpFunction((string)cs.Identifier.Value, out var symbol))
            {
                diagnostics.ReportIdentifierError(ErrorMessage.UnresolvedIdentifier, cs.Identifier.Span, (string)cs.Identifier.Value);
                return new BoundInvalidExpression();
            }

            if (cs.Arguments.Arguments.Length != symbol.Parameters.Length)
            {
                diagnostics.ReportSyntaxError(ErrorMessage.WrongAmountOfArguments, cs.Arguments.LeftParenthesis.Span + cs.Arguments.RightParenthesis.Span, symbol.Name, symbol.Parameters.Length, cs.Arguments.Arguments.Length);
                return new BoundInvalidExpression();
            }


            var argBuilder = ImmutableArray.CreateBuilder<BoundExpression>(symbol.Parameters.Length);

            for (int i = 0; i < symbol.Parameters.Length; i++)
            {
                var arg = cs.Arguments[i];
                var param = symbol.Parameters[i];

                var boundArg = CheckTypeAndConversion(param.Type, arg);

                if (boundArg is BoundInvalidExpression)
                    return new BoundInvalidExpression();

                argBuilder.Add(boundArg);
            }

            return new BoundCallExpression(symbol, argBuilder.MoveToImmutable());
        }

        private BoundExpression BindExplicitConversion(TypeSymbol type, ExpressionSyntax expressionSyntax)
        {
            var expr = Fett(expressionSyntax);

            if (expr is BoundInvalidExpression)
                return new BoundInvalidExpression();

            var conversion = BindFacts.ClassifyConversion(expr.ResultType, type);

            if (conversion == ConversionType.None)
            {
                diagnostics.ReportTypeError(ErrorMessage.CannotConvert, expressionSyntax.Span, expr.ResultType, type);
                return new BoundInvalidExpression();
            }

            return new BoundConversionExpression(type, expr);
        }

        private BoundExpression BindPostIncDecExpression(PostIncDecExpression ide)
        {
            if (!scope.TryLookUpVariable((string)ide.Identifier.Value, out VariableSymbol variable))
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
            if (!scope.TryLookUpVariable((string)ae.Identifier.Value, out VariableSymbol variable))
            {
                diagnostics.ReportIdentifierError(ErrorMessage.UnresolvedIdentifier, ae.Identifier.Span, (string)ae.Identifier.Value);
                return new BoundInvalidExpression();
            }

            var left = new BoundVariableExpression(variable);
            var right = Fett(ae.Expression);

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
            if (!scope.TryLookUpVariable((string)ee.Identifier.Value, out var variable))
            {
                diagnostics.ReportIdentifierError(ErrorMessage.UnresolvedIdentifier, ee.Identifier.Span, (string)ee.Identifier.Value);
                return new BoundInvalidExpression();
            }

            var expr = CheckTypeAndConversion(variable.Type, ee.Expression);

            if (expr is BoundInvalidExpression)
                return new BoundInvalidExpression();

            else return new BoundAssignementExpression(variable, expr);

        }

        private BoundExpression BindVariableExpression(VariableExpressionSyntax ve)
        {
            var identifier = (string)ve.Name.Value;
            if (!scope.TryLookUpVariable(identifier, out VariableSymbol variable))
            {
                diagnostics.ReportIdentifierError(ErrorMessage.UnresolvedIdentifier, ve.Name.Span, ve.Name.Value);
                return new BoundInvalidExpression();
            }
            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax be)
        {
            var left = Fett(be.Left);
            var right = Fett(be.Right);

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
            var right = Fett(ue.Expression);
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
