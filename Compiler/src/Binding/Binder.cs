using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Compiler.Diagnostics;
using Compiler.Symbols;
using Compiler.Syntax;

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
            foreach (var func in unit.Members.OfType<FunctionDeclarationSyntax>())
                DeclareFunction(func);

            var stmtBuilder = ImmutableArray.CreateBuilder<BoundStatement>();

            foreach (var globalStmt in unit.Members.OfType<GlobalStatementSynatx>())
            {
                var stmt = BindStatement(globalStmt.Statement);
                stmtBuilder.Add(stmt);
            }

            var globalStatements = new BoundBlockStatement(stmtBuilder.ToImmutable());


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
                var type = BindFacts.GetTypeSymbol(parameterSyntax.TypeKeyword.Kind);
                var name = (string)parameterSyntax.Identifier.Value;
                if (!seenParameters.Add(name)) diagnostics.ReportIdentifierError(ErrorMessage.DuplicatedParameters, parameterSyntax.Span, name);
                else parameters.Add(new ParameterSymbol(name, type));
            }

            var returnType = BindFacts.GetTypeSymbol(func.TypeKeyword.Kind);
            var symbol = new FunctionSymbol((string)func.Identifier.Value, parameters.ToImmutable(), returnType);
            if (!scope.TryDeclareFunction(symbol))
                diagnostics.ReportIdentifierError(ErrorMessage.FunctionAlreadyDeclared, func.Identifier.Span, func.Identifier.Value);
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
            else if (statement is VariableDeclarationStatement vs)
                return BindVariableDeclarationStatement(vs);
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
            var variableDecl = BindStatement(fs.VariableDeclaration);

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

        private BoundStatement BindVariableDeclarationStatement(VariableDeclarationStatement vs)
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
            return new BoundVariableDeclaration(variable, expr);
        }

        private BoundStatement BindBlockStatement(BlockStatment bs)
        {
            var builder = ImmutableArray.CreateBuilder<BoundStatement>();
            scope = new BoundScope(scope);

            foreach (var stmt in bs.Statements)
            {
                var bound = BindStatement(stmt);
                if (bound is BoundInvalidStatement)
                {
                    scope = scope.Parent;
                    return new BoundInvalidStatement();
                }
                builder.Add(bound);
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
            if (cs.Arguments.Length == 1 && TypeSymbol.Lookup((string)cs.Identifier.Value) is TypeSymbol type)
                return BindExplicitConversion(type, cs.Arguments[0]);

            if (!scope.TryLookUpFunction((string)cs.Identifier.Value, out var symbol))
            {
                diagnostics.ReportIdentifierError(ErrorMessage.UnresolvedIdentifier, cs.Identifier.Span, (string)cs.Identifier.Value);
                return new BoundInvalidExpression();
            }

            if (cs.Arguments.Length != symbol.Parameters.Length)
            {
                diagnostics.ReportSyntaxError(ErrorMessage.WrongAmountOfArguments, cs.LeftParenthesis.Span + cs.RightParenthesis.Span, symbol.Name, symbol.Parameters.Length, cs.Arguments.Length);
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

            var resultType = BindFacts.ResolveBinaryType(op, left.ResultType, right.ResultType);

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
            var resultType = BindFacts.ResolveBinaryType(op, left.ResultType, right.ResultType);

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
            var resultType = BindFacts.ResolveBinaryType(boundOperator, left.ResultType, right.ResultType);

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

            var resultType = BindFacts.ResolveUnaryType(boundOperator, right.ResultType);

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
