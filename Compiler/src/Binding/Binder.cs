using System;
using Compiler.Diagnostics;
using Compiler.Syntax;

namespace Compiler.Binding
{
    internal sealed class Binder
    {
        public DiagnosticBag Diagnostics { get; }

        public Binder(DiagnosticBag diagnostics)
        {
            Diagnostics = diagnostics;
        }

        public BoundExpression BindExpression(ExpressionSyntax syntax)
        {
            if (syntax is LiteralExpressionSyntax le)
                return BindLiteralExpression(le);
            else if (syntax is UnaryExpressionSyntax ue)
                return BindUnaryExpression(ue);
            else if (syntax is BinaryExpressionSyntax be)
                return BindBinaryExpression(be);
            else throw new Exception($"Unknown Syntax kind <{syntax}>");
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax be)
        {
            var left = BindExpression(be.Left);
            var right = BindExpression(be.Right);
            var boundOperator = BindBinaryOperator(be.Op, left.ResultType, right.ResultType);

            var resultType = BindFacts.ResolveBinaryType(boundOperator, left.ResultType, right.ResultType);

            if (boundOperator == null || resultType == null)
            {
                Diagnostics.ReportUnsupportedBinaryOperator(be, left, right);
                return new BoundInvalidExpression();
            }


            return new BoundBinaryExpression((BoundBinaryOperator)boundOperator, be.Op.Span, left, right, (TypeSymbol)resultType);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax ue)
        {
            var right = BindExpression(ue.Expression);
            var boundOperator = BindUnaryOperator(ue.Op, right.ResultType);

            var resultType = BindFacts.ResolveUnaryType(boundOperator, right.ResultType);

            if (boundOperator == null || resultType == null)
            {
                Diagnostics.ReportUnsupportedUnaryOperator(ue, right);
                return new BoundInvalidExpression();
            }

            return new BoundUnaryExpression((BoundUnaryOperator)boundOperator, ue.Op.Span, right, (TypeSymbol)resultType);
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax le)
        {
            var value = le.Literal.Value;
            var type = le.Literal.Kind.GetTypeSymbol();
            return new BoundLiteralExpression(le.Span, value, type);
        }

        private BoundBinaryOperator? BindBinaryOperator(SyntaxToken op, TypeSymbol leftType, TypeSymbol rightType)
        {
            BoundBinaryOperator boundOp;

            switch (op.Kind)
            {
                case SyntaxTokenKind.Plus: boundOp = BoundBinaryOperator.Addition; break;
                case SyntaxTokenKind.Minus: boundOp = BoundBinaryOperator.Subtraction; break;
                case SyntaxTokenKind.Star: boundOp = BoundBinaryOperator.Multiplication; break;
                case SyntaxTokenKind.Slash: boundOp = BoundBinaryOperator.Division; break;
                case SyntaxTokenKind.StarStar: boundOp = BoundBinaryOperator.Power; break;
                case SyntaxTokenKind.SlashSlah: boundOp = BoundBinaryOperator.Root; break;
                case SyntaxTokenKind.EqualEqual: boundOp = BoundBinaryOperator.EqualEqual; break;
                case SyntaxTokenKind.NotEqual: boundOp = BoundBinaryOperator.NotEqual; break;
                case SyntaxTokenKind.LessThan: boundOp = BoundBinaryOperator.LessThan; break;
                case SyntaxTokenKind.LessEqual: boundOp = BoundBinaryOperator.LessEqual; break;
                case SyntaxTokenKind.GreaterThan: boundOp = BoundBinaryOperator.GreaterThan; break;
                case SyntaxTokenKind.GreaterEqual: boundOp = BoundBinaryOperator.GreaterEqual; break;
                case SyntaxTokenKind.AmpersandAmpersand: boundOp = BoundBinaryOperator.LogicalAnd; break;
                case SyntaxTokenKind.PipePipe: boundOp = BoundBinaryOperator.LogicalOr; break;
                default: return null;
            }

            return boundOp;
        }

        private BoundUnaryOperator? BindUnaryOperator(SyntaxToken op, TypeSymbol type)
        {
            BoundUnaryOperator boundOp;

            switch (op.Kind)
            {
                case SyntaxTokenKind.Plus: boundOp = BoundUnaryOperator.Identety; break;
                case SyntaxTokenKind.Minus: boundOp = BoundUnaryOperator.Negation; break;
                case SyntaxTokenKind.Bang: boundOp = BoundUnaryOperator.LogicalNot; break;
                default: return null;
            }
            return boundOp;
        }
    }
}