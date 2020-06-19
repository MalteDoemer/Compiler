using System;
using Compiler.Diagnostics;
using Compiler.Syntax;

namespace Compiler.Binding
{
    internal abstract class BoundNode
    {
        public abstract int Pos { get; }
    }

    internal abstract class BoundExpression : BoundNode
    {
        public abstract TypeSymbol Type { get; }
    }

    internal sealed class BoundInvalidExpression : BoundExpression
    {
        public override TypeSymbol Type => TypeSymbol.Null;

        public override int Pos => -1;
    }

    internal sealed class BoundLiteralExpression : BoundExpression
    {
        private readonly int pos;
        private readonly TypeSymbol symbol;

        public BoundLiteralExpression(int pos, dynamic value, TypeSymbol symbol)
        {
            this.pos = pos;
            Value = value;
            this.symbol = symbol;
        }

        public override TypeSymbol Type => symbol;
        public override int Pos => pos;
        public dynamic Value { get; }
    }

    internal enum BoundUnaryOperator
    {
        Identety,
        Negation,
    }

    internal sealed class BoundUnaryExpression : BoundExpression
    {
        private readonly int pos;

        public BoundUnaryExpression(int pos, BoundUnaryOperator op, BoundExpression right)
        {
            this.pos = pos;
            Op = op;
            Right = right;
        }

        public override TypeSymbol Type => Right.Type;

        public BoundUnaryOperator Op { get; }
        public BoundExpression Right { get; }

        public override int Pos => pos;
    }

    internal enum BoundBinaryOperator
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
    }

    internal sealed class BoundBinaryExpression : BoundExpression
    {
        private readonly int pos;

        public BoundBinaryExpression(int pos, BoundBinaryOperator op, BoundExpression left, BoundExpression right)
        {
            this.pos = pos;
            Op = op;
            Left = left;
            Right = right;
        }

        public override TypeSymbol Type => Left.Type;

        public BoundBinaryOperator Op { get; }
        public BoundExpression Left { get; }
        public BoundExpression Right { get; }

        public override int Pos => pos;
    }

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
            else throw new Exception("Unknown Syntax kind");
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax be)
        {
            var left = BindExpression(be.Left);
            var right = BindExpression(be.Right);
            var boundOperator = BindBinaryOperator(be.Op, left.Type, right.Type);

            if (boundOperator == null)
            {
                Diagnostics.ReportUnsupportedBinaryOperator(be.Op, left, right);
                return new BoundInvalidExpression();
            }

            return new BoundBinaryExpression(be.Pos, (BoundBinaryOperator)boundOperator, left, right);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax ue)
        {
            var right = BindExpression(ue.Expression);
            var boundOperator = BindUnaryOperator(ue.Op, right.Type);

            if (boundOperator == null)
            {
                Diagnostics.ReportUnsupportedUnaryOperator(ue.Op, right);
                return new BoundInvalidExpression();
            }

            return new BoundUnaryExpression(ue.Pos, (BoundUnaryOperator)boundOperator, right);
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax le)
        {
            var value = le.Literal.value;
            var type = le.Literal.kind.GetTypeSymbol();
            return new BoundLiteralExpression(le.Pos, value, type);
        }

        private BoundBinaryOperator? BindBinaryOperator(SyntaxToken op, TypeSymbol leftType, TypeSymbol rightType)
        {
            BoundBinaryOperator boundOp;

            switch (op.kind)
            {
                case SyntaxTokenKind.Plus: boundOp = BoundBinaryOperator.Addition; break;
                case SyntaxTokenKind.Minus: boundOp = BoundBinaryOperator.Subtraction; break;
                case SyntaxTokenKind.Star: boundOp = BoundBinaryOperator.Multiplication; break;
                case SyntaxTokenKind.Slash: boundOp = BoundBinaryOperator.Division; break;
                default: return null;
            }

            if (!leftType.MatchBinaryOperator(rightType, boundOp)) return null;

            return boundOp;
        }

        private BoundUnaryOperator? BindUnaryOperator(SyntaxToken op, TypeSymbol type)
        {
            BoundUnaryOperator boundOp;

            switch (op.kind)
            {
                case SyntaxTokenKind.Plus: boundOp = BoundUnaryOperator.Identety; break;
                case SyntaxTokenKind.Minus: boundOp = BoundUnaryOperator.Negation; break;
                default: return null;
            }

            if (!type.MatchUnaryOperator(boundOp)) return null;

            return boundOp;
        }
    }
}