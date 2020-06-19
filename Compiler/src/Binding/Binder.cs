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
        public abstract TypeSymbol ResultType { get; }
    }

    internal sealed class BoundInvalidExpression : BoundExpression
    {
        public override TypeSymbol ResultType => TypeSymbol.NullType;

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

        public override TypeSymbol ResultType => symbol;
        public override int Pos => pos;
        public dynamic Value { get; }
    }

    internal enum BoundUnaryOperator
    {
        Identety,
        Negation,
        LogicalNot,
    }

    internal enum BoundBinaryOperator
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
        Power,
        Root,

        EqualEqual,
        NotEqual,
        LessThan,
        GreaterThan,
        LessEqual,
        GreaterEqual,

        LogicalAnd,
        LogicalOr,

    }

    internal sealed class BoundUnaryExpression : BoundExpression
    {
        private readonly int pos;
        private readonly TypeSymbol resultType;

        public BoundUnaryExpression(int pos, BoundUnaryOperator op, BoundExpression right, TypeSymbol resultType)
        {
            this.resultType = resultType;
            this.pos = pos;
            Op = op;
            Right = right;
        }

        public override TypeSymbol ResultType => resultType;

        public BoundUnaryOperator Op { get; }
        public BoundExpression Right { get; }

        public override int Pos => pos;
    }

    internal sealed class BoundBinaryExpression : BoundExpression
    {
        private readonly int pos;
        private readonly TypeSymbol resultType;

        public BoundBinaryExpression(int pos, BoundBinaryOperator op, BoundExpression left, BoundExpression right, TypeSymbol resultType)
        {
            this.pos = pos;
            Op = op;
            Left = left;
            Right = right;
            this.resultType = resultType;
        }

        public override TypeSymbol ResultType => resultType;

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
                Diagnostics.ReportUnsupportedBinaryOperator(be.Op, left, right);
                return new BoundInvalidExpression();
            }
            

            return new BoundBinaryExpression(be.Pos, (BoundBinaryOperator)boundOperator, left, right, (TypeSymbol)resultType);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax ue)
        {
            var right = BindExpression(ue.Expression);
            var boundOperator = BindUnaryOperator(ue.Op, right.ResultType);

            var resultType = BindFacts.ResolveUnaryType(boundOperator, right.ResultType);

            if (boundOperator == null || resultType == null)
            {
                Diagnostics.ReportUnsupportedUnaryOperator(ue.Op, right);
                return new BoundInvalidExpression();
            }

            return new BoundUnaryExpression(ue.Pos, (BoundUnaryOperator)boundOperator, right, (TypeSymbol)resultType);
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

            //if (!leftType.MatchBinaryOperator(rightType, boundOp)) return null;

            return boundOp;
        }

        private BoundUnaryOperator? BindUnaryOperator(SyntaxToken op, TypeSymbol type)
        {
            BoundUnaryOperator boundOp;

            switch (op.kind)
            {
                case SyntaxTokenKind.Plus: boundOp = BoundUnaryOperator.Identety; break;
                case SyntaxTokenKind.Minus: boundOp = BoundUnaryOperator.Negation; break;
                case SyntaxTokenKind.Bang: boundOp = BoundUnaryOperator.LogicalNot; break;
                default: return null;
            }

            //if (!type.MatchUnaryOperator(boundOp)) return null;

            return boundOp;
        }
    }
}