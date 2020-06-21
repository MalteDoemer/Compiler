using System;
using System.Collections.Generic;
using Compiler.Syntax;

namespace Compiler.Binding
{
    internal static class BindFacts
    {
        private static readonly Dictionary<(TypeSymbol, BoundUnaryOperator), TypeSymbol> UnaryResultTypes = new Dictionary<(TypeSymbol, BoundUnaryOperator), TypeSymbol>()
        {
            {(TypeSymbol.Int, BoundUnaryOperator.Identety), TypeSymbol.Int},
            {(TypeSymbol.Float, BoundUnaryOperator.Identety), TypeSymbol.Float},
            {(TypeSymbol.Int, BoundUnaryOperator.Negation), TypeSymbol.Int},
            {(TypeSymbol.Float, BoundUnaryOperator.Negation), TypeSymbol.Float},
            {(TypeSymbol.Bool, BoundUnaryOperator.LogicalNot), TypeSymbol.Bool},
        };

        private static readonly Dictionary<(TypeSymbol, TypeSymbol, BoundBinaryOperator), TypeSymbol> BinaryResultTypes = new Dictionary<(TypeSymbol, TypeSymbol, BoundBinaryOperator), TypeSymbol>()
        {
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Addition), TypeSymbol.Int},
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Subtraction), TypeSymbol.Int},
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Multiplication), TypeSymbol.Int},
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Division), TypeSymbol.Int},
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Power), TypeSymbol.Int},
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Root), TypeSymbol.Int},

            {(TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Addition), TypeSymbol.Float},
            {(TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Subtraction), TypeSymbol.Float},
            {(TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Multiplication), TypeSymbol.Float},
            {(TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Division), TypeSymbol.Float},
            {(TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Power), TypeSymbol.Float},
            {(TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Root), TypeSymbol.Float},

            {(TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Addition), TypeSymbol.Float},
            {(TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Subtraction), TypeSymbol.Float},
            {(TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Multiplication), TypeSymbol.Float},
            {(TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Division), TypeSymbol.Float},
            {(TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Power), TypeSymbol.Float},
            {(TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Root), TypeSymbol.Float},

            {(TypeSymbol.String, TypeSymbol.String, BoundBinaryOperator.Addition), TypeSymbol.String},
            {(TypeSymbol.String, TypeSymbol.Int, BoundBinaryOperator.Addition), TypeSymbol.String},
            {(TypeSymbol.String, TypeSymbol.Float, BoundBinaryOperator.Addition), TypeSymbol.String},
            {(TypeSymbol.String, TypeSymbol.Bool, BoundBinaryOperator.Addition), TypeSymbol.String},

            {(TypeSymbol.Bool, TypeSymbol.Bool, BoundBinaryOperator.EqualEqual), TypeSymbol.Bool},
            {(TypeSymbol.Bool, TypeSymbol.Bool, BoundBinaryOperator.NotEqual), TypeSymbol.Bool},
            {(TypeSymbol.Bool, TypeSymbol.Bool, BoundBinaryOperator.LessThan), TypeSymbol.Bool},
            {(TypeSymbol.Bool, TypeSymbol.Bool, BoundBinaryOperator.LessEqual), TypeSymbol.Bool},
            {(TypeSymbol.Bool, TypeSymbol.Bool, BoundBinaryOperator.GreaterThan), TypeSymbol.Bool},
            {(TypeSymbol.Bool, TypeSymbol.Bool, BoundBinaryOperator.GreaterEqual), TypeSymbol.Bool},

            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.EqualEqual), TypeSymbol.Bool},
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.NotEqual), TypeSymbol.Bool},
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.LessThan), TypeSymbol.Bool},
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.LessEqual), TypeSymbol.Bool},
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.GreaterThan), TypeSymbol.Bool},
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.GreaterEqual), TypeSymbol.Bool},

            {(TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.EqualEqual), TypeSymbol.Bool},
            {(TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.NotEqual), TypeSymbol.Bool},
            {(TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.LessThan), TypeSymbol.Bool},
            {(TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.LessEqual), TypeSymbol.Bool},
            {(TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.GreaterThan), TypeSymbol.Bool},
            {(TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.GreaterEqual), TypeSymbol.Bool},

            {(TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.EqualEqual), TypeSymbol.Bool},
            {(TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.NotEqual), TypeSymbol.Bool},
            {(TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.LessThan), TypeSymbol.Bool},
            {(TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.LessEqual), TypeSymbol.Bool},
            {(TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.GreaterThan), TypeSymbol.Bool},
            {(TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.GreaterEqual), TypeSymbol.Bool},

            {(TypeSymbol.Bool, TypeSymbol.Bool, BoundBinaryOperator.LogicalAnd), TypeSymbol.Bool},
            {(TypeSymbol.Bool, TypeSymbol.Bool, BoundBinaryOperator.LogicalOr), TypeSymbol.Bool},

        };

        internal static TypeSymbol? ResolveUnaryType(BoundUnaryOperator? op, TypeSymbol type)
        {
            if (op == null) return null;
            foreach (var pair in UnaryResultTypes)
                if (pair.Key.Item2 == op && pair.Key.Item1 == type) return pair.Value;
            return null;
        }

        internal static TypeSymbol? ResolveBinaryType(BoundBinaryOperator? op, TypeSymbol left, TypeSymbol right)
        {
            if (op == null) return null;
            foreach (var pair in BinaryResultTypes)
                if (((pair.Key.Item1 == left && pair.Key.Item2 == right) || (pair.Key.Item2 == left && pair.Key.Item1 == right)) && pair.Key.Item3 == op) return pair.Value;
            return null;
        }

        internal static BoundBinaryOperator? BindBinaryOperator(SyntaxToken op)
        {
            switch (op.Kind)
            {
                case SyntaxTokenKind.Plus: return BoundBinaryOperator.Addition;
                case SyntaxTokenKind.Minus: return BoundBinaryOperator.Subtraction;
                case SyntaxTokenKind.Star: return BoundBinaryOperator.Multiplication;
                case SyntaxTokenKind.Slash: return BoundBinaryOperator.Division;
                case SyntaxTokenKind.StarStar: return BoundBinaryOperator.Power;
                case SyntaxTokenKind.SlashSlah: return BoundBinaryOperator.Root;
                case SyntaxTokenKind.EqualEqual: return BoundBinaryOperator.EqualEqual;
                case SyntaxTokenKind.NotEqual: return BoundBinaryOperator.NotEqual;
                case SyntaxTokenKind.LessThan: return BoundBinaryOperator.LessThan;
                case SyntaxTokenKind.LessEqual: return BoundBinaryOperator.LessEqual;
                case SyntaxTokenKind.GreaterThan: return BoundBinaryOperator.GreaterThan;
                case SyntaxTokenKind.GreaterEqual: return BoundBinaryOperator.GreaterEqual;
                case SyntaxTokenKind.AmpersandAmpersand: return BoundBinaryOperator.LogicalAnd;
                case SyntaxTokenKind.PipePipe: return BoundBinaryOperator.LogicalOr;
                default: return null;
            }
        }

        internal static BoundUnaryOperator? BindUnaryOperator(SyntaxToken op)
        {
            switch (op.Kind)
            {
                case SyntaxTokenKind.Plus: return BoundUnaryOperator.Identety;
                case SyntaxTokenKind.Minus: return BoundUnaryOperator.Negation;
                case SyntaxTokenKind.Bang: return BoundUnaryOperator.LogicalNot;
                default: return null;
            }
        }

        public static TypeSymbol GetTypeSymbol(SyntaxTokenKind kind)
        {
            switch (kind)
            {
                case SyntaxTokenKind.Int: return TypeSymbol.Int;
                case SyntaxTokenKind.Float: return TypeSymbol.Float;
                case SyntaxTokenKind.String: return TypeSymbol.String;
                case SyntaxTokenKind.Null: return TypeSymbol.NullType;
                case SyntaxTokenKind.False: return TypeSymbol.Bool;
                case SyntaxTokenKind.True: return TypeSymbol.Bool;
                default: throw new Exception($"<{kind}> canno't be converted to a TypeSymbol");
            }
        }

    }
}