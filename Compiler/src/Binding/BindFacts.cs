using System;
using System.Collections.Generic;
using Compiler.Syntax;

namespace Compiler.Binding
{
    internal static class BindFacts
    {
        private static readonly (TypeSymbol, BoundUnaryOperator)[] UnaryMatcher =
        {
            (TypeSymbol.Int, BoundUnaryOperator.Identety),
            (TypeSymbol.Float, BoundUnaryOperator.Identety),
            (TypeSymbol.Int, BoundUnaryOperator.Negation),
            (TypeSymbol.Float, BoundUnaryOperator.Negation),
            (TypeSymbol.Bool, BoundUnaryOperator.LogicalNot),
        };

        private static readonly (TypeSymbol, TypeSymbol, BoundBinaryOperator)[] BinaryMatcher =
        {
            (TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Addition),
            (TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Subtraction),
            (TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Multiplication),
            (TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Division),
            (TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Power),
            (TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Root),

            (TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Addition),
            (TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Subtraction),
            (TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Multiplication),
            (TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Division),
            (TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Power),
            (TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Root),

            (TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Addition),
            (TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Subtraction),
            (TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Multiplication),
            (TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Division),
            (TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Power),
            (TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Root),

            (TypeSymbol.String, TypeSymbol.String, BoundBinaryOperator.Addition),
            (TypeSymbol.String, TypeSymbol.Int, BoundBinaryOperator.Addition),
            (TypeSymbol.String, TypeSymbol.Float, BoundBinaryOperator.Addition),
            (TypeSymbol.String, TypeSymbol.Bool, BoundBinaryOperator.Addition),

            (TypeSymbol.Bool, TypeSymbol.Bool, BoundBinaryOperator.EqualEqual),
            (TypeSymbol.Bool, TypeSymbol.Bool, BoundBinaryOperator.NotEqual),
            (TypeSymbol.Bool, TypeSymbol.Bool, BoundBinaryOperator.LessThan),
            (TypeSymbol.Bool, TypeSymbol.Bool, BoundBinaryOperator.LessEqual),
            (TypeSymbol.Bool, TypeSymbol.Bool, BoundBinaryOperator.GreaterThan),
            (TypeSymbol.Bool, TypeSymbol.Bool, BoundBinaryOperator.GreaterEqual),

            (TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.EqualEqual),
            (TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.NotEqual),
            (TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.LessThan),
            (TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.LessEqual),
            (TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.GreaterThan),
            (TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.GreaterEqual),

            (TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.EqualEqual),
            (TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.NotEqual),
            (TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.LessThan),
            (TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.LessEqual),
            (TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.GreaterThan),
            (TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.GreaterEqual),

            (TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.EqualEqual),
            (TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.NotEqual),
            (TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.LessThan),
            (TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.LessEqual),
            (TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.GreaterThan),
            (TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.GreaterEqual),

            (TypeSymbol.Bool, TypeSymbol.Bool, BoundBinaryOperator.LogicalAnd),
            (TypeSymbol.Bool, TypeSymbol.Bool, BoundBinaryOperator.LogicalOr),

        };

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

        // public static bool MatchUnaryOperator(this TypeSymbol symbol, BoundUnaryOperator op)
        // {
        //     foreach (var t in UnaryMatcher)
        //         if (symbol == t.Item1 && op == t.Item2) return true;
        //     return false;
        // }

        // public static bool MatchBinaryOperator(this TypeSymbol symbol, TypeSymbol other, BoundBinaryOperator op)
        // {
        //     foreach (var t in BinaryMatcher)
        //         if (((t.Item1 == symbol && t.Item2 == other) || (t.Item2 == symbol && t.Item1 == other)) && t.Item3 == op) return true;
        //     return false;
        // }

        public static TypeSymbol GetTypeSymbol(this SyntaxTokenKind kind)
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