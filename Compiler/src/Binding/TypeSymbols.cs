using System;
using Compiler.Syntax;

namespace Compiler.Binding
{
    internal static class BindFacts
    {
        private static readonly (TypeSymbol, BoundUnaryOperator)[] UnaryMatcher =
        {
            (TypeSymbol.Int, BoundUnaryOperator.Identety),
            (TypeSymbol.Int, BoundUnaryOperator.Negation),
            (TypeSymbol.Float, BoundUnaryOperator.Identety),
            (TypeSymbol.Float, BoundUnaryOperator.Negation),
        };

        private static readonly (TypeSymbol, TypeSymbol, BoundBinaryOperator)[] BinaryMatcher = 
        {
            (TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Addition),
            (TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Subtraction),
            (TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Multiplication),
            (TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Division),
            
            (TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Addition),
            (TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Subtraction),
            (TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Multiplication),
            (TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Division),
            
            (TypeSymbol.Float, TypeSymbol.Int, BoundBinaryOperator.Addition),
            (TypeSymbol.Float, TypeSymbol.Int, BoundBinaryOperator.Subtraction),
            (TypeSymbol.Float, TypeSymbol.Int, BoundBinaryOperator.Multiplication),
            (TypeSymbol.Float, TypeSymbol.Int, BoundBinaryOperator.Division),

            (TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Addition),
            (TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Subtraction),
            (TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Multiplication),
            (TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Division),
        };


        public static bool MatchUnaryOperator(this TypeSymbol symbol, BoundUnaryOperator op)
        {
            foreach (var t in UnaryMatcher)
                if (symbol == t.Item1 && op == t.Item2) return true;
            return false;
        }
    
        public static bool MatchBinaryOperator(this TypeSymbol symbol, TypeSymbol other, BoundBinaryOperator op)
        {
            foreach (var t in BinaryMatcher)
                if (t.Item1 == symbol && t.Item2 == other && t.Item3 == op) return true;
            return false;
        }
    
        public static TypeSymbol GetTypeSymbol(this SyntaxTokenKind kind)
        {
            switch(kind)
            {
                case SyntaxTokenKind.Int: return TypeSymbol.Int;
                case SyntaxTokenKind.Float: return TypeSymbol.Float;
                case SyntaxTokenKind.String: return TypeSymbol.String;
                case SyntaxTokenKind.Null: return TypeSymbol.Null;
                case SyntaxTokenKind.False: return TypeSymbol.Bool;
                case SyntaxTokenKind.True: return TypeSymbol.Bool;
                default: throw new Exception($"<{kind}> canno't be converted to a TypeSymbol");
            }
        }

    }

    internal enum TypeSymbol
    {
        Int,
        Float,
        Bool,
        String,
        Null,
    }
}