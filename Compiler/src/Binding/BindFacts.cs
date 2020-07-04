using System;
using System.Collections.Generic;
using Compiler.Syntax;
using System.Reflection;
using Compiler.Symbols;

namespace Compiler.Binding
{
    public static class BindFacts
    {
        private static readonly Dictionary<(TypeSymbol, BoundUnaryOperator), TypeSymbol> UnaryResultTypes = new Dictionary<(TypeSymbol, BoundUnaryOperator), TypeSymbol>()
        {
            {(TypeSymbol.Int, BoundUnaryOperator.Identety), TypeSymbol.Int},
            {(TypeSymbol.Float, BoundUnaryOperator.Identety), TypeSymbol.Float},
            {(TypeSymbol.Int, BoundUnaryOperator.Negation), TypeSymbol.Int},
            {(TypeSymbol.Float, BoundUnaryOperator.Negation), TypeSymbol.Float},
            {(TypeSymbol.Bool, BoundUnaryOperator.LogicalNot), TypeSymbol.Bool},
            {(TypeSymbol.Int, BoundUnaryOperator.BitwiseNot), TypeSymbol.Int},
        };

        private static readonly Dictionary<(TypeSymbol, TypeSymbol, BoundBinaryOperator), TypeSymbol> BinaryResultTypes = new Dictionary<(TypeSymbol, TypeSymbol, BoundBinaryOperator), TypeSymbol>()
        {
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Addition), TypeSymbol.Int},
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Subtraction), TypeSymbol.Int},
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Multiplication), TypeSymbol.Int},
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Division), TypeSymbol.Int},
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Power), TypeSymbol.Int},
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Root), TypeSymbol.Int},
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Modulo), TypeSymbol.Int},
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.BitwiseAnd), TypeSymbol.Int},
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.BitwiseOr), TypeSymbol.Int},
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.BitwiseXor), TypeSymbol.Int},

            {(TypeSymbol.Bool, TypeSymbol.Bool, BoundBinaryOperator.BitwiseAnd), TypeSymbol.Bool},
            {(TypeSymbol.Bool, TypeSymbol.Bool, BoundBinaryOperator.BitwiseOr), TypeSymbol.Bool},
            {(TypeSymbol.Bool, TypeSymbol.Bool, BoundBinaryOperator.BitwiseXor), TypeSymbol.Bool},

            {(TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Addition), TypeSymbol.Float},
            {(TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Subtraction), TypeSymbol.Float},
            {(TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Multiplication), TypeSymbol.Float},
            {(TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Division), TypeSymbol.Float},
            {(TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Power), TypeSymbol.Float},
            {(TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Root), TypeSymbol.Float},
            {(TypeSymbol.Int, TypeSymbol.Float, BoundBinaryOperator.Modulo), TypeSymbol.Float},

            {(TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Addition), TypeSymbol.Float},
            {(TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Subtraction), TypeSymbol.Float},
            {(TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Multiplication), TypeSymbol.Float},
            {(TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Division), TypeSymbol.Float},
            {(TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Power), TypeSymbol.Float},
            {(TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Root), TypeSymbol.Float},
            {(TypeSymbol.Float, TypeSymbol.Float, BoundBinaryOperator.Modulo), TypeSymbol.Float},

            {(TypeSymbol.String, TypeSymbol.String, BoundBinaryOperator.Addition), TypeSymbol.String},
            {(TypeSymbol.String, TypeSymbol.Int, BoundBinaryOperator.Addition), TypeSymbol.String},
            {(TypeSymbol.String, TypeSymbol.Float, BoundBinaryOperator.Addition), TypeSymbol.String},
            {(TypeSymbol.String, TypeSymbol.Bool, BoundBinaryOperator.Addition), TypeSymbol.String},

            {(TypeSymbol.String, TypeSymbol.String, BoundBinaryOperator.NotEqual), TypeSymbol.Bool},
            {(TypeSymbol.String, TypeSymbol.String, BoundBinaryOperator.EqualEqual), TypeSymbol.Bool},

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

        public static TypeSymbol ResolveUnaryType(BoundUnaryOperator? op, TypeSymbol type)
        {
            if (op == null) return null;
            if (type == TypeSymbol.Any) return TypeSymbol.Any;

            foreach (var pair in UnaryResultTypes)
                if (pair.Key.Item2 == op && pair.Key.Item1 == type) return pair.Value;
            return null;
        }

        public static TypeSymbol ResolveBinaryType(BoundBinaryOperator? op, TypeSymbol left, TypeSymbol right)
        {
            if (op == null) return null;
            if (left == TypeSymbol.Any || right == TypeSymbol.Any) return TypeSymbol.Any;

            foreach (var pair in BinaryResultTypes)
                if (((pair.Key.Item1 == left && pair.Key.Item2 == right) || (pair.Key.Item2 == left && pair.Key.Item1 == right)) && pair.Key.Item3 == op) return pair.Value;
            return null;
        }

        public static TypeSymbol GetTypeSymbol(SyntaxTokenKind kind)
        {
            switch (kind)
            {
                case SyntaxTokenKind.Int: return TypeSymbol.Int;
                case SyntaxTokenKind.Float: return TypeSymbol.Float;
                case SyntaxTokenKind.String: return TypeSymbol.String;
                case SyntaxTokenKind.False: return TypeSymbol.Bool;
                case SyntaxTokenKind.True: return TypeSymbol.Bool;
                case SyntaxTokenKind.IntKeyword: return TypeSymbol.Int;
                case SyntaxTokenKind.FloatKeyword: return TypeSymbol.Float;
                case SyntaxTokenKind.StringKeyword: return TypeSymbol.String;
                case SyntaxTokenKind.BoolKeyword: return TypeSymbol.Bool;
                case SyntaxTokenKind.VoidKeyword: return TypeSymbol.Void;
                case SyntaxTokenKind.AnyKeyword: return TypeSymbol.Any;
                default: return null;
            }
        }

        internal static ConversionType ClassifyConversion(TypeSymbol from, TypeSymbol to)
        {
            if (from == to) return ConversionType.Identety;

            if (to == TypeSymbol.Any || from == TypeSymbol.Any) return ConversionType.Implicit;

            switch (from.Name, to.Name)
            {
                case ("int", "float"): return ConversionType.Implicit;
                case ("float", "int"): return ConversionType.Explicit;
                case ("float", "string"): return ConversionType.Explicit;
                case ("int", "string"): return ConversionType.Explicit;
                case ("bool", "string"): return ConversionType.Explicit;
                default : return ConversionType.None;
            }
        }
    }
}
