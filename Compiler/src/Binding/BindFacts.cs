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
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Power), TypeSymbol.Float}, // Special here 
            {(TypeSymbol.Int, TypeSymbol.Int, BoundBinaryOperator.Root), TypeSymbol.Float}, //  Math.Pow always returns a float64
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

            {(TypeSymbol.Any, TypeSymbol.Any, BoundBinaryOperator.NotEqual), TypeSymbol.Bool},
            {(TypeSymbol.Any, TypeSymbol.Any, BoundBinaryOperator.EqualEqual), TypeSymbol.Bool},

            {(TypeSymbol.Bool, TypeSymbol.Bool, BoundBinaryOperator.EqualEqual), TypeSymbol.Bool},
            {(TypeSymbol.Bool, TypeSymbol.Bool, BoundBinaryOperator.NotEqual), TypeSymbol.Bool},

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

            foreach (var pair in UnaryResultTypes)
                if (pair.Key.Item2 == op && pair.Key.Item1 == type) return pair.Value;
            return null;
        }

        public static TypeSymbol ResolveBinaryType(BoundBinaryOperator? op, TypeSymbol left, TypeSymbol right)
        {
            if (op == null) return null;

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
                case SyntaxTokenKind.ObjKeyword: return TypeSymbol.Any;
                default: return TypeSymbol.ErrorType;
            }
        }

        internal static ConversionType ClassifyConversion(TypeSymbol from, TypeSymbol to)
        {
            if (from == null || to == null) return ConversionType.None;

            if (from == to) return ConversionType.Identety;

            if (to == TypeSymbol.Any || from == TypeSymbol.Any) return ConversionType.Implicit;

            switch (from.Name, to.Name)
            {
                case ("int", "float"): return ConversionType.Implicit;
                case ("float", "int"): return ConversionType.Explicit;
                case ("float", "str"): return ConversionType.Explicit;
                case ("int", "str"): return ConversionType.Explicit;
                case ("bool", "str"): return ConversionType.Explicit;
                default: return ConversionType.None;
            }
        }

        public static string GetText(this BoundUnaryOperator op)
        {
            switch (op)
            {
                case BoundUnaryOperator.Identety:
                    return "+";
                case BoundUnaryOperator.Negation:
                    return "-";
                case BoundUnaryOperator.LogicalNot:
                    return "!";
                case BoundUnaryOperator.BitwiseNot:
                    return "~";
                default: return string.Empty;
            }
        }

        public static string GetText(this BoundBinaryOperator op)
        {
            switch (op)
            {
                case BoundBinaryOperator.Addition:
                    return "+";
                case BoundBinaryOperator.Subtraction:
                    return "-";
                case BoundBinaryOperator.Multiplication:
                    return "*";
                case BoundBinaryOperator.Division:
                    return "/";
                case BoundBinaryOperator.Power:
                    return "**";
                case BoundBinaryOperator.Root:
                    return "//";
                case BoundBinaryOperator.Modulo:
                    return "%";
                case BoundBinaryOperator.EqualEqual:
                    return "==";
                case BoundBinaryOperator.NotEqual:
                    return "!=";
                case BoundBinaryOperator.LessThan:
                    return "<";
                case BoundBinaryOperator.GreaterThan:
                    return ">";
                case BoundBinaryOperator.LessEqual:
                    return "<=";
                case BoundBinaryOperator.GreaterEqual:
                    return ">=";
                case BoundBinaryOperator.LogicalAnd:
                    return "&&";
                case BoundBinaryOperator.LogicalOr:
                    return "||";
                case BoundBinaryOperator.BitwiseAnd:
                    return "&";
                case BoundBinaryOperator.BitwiseOr:
                    return "|";
                case BoundBinaryOperator.BitwiseXor:
                    return "^";
                default: return string.Empty;
            }
        }

    }
}