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

        public static TypeSymbol? ResolveUnaryType(BoundUnaryOperator? op, TypeSymbol type)
        {
            if (op == null) return null;
            if (type == TypeSymbol.Object) return TypeSymbol.Object;
            foreach (var pair in UnaryResultTypes)
                if (pair.Key.Item2 == op && pair.Key.Item1 == type) return pair.Value;
            return null;
        }

        public static TypeSymbol? ResolveBinaryType(BoundBinaryOperator? op, TypeSymbol left, TypeSymbol right)
        {
            if (op == null) return null;
            if (left == TypeSymbol.Object || right == TypeSymbol.Object) return TypeSymbol.Object;
            foreach (var pair in BinaryResultTypes)
                if (((pair.Key.Item1 == left && pair.Key.Item2 == right) || (pair.Key.Item2 == left && pair.Key.Item1 == right)) && pair.Key.Item3 == op) return pair.Value;
            return null;
        }

        public static BoundBinaryOperator? BindBinaryOperator(SyntaxTokenKind op)
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

        public static BoundUnaryOperator? BindUnaryOperator(SyntaxTokenKind op)
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
                case SyntaxTokenKind.ObjKeyword: return TypeSymbol.Object;
                default: throw new Exception($"<{kind}> canno't be converted to a TypeSymbol");
            }
        }

        private static Type GetDotnetType(TypeSymbol typeSymbol)
        {
            switch (typeSymbol)
            {
                case TypeSymbol.Int: return typeof(long);
                case TypeSymbol.Float: return typeof(double);
                case TypeSymbol.Bool: return typeof(bool);
                case TypeSymbol.String: return typeof(string);
                default: return typeof(void);
            }
        }

        private static TypeSymbol? GetTypeSymbolFromDotnetType(Type t)
        {
            if (t == typeof(long))
                return TypeSymbol.Int;
            else if (t == typeof(double))
                return TypeSymbol.Float;
            else if (t == typeof(bool))
                return TypeSymbol.Bool;
            else if (t == typeof(string))
                return TypeSymbol.String;
            else return null;
        }

        private static string GetBinaryMethodName(BoundBinaryOperator op)
        {
            switch (op)
            {
                case BoundBinaryOperator.Addition: return "op_Addition";
                case BoundBinaryOperator.Subtraction: return "op_Subtraction";
                case BoundBinaryOperator.Multiplication: return "op_Multiply";
                case BoundBinaryOperator.Division: return "op_Division";
                //case BoundBinaryOperator.Power: return "op_";
                //case BoundBinaryOperator.Root: return "op_";
                case BoundBinaryOperator.Modulo: return "op_Modulus";
                case BoundBinaryOperator.EqualEqual: return "op_Equality";
                case BoundBinaryOperator.NotEqual: return "op_Inequality";
                case BoundBinaryOperator.LessThan: return "op_LessThan";
                case BoundBinaryOperator.GreaterThan: return "op_GreaterThan";
                case BoundBinaryOperator.LessEqual: return "op_LessThanOrEqual";
                case BoundBinaryOperator.GreaterEqual: return "op_GreaterThanOrEqual";
                case BoundBinaryOperator.LogicalAnd: return "op_LogicalAnd";
                case BoundBinaryOperator.LogicalOr: return "op_LogicalOr";
                case BoundBinaryOperator.BitwiseAnd: return "op_BitwiseAnd";
                case BoundBinaryOperator.BitwiseOr: return "op_BitwiseOr";
                case BoundBinaryOperator.BitwiseXor: return "op_ExclusiveOr";
                default: return "";
            }
        }
    }
}
