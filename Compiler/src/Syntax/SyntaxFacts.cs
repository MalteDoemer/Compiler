using System;
using System.Collections.Generic;

namespace Compiler.Syntax
{
    public static class SyntaxFacts
    {
        internal static readonly Dictionary<string, SyntaxTokenKind> SingleCharacters = new Dictionary<string, SyntaxTokenKind>()
        {
            {"+", SyntaxTokenKind.Plus},
            {"-", SyntaxTokenKind.Minus},
            {"*", SyntaxTokenKind.Star},
            {"/", SyntaxTokenKind.Slash},
            {"<", SyntaxTokenKind.LessThan},
            {">", SyntaxTokenKind.GreaterThan},
            {"!", SyntaxTokenKind.Bang},
            {"(", SyntaxTokenKind.LParen},
            {")", SyntaxTokenKind.RParen},
            {"=", SyntaxTokenKind.Equal},
        };

        internal static readonly Dictionary<string, SyntaxTokenKind> DoubleCharacters = new Dictionary<string, SyntaxTokenKind>()
        {
            {"**", SyntaxTokenKind.StarStar},
            {"//", SyntaxTokenKind.SlashSlah},
            {"==", SyntaxTokenKind.EqualEqual},
            {"!=", SyntaxTokenKind.NotEqual},
            {"<=", SyntaxTokenKind.LessEqual},
            {">=", SyntaxTokenKind.GreaterEqual},
            {"&&", SyntaxTokenKind.AmpersandAmpersand},
            {"||", SyntaxTokenKind.PipePipe},
        };

        internal static readonly Dictionary<string, SyntaxTokenKind> Keywords = new Dictionary<string, SyntaxTokenKind>()
        {
            {"true", SyntaxTokenKind.True},
            {"false", SyntaxTokenKind.False},
            {"null", SyntaxTokenKind.Null},
        };

        internal const int MaxPrecedence = 5;

        internal static dynamic GetKeywordValue(string keyword)
        {
            switch (keyword)
            {
                case "true": return true;
                case "false": return false;
                case "null": return null;
                default: return keyword;
            }
        }

        internal static bool IsUnaryOperator(this SyntaxTokenKind kind)
        {
            switch (kind)
            {
                case SyntaxTokenKind.Minus:
                case SyntaxTokenKind.Plus:
                case SyntaxTokenKind.Bang:
                    return true;
                default: return false;
            }
        }

        internal static bool IsLiteralExpression(this SyntaxTokenKind kind)
        {
            switch (kind)
            {
                case SyntaxTokenKind.Float:
                case SyntaxTokenKind.Int:
                case SyntaxTokenKind.String:
                case SyntaxTokenKind.True:
                case SyntaxTokenKind.False:
                case SyntaxTokenKind.Null:
                    return true;
                default: return false;
            }
        }
        
        internal static SyntaxTokenKind? IsKeyWord(string text)
        {
            foreach (var pair in Keywords)
                if (pair.Key == text) return pair.Value;
            return null;
        }

        public static int GetBinaryPrecedence(this SyntaxTokenKind kind)
        {
            switch (kind)
            {
                case SyntaxTokenKind.StarStar:
                case SyntaxTokenKind.SlashSlah:
                    return 1;

                case SyntaxTokenKind.Star:
                case SyntaxTokenKind.Slash:
                    return 2;

                case SyntaxTokenKind.Plus:
                case SyntaxTokenKind.Minus:
                    return 3;

                case SyntaxTokenKind.LessEqual:
                case SyntaxTokenKind.GreaterEqual:
                case SyntaxTokenKind.LessThan:
                case SyntaxTokenKind.GreaterThan:
                case SyntaxTokenKind.EqualEqual:
                case SyntaxTokenKind.NotEqual:
                    return 4;

                case SyntaxTokenKind.AmpersandAmpersand:
                case SyntaxTokenKind.PipePipe:
                    return 5;

                default: return 0;
            }
        }

        public static IEnumerable<SyntaxTokenKind> GetUnaryOperators()
        {
            var tokens = (SyntaxTokenKind[])Enum.GetValues(typeof(SyntaxTokenKind));

            foreach (var t in tokens)
                if (IsUnaryOperator(t)) yield return t;
        }

        public static IEnumerable<SyntaxTokenKind> GetBinaryOperators()
        {
            var tokens = (SyntaxTokenKind[])Enum.GetValues(typeof(SyntaxTokenKind));

            foreach (var t in tokens)
                if (GetBinaryPrecedence(t) > 0) yield return t;
        }

        public static string GetOperatorText(SyntaxTokenKind kind)
        {
            switch (kind)
            {
                case SyntaxTokenKind.Plus: return "+";
                case SyntaxTokenKind.Minus: return "-";
                case SyntaxTokenKind.Star: return "*";
                case SyntaxTokenKind.Slash: return "/";
                case SyntaxTokenKind.StarStar: return "**";
                case SyntaxTokenKind.SlashSlah: return "//";
                case SyntaxTokenKind.LessThan: return "<";
                case SyntaxTokenKind.GreaterThan: return ">";
                case SyntaxTokenKind.LessEqual: return "<=";
                case SyntaxTokenKind.GreaterEqual: return ">=";
                case SyntaxTokenKind.EqualEqual: return "==";
                case SyntaxTokenKind.NotEqual: return "!=";
                case SyntaxTokenKind.Bang: return "!";
                case SyntaxTokenKind.PipePipe: return "||";
                case SyntaxTokenKind.AmpersandAmpersand: return "&&";
                default: return null;
            }
        }
    }
}