using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Compiler.Syntax
{
    public static class SyntaxFacts
    {

        internal const int MaxPrecedence = 5;

        private static readonly Dictionary<string, SyntaxTokenKind> SingleCharacters = new Dictionary<string, SyntaxTokenKind>()
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
            {"{", SyntaxTokenKind.LCurly},
            {"}", SyntaxTokenKind.RCurly},
            {"=", SyntaxTokenKind.Equal},
        };

        private static readonly Dictionary<string, SyntaxTokenKind> DoubleCharacters = new Dictionary<string, SyntaxTokenKind>()
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

        private static readonly Dictionary<string, SyntaxTokenKind> Keywords = new Dictionary<string, SyntaxTokenKind>()
        {
            {"true", SyntaxTokenKind.True},
            {"false", SyntaxTokenKind.False},
            {"null", SyntaxTokenKind.Null},
            {"int", SyntaxTokenKind.IntKeyword},
            {"float", SyntaxTokenKind.FloatKeyword},
            {"bool", SyntaxTokenKind.BoolKeyword},
            {"string", SyntaxTokenKind.StringKeyword},
            {"var", SyntaxTokenKind.Var},
        };



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

        internal static SyntaxTokenKind? IsSingleCharacter(char c)
        {
            foreach (var pair in SingleCharacters)
                if (pair.Key[0] == c) return pair.Value;
            return null;
        }

        internal static SyntaxTokenKind? IsDoubleCharacter(char c1, char c2)
        {
            foreach (var pair in DoubleCharacters)
                if (c1 == pair.Key[0] && c2 == pair.Key[1]) return pair.Value;
            return null;
        }

        internal static SyntaxTokenKind? IsKeyWord(string text)
        {
            foreach (var pair in Keywords)
                if (pair.Key == text) return pair.Value;
            return null;
        }

        internal static bool IsTypeKeyword(this SyntaxTokenKind kind)
        {
            switch (kind)
            {
                case SyntaxTokenKind.IntKeyword:
                case SyntaxTokenKind.FloatKeyword:
                case SyntaxTokenKind.BoolKeyword:
                case SyntaxTokenKind.StringKeyword:
                case SyntaxTokenKind.Var:
                    return true;
                default: return false;
            }
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

        public static bool IsUnaryOperator(this SyntaxTokenKind kind)
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

        public static bool IsBinaryOperator(this SyntaxTokenKind kind) => kind.GetBinaryPrecedence() > 0;

        public static string GetText(this SyntaxTokenKind kind)
        {
            foreach (var pair in SingleCharacters)
                if (kind == pair.Value) return pair.Key;
            foreach (var pair in DoubleCharacters)
                if (kind == pair.Value) return pair.Key;
            foreach (var pair in Keywords)
                if (kind == pair.Value) return pair.Key;
            return null;
        }
    }
}