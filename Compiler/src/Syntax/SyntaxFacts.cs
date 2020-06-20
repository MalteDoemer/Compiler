using System.Collections.Generic;

namespace Compiler.Syntax
{
    internal static class SyntaxFacts
    {
        public static readonly Dictionary<string, SyntaxTokenKind> SingleCharacters = new Dictionary<string, SyntaxTokenKind>()
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
        };

        public static readonly Dictionary<string, SyntaxTokenKind> DoubleCharacters = new Dictionary<string, SyntaxTokenKind>()
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

        public static readonly Dictionary<string, SyntaxTokenKind> Keywords = new Dictionary<string, SyntaxTokenKind>()
        {
            {"true", SyntaxTokenKind.True},
            {"false", SyntaxTokenKind.False},
            {"null", SyntaxTokenKind.Null},
        };

        public const int MaxPrecedence = 5;

        public static dynamic GetKeywordValue(string keyword)
        {
            switch (keyword)
            {
                case "true": return true;
                case "false": return false;
                case "null": return null;
                default: return keyword;
            }
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

        public static bool IsLiteralExpression(this SyntaxTokenKind kind)
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

        public static SyntaxTokenKind? IsKeyWord(string text)
        {
            foreach (var pair in Keywords)
                if (pair.Key == text) return pair.Value;
            return null;
        }

    }
}