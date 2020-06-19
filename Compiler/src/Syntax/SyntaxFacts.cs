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
            {"(", SyntaxTokenKind.LParen},
            {")", SyntaxTokenKind.RParen},
        };

        public static readonly Dictionary<string, SyntaxTokenKind> DoubleCharacters = new Dictionary<string, SyntaxTokenKind>()
        {
            {"**", SyntaxTokenKind.StarStar},
            {"//", SyntaxTokenKind.SlashSlah},
        };

        public static readonly Dictionary<string, SyntaxTokenKind> Keywords = new Dictionary<string, SyntaxTokenKind>()
        {
            {"true", SyntaxTokenKind.True},
            {"false", SyntaxTokenKind.False},
            {"null", SyntaxTokenKind.Null},
        };

        public const int MaxPrecedence = 2;

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
            switch(kind)
            {
                case SyntaxTokenKind.Minus:
                case SyntaxTokenKind.Plus:
                    return true;
                default: return false;
            }
        }

        public static bool IsLiteralExpression(this SyntaxTokenKind kind)
        {
            switch(kind)
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
            switch(kind)
            {
                case SyntaxTokenKind.Star:
                case SyntaxTokenKind.Slash:
                    return 1;
                case SyntaxTokenKind.Plus:
                case SyntaxTokenKind.Minus:
                    return 2;

                default: return 0;
            }
        }

    }
}