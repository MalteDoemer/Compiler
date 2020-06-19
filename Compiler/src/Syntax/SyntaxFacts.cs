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

    }
}