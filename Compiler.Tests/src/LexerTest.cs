using System;
using Xunit;
using Compiler.Syntax;
using Compiler.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Compiler.Text;

namespace Compiler.Syntax
{
    public class LexerTest
    {
        [Theory]
        [MemberData(nameof(GetTokenData))]
        public void LexSingleToken(string text, SyntaxTokenKind kind)
        {
            var tokens = SyntaxTree.ParseTokens(new SourceText(text));
            Assert.Equal(2, tokens.Length);
            Assert.Equal(kind, tokens[0].Kind);
            Assert.Equal(SyntaxTokenKind.End, tokens[1].Kind);
        }

        [Theory]
        [MemberData(nameof(GetTokenPairData))]
        public void LexTokenPairs(string text1, SyntaxTokenKind kind1, string text2, SyntaxTokenKind kind2)
        {
            var text = text1 + text2;
            var tokens = SyntaxTree.ParseTokens(new SourceText(text));
            Assert.Equal(3, tokens.Length);
            Assert.Equal(kind1, tokens[0].Kind);
            Assert.Equal(kind2, tokens[1].Kind);
            Assert.Equal(SyntaxTokenKind.End, tokens[2].Kind);
        }

        [Theory]
        [MemberData(nameof(GetTokenPairsWithSpaceData))]
        public void LexTokenPairsWithSpace(string space, string text1, SyntaxTokenKind kind1, string text2, SyntaxTokenKind kind2)
        {
            var text = text1 + space + text2;
            var tokens = SyntaxTree.ParseTokens(new SourceText(text));

            Assert.Equal(3, tokens.Length);
            Assert.Equal(kind1, tokens[0].Kind);
            Assert.Equal(kind2, tokens[1].Kind);
            Assert.Equal(SyntaxTokenKind.End, tokens[2].Kind);
        }

        public static IEnumerable<object[]> GetTokenData()
        {
            foreach (var t in GetTokens())
                yield return new object[] { t.Item1, t.Item2 };
        }

        public static IEnumerable<object[]> GetTokenPairData()
        {
            foreach (var t in GetTokenPairs())
                if (!RequiresSeperator(t.Item2, t.Item4))
                    yield return new object[] { t.Item1, t.Item2, t.Item3, t.Item4 };
        }

        public static IEnumerable<object[]> GetTokenPairsWithSpaceData()
        {
            foreach (var t in GetTokenPairsWithSpace())
                yield return new object[] { t.Item1, t.Item2, t.Item3, t.Item4, t.Item5 };
        }

        private static IEnumerable<(string, SyntaxTokenKind)> GetTokens()
        {
            return new[]{
                ("+", SyntaxTokenKind.Plus),
                ("-", SyntaxTokenKind.Minus),
                ("*", SyntaxTokenKind.Star),
                ("/", SyntaxTokenKind.Slash),
                ("**", SyntaxTokenKind.StarStar),
                ("//", SyntaxTokenKind.SlashSlah),
                ("=", SyntaxTokenKind.Equal),
                ("==", SyntaxTokenKind.EqualEqual),
                ("!=", SyntaxTokenKind.NotEqual),
                ("<", SyntaxTokenKind.LessThan),
                (">", SyntaxTokenKind.GreaterThan),
                ("<=", SyntaxTokenKind.LessEqual),
                (">=", SyntaxTokenKind.GreaterEqual),
                ("&&", SyntaxTokenKind.AmpersandAmpersand),
                ("||", SyntaxTokenKind.PipePipe),
                ("!", SyntaxTokenKind.Bang),
                ("(", SyntaxTokenKind.LParen),
                (")", SyntaxTokenKind.RParen),
                ("true", SyntaxTokenKind.True),
                ("false", SyntaxTokenKind.False),
                ("null", SyntaxTokenKind.Null),

                ("5", SyntaxTokenKind.Int),
                ("123456", SyntaxTokenKind.Int),
                ("5.9", SyntaxTokenKind.Float),
                ("100.3", SyntaxTokenKind.Float),
                ("102.12334", SyntaxTokenKind.Float),
                ("0.0", SyntaxTokenKind.Float),

                ("abc", SyntaxTokenKind.Identifier),
                ("_", SyntaxTokenKind.Identifier),
                ("A1234567890_", SyntaxTokenKind.Identifier),
                ("_0", SyntaxTokenKind.Identifier),
            };
        }

        private static IEnumerable<string> GetSpaces()
        {
            return new[]
            {
                "  ",
                "\t \t",
                "\r\n",
                "\n ",
            };
        }

        private static IEnumerable<(string, SyntaxTokenKind, string, SyntaxTokenKind)> GetTokenPairs()
        {
            foreach (var t1 in GetTokens())
                foreach (var t2 in GetTokens())
                    yield return (t1.Item1, t1.Item2, t2.Item1, t2.Item2);
        }

        private static IEnumerable<(string, string, SyntaxTokenKind, string, SyntaxTokenKind)> GetTokenPairsWithSpace()
        {
            foreach (var t1 in GetTokens())
                foreach (var t2 in GetTokens())
                    foreach (var s in GetSpaces())
                        yield return (s, t1.Item1, t1.Item2, t2.Item1, t2.Item2);
        }

        private static bool RequiresSeperator(SyntaxTokenKind kind1, SyntaxTokenKind kind2)
        {
            if (kind1 == SyntaxTokenKind.Identifier && kind2 == SyntaxTokenKind.Identifier) return true;
            if (kind1 == SyntaxTokenKind.Identifier && kind2 == SyntaxTokenKind.True) return true;
            if (kind1 == SyntaxTokenKind.Identifier && kind2 == SyntaxTokenKind.False) return true;
            if (kind1 == SyntaxTokenKind.Identifier && kind2 == SyntaxTokenKind.Null) return true;
            if (kind1 == SyntaxTokenKind.Identifier && kind2 == SyntaxTokenKind.Int) return true;
            if (kind1 == SyntaxTokenKind.Identifier && kind2 == SyntaxTokenKind.Float) return true;

            if (kind1 == SyntaxTokenKind.True && kind2 == SyntaxTokenKind.Identifier) return true;
            if (kind1 == SyntaxTokenKind.True && kind2 == SyntaxTokenKind.True) return true;
            if (kind1 == SyntaxTokenKind.True && kind2 == SyntaxTokenKind.False) return true;
            if (kind1 == SyntaxTokenKind.True && kind2 == SyntaxTokenKind.Null) return true;
            if (kind1 == SyntaxTokenKind.True && kind2 == SyntaxTokenKind.Int) return true;
            if (kind1 == SyntaxTokenKind.True && kind2 == SyntaxTokenKind.Float) return true;

            if (kind1 == SyntaxTokenKind.False && kind2 == SyntaxTokenKind.Identifier) return true;
            if (kind1 == SyntaxTokenKind.False && kind2 == SyntaxTokenKind.True) return true;
            if (kind1 == SyntaxTokenKind.False && kind2 == SyntaxTokenKind.False) return true;
            if (kind1 == SyntaxTokenKind.False && kind2 == SyntaxTokenKind.Null) return true;
            if (kind1 == SyntaxTokenKind.False && kind2 == SyntaxTokenKind.Int) return true;
            if (kind1 == SyntaxTokenKind.False && kind2 == SyntaxTokenKind.Float) return true;

            if (kind1 == SyntaxTokenKind.Null && kind2 == SyntaxTokenKind.Identifier) return true;
            if (kind1 == SyntaxTokenKind.Null && kind2 == SyntaxTokenKind.True) return true;
            if (kind1 == SyntaxTokenKind.Null && kind2 == SyntaxTokenKind.False) return true;
            if (kind1 == SyntaxTokenKind.Null && kind2 == SyntaxTokenKind.Null) return true;
            if (kind1 == SyntaxTokenKind.Null && kind2 == SyntaxTokenKind.Int) return true;
            if (kind1 == SyntaxTokenKind.Null && kind2 == SyntaxTokenKind.Float) return true;

            if (kind1 == SyntaxTokenKind.Int && kind2 == SyntaxTokenKind.Identifier) return true;
            if (kind1 == SyntaxTokenKind.Int && kind2 == SyntaxTokenKind.True) return true;
            if (kind1 == SyntaxTokenKind.Int && kind2 == SyntaxTokenKind.False) return true;
            if (kind1 == SyntaxTokenKind.Int && kind2 == SyntaxTokenKind.Null) return true;
            if (kind1 == SyntaxTokenKind.Int && kind2 == SyntaxTokenKind.Int) return true;
            if (kind1 == SyntaxTokenKind.Int && kind2 == SyntaxTokenKind.Float) return true;

            if (kind1 == SyntaxTokenKind.Float && kind2 == SyntaxTokenKind.Identifier) return true;
            if (kind1 == SyntaxTokenKind.Float && kind2 == SyntaxTokenKind.True) return true;
            if (kind1 == SyntaxTokenKind.Float && kind2 == SyntaxTokenKind.False) return true;
            if (kind1 == SyntaxTokenKind.Float && kind2 == SyntaxTokenKind.Null) return true;
            if (kind1 == SyntaxTokenKind.Float && kind2 == SyntaxTokenKind.Int) return true;
            if (kind1 == SyntaxTokenKind.Float && kind2 == SyntaxTokenKind.Float) return true;

            if (kind1 == SyntaxTokenKind.Star && kind2 == SyntaxTokenKind.Star) return true;
            if (kind1 == SyntaxTokenKind.Slash && kind2 == SyntaxTokenKind.Slash) return true;
            if (kind1 == SyntaxTokenKind.Star && kind2 == SyntaxTokenKind.StarStar) return true;
            if (kind1 == SyntaxTokenKind.Slash && kind2 == SyntaxTokenKind.SlashSlah) return true;

            if (kind1 == SyntaxTokenKind.Equal && kind2 == SyntaxTokenKind.Equal) return true;
            if (kind1 == SyntaxTokenKind.Equal && kind2 == SyntaxTokenKind.EqualEqual) return true;

            if (kind1 == SyntaxTokenKind.LessThan && kind2 == SyntaxTokenKind.Equal) return true;
            if (kind1 == SyntaxTokenKind.GreaterThan && kind2 == SyntaxTokenKind.Equal) return true;
            if (kind1 == SyntaxTokenKind.LessThan && kind2 == SyntaxTokenKind.EqualEqual) return true;
            if (kind1 == SyntaxTokenKind.GreaterThan && kind2 == SyntaxTokenKind.EqualEqual) return true;

            if (kind1 == SyntaxTokenKind.Bang && kind2 == SyntaxTokenKind.Equal) return true;
            if (kind1 == SyntaxTokenKind.Bang && kind2 == SyntaxTokenKind.EqualEqual) return true;

            return false;
        }

    }
}
