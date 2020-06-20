using System;
using Xunit;
using Compiler.Syntax;
using Compiler.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace Compiler.Tests.Syntax
{
    public class LexerTest
    {
        [Theory]
        [MemberData(nameof(GetTokenData))]
        public void LexSingleToken(string text, SyntaxTokenKind kind)
        {
            var tokens = Evaluator.Tokenize(text, out DiagnosticBag bag).ToArray();
            Assert.Equal(2, tokens.Length);
            Assert.Equal(kind, tokens[0].kind);
            Assert.Equal(SyntaxTokenKind.End, tokens[1].kind);
        }

        [Theory]
        [MemberData(nameof(GetTokenPairData))]
        public void LexTokenPairs(string text1, SyntaxTokenKind kind1, string text2, SyntaxTokenKind kind2)
        {
            var text = text1 + text2;
            var tokens = Evaluator.Tokenize(text, out DiagnosticBag bag).ToArray();

            Assert.Equal(3, tokens.Length);
            Assert.Equal(kind1, tokens[0].kind);
            Assert.Equal(kind2, tokens[1].kind);
            Assert.Equal(SyntaxTokenKind.End, tokens[2].kind);
        }

        public static IEnumerable<object[]> GetTokenData()
        {
            foreach (var t in GetTokens())
                yield return new object[] {t.Item1, t.Item2};
        }

        public static IEnumerable<object[]> GetTokenPairData()
        {
            foreach (var t in GetTokenPairs())
                yield return new object[] {t.Item1, t.Item2, t.Item3, t.Item4};
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
            };
        }
    
        private static IEnumerable<(string, SyntaxTokenKind, string, SyntaxTokenKind)> GetTokenPairs()
        {
            foreach (var t1 in GetTokens())
                foreach (var t2 in GetTokens())
                    yield return (t1.Item1, t1.Item2, t2.Item1, t2.Item2);
        }   
   
    }
}
