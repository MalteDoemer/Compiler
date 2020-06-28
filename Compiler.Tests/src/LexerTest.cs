using System;
using System.Collections.Generic;
using System.Linq;
using Compiler.Syntax;
using Xunit;

namespace Compiler.Test
{
    public class LexerTest
    {

        [Theory]
        [MemberData(nameof(GetAllTokenData))]
        public static void LexAllTokens(string text, SyntaxTokenKind kind)
        {
            var tokens = Compilation.Tokenize(text);
            Assert.Equal(2, tokens.Length);
            Assert.Equal(kind, tokens[0].Kind);
            Assert.Equal(SyntaxTokenKind.End, tokens[1].Kind);
        }

        [Theory]
        [MemberData(nameof(GetAllTokenDataSquared))]
        public static void LexAllTokensWithSpace(string text1, SyntaxTokenKind kind1, string text2, SyntaxTokenKind kind2)
        {
            var text = text1 + " " + text2;
            var tokens = Compilation.Tokenize(text);
            Assert.Equal(3, tokens.Length);
            Assert.Equal(kind1, tokens[0].Kind);
            Assert.Equal(kind2, tokens[1].Kind);
            Assert.Equal(SyntaxTokenKind.End, tokens[2].Kind);
        }


        public static IEnumerable<object[]> GetAllTokenData()
        {
            foreach (var t in GetAllTokens())
                yield return new object[] { t.Item1, t.Item2 };
        }

        public static IEnumerable<object[]> GetAllTokenDataSquared()
        {
            foreach (var t1 in GetAllTokens())
                foreach (var t2 in GetAllTokens())
                    yield return new object[] { t1.Item1, t1.Item2, t2.Item1, t2.Item2 };
        }

        public static IEnumerable<object[]> GetStringTokenData()
        {
            foreach (var t in GetStringTokens())
                yield return new object[] { t.Item1, t.Item2 };
        }

        public static IEnumerable<object[]> GetIdentifierTokenData()
        {
            foreach (var t in GetIdentifierTokens())
                yield return new object[] { t.Item1, t.Item2 };
        }

        public static IEnumerable<object[]> GetNumberTokenData()
        {
            foreach (var t in GetNumberTokens())
                yield return new object[] { t.Item1, t.Item2 };
        }

        public static IEnumerable<object[]> GetLiteralTokenData()
        {
            foreach (var t in GetLiteralTokens())
                yield return new object[] { t.Item1, t.Item2 };
        }

        private static IEnumerable<(string, SyntaxTokenKind)> GetAllTokens()
        {
            return GetStringTokens().Concat(GetIdentifierTokens().Concat(GetNumberTokens().Concat(GetLiteralTokens())));
        }

        private static IEnumerable<(string, SyntaxTokenKind)> GetStringTokens()
        {
            yield return ("'Hello'", SyntaxTokenKind.String);
            yield return ("'\tthe l l o'", SyntaxTokenKind.String);
            yield return ("'Fett'", SyntaxTokenKind.String);
            yield return ("'Trololololololololol'", SyntaxTokenKind.String);
            yield return ("'1231235.023'", SyntaxTokenKind.String);
            yield return ("'0'", SyntaxTokenKind.String);
        }

        private static IEnumerable<(string, SyntaxTokenKind)> GetIdentifierTokens()
        {
            yield return ("Hello", SyntaxTokenKind.Identifier);
            yield return ("öäüêéàè", SyntaxTokenKind.Identifier);
            yield return ("ασδφοςεχ", SyntaxTokenKind.Identifier);
            yield return ("i", SyntaxTokenKind.Identifier);
            yield return ("ThisIsAIdentifier", SyntaxTokenKind.Identifier);
            yield return ("_", SyntaxTokenKind.Identifier);
            yield return ("_someName", SyntaxTokenKind.Identifier);
        }

        private static IEnumerable<(string, SyntaxTokenKind)> GetNumberTokens()
        {
            var random = new Random();
            for (int i = 0; i < 10; i++)
                yield return (random.Next(short.MaxValue).ToString(), SyntaxTokenKind.Int);
            for (int i = 0; i < 10; i++)
                yield return ((random.NextDouble() * short.MaxValue).ToString(), SyntaxTokenKind.Float);
        }

        private static IEnumerable<(string, SyntaxTokenKind)> GetLiteralTokens()
        {
            var kinds = (SyntaxTokenKind[])Enum.GetValues(typeof(SyntaxTokenKind));

            foreach (var kind in kinds)
            {
                var text = SyntaxFacts.GetStringRepresentation(kind);
                if (text != null)
                    yield return (text, kind);
            }
        }
    }
}