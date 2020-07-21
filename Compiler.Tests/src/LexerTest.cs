using System;
using System.Collections.Generic;
using System.Linq;
using Compiler.Syntax;
using Compiler.Text;
using Xunit;

namespace Compiler.Test
{
    public class LexerTest
    {

        [Theory]
        [MemberData(nameof(GetAllTokenData))]
        public static void LexAllTokens(string text, SyntaxTokenKind kind)
        {
            var tokens = SyntaxTree.Tokenize(new SourceText(text,null));
            Assert.Equal(2, tokens.Length);
            Assert.Equal(kind, tokens[0].TokenKind);
            Assert.Equal(SyntaxTokenKind.EndOfFile, tokens[1].TokenKind);
        }

        public static IEnumerable<object[]> GetAllTokenData()
        {
            foreach (var t in GetAllTokens())
                yield return new object[] { t.Item1, t.Item2 };
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
                var text = SyntaxFacts.GetText(kind);
                if (text != null)
                    yield return (text, kind);
            }
        }
    }
}