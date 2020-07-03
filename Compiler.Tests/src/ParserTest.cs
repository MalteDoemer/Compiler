using Xunit;
using System;
using Compiler;
using System.Collections.Generic;
using Compiler.Syntax;

namespace Compiler.Test
{
    public class ParserTest
    {


        [Theory]
        [MemberData(nameof(GetBinaryOperatorPairs))]
        public static void TestBinaryPrecedence(SyntaxTokenKind op1, SyntaxTokenKind op2)
        {
            var text1 = SyntaxFacts.GetStringRepresentation(op1);
            var text2 = SyntaxFacts.GetStringRepresentation(op2);
            var precedence1 = SyntaxFacts.GetBinaryPrecedence(op1);
            var precedence2 = SyntaxFacts.GetBinaryPrecedence(op2);

            Assert.NotNull(text1);
            Assert.NotNull(text2);
            Assert.NotEqual(0, precedence1);
            Assert.NotEqual(0, precedence2);

            var text = $"a {text1} b {text2} c";
            var res = Compilation.SyntaxTreeToString(text);

            if (precedence1 > precedence2)
            {
                // (a + (b * c))
                var expected = $"(a {text1} (b {text2} c))\n";
                Assert.Equal(expected, res);
            }
            else
            {
                // ((a * b) + c)
                var expected = $"((a {text1} b) {text2} c)\n";
                Assert.Equal(expected, res);
            }

        }

        [Theory]
        [MemberData(nameof(GetBinaryUnaryPairs))]
        public static void TestUnaryBinaryPrecedence(SyntaxTokenKind binary, SyntaxTokenKind unary)
        {
            var text1 = SyntaxFacts.GetStringRepresentation(binary);
            var text2 = SyntaxFacts.GetStringRepresentation(unary);
            Assert.NotNull(text1);
            Assert.NotNull(text2);

            var text = $"a {text1} {text2}b";
            var res = Compilation.SyntaxTreeToString(text);
            var expected = $"(a {text1} ({text2}b))\n";
            Assert.Equal(expected, res);
        }

        public static IEnumerable<object[]> GetBinaryOperatorPairs()
        {
            foreach (var op1 in SyntaxFacts.GetBinaryOperators())
                foreach (var op2 in SyntaxFacts.GetBinaryOperators())
                    yield return new object[] { op1, op2 };
        }

        public static IEnumerable<object[]> GetBinaryUnaryPairs()
        {
            foreach (var op1 in SyntaxFacts.GetBinaryOperators())
                foreach (var op2 in SyntaxFacts.GetUnaryOperators())
                    yield return new object[] { op1, op2 };
        }

    }
}
