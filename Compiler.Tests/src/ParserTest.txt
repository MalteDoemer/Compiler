using System;
using System.Collections.Generic;
using Xunit;
using Compiler.Diagnostics;
using Compiler.Text;

namespace Compiler.Syntax
{
    public class ParserTest
    {

        [Theory]
        [MemberData(nameof(GetBinaryOperatorPairData))]
        public static void CheckPrecedence(SyntaxTokenKind op1, SyntaxTokenKind op2)
        {
            var precedence1 = op1.GetBinaryPrecedence();
            var precedence2 = op2.GetBinaryPrecedence();
            var text1 = op1.GetText();
            var text2 = op2.GetText();

            Assert.NotNull(text1);
            Assert.NotNull(text2);
            Assert.NotEqual(0, precedence1);
            Assert.NotEqual(0, precedence2);

            var text = $"a {text1} b {text2} c";
            var res = SyntaxTree.ParseSyntaxTree(new SourceText(text)).ToString();

            if (precedence1 > precedence2)
            {
                // (a + (b * c))
                var expected = $"(a {text1} (b {text2} c))";
                Assert.Equal(expected, res);
            }
            else
            {
                // ((a * b) + c)
                var expected = $"((a {text1} b) {text2} c)";
                Assert.Equal(expected, res);
            }

        }

        public static IEnumerable<object[]> GetBinaryOperatorPairData()
        {
            foreach (var op1 in SyntaxFacts.GetBinaryOperators())
                foreach (var op2 in SyntaxFacts.GetBinaryOperators())
                    yield return new object[] { op1, op2 };
        }

    }
}
