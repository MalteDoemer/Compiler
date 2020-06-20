using System;
using System.Collections.Generic;
using Xunit;
using Compiler.Diagnostics;
using Compiler.Syntax;

namespace Compiler.Tests.Syntax
{
    public class ParserTest
    {

        [Theory]
        [MemberData(nameof(GetBinaryOperatorPairData))]
        public static void CheckPrecedence(SyntaxTokenKind op1, SyntaxTokenKind op2)
        {
            var precedence1 = SyntaxFacts.GetBinaryPrecedence(op1);
            var precedence2 = SyntaxFacts.GetBinaryPrecedence(op2);
            var text1 = SyntaxFacts.GetOperatorText(op1);
            var text2 = SyntaxFacts.GetOperatorText(op2);

            Assert.NotNull(text1);
            Assert.NotNull(text2);
            Assert.NotEqual(0, precedence1);
            Assert.NotEqual(0, precedence2);

            var text = $"a {text1} b {text2} c";
            var res = Evaluator.GetExpressionAsString(text, out DiagnosticBag bag);

            Assert.Equal(0, bag.Errors);

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
