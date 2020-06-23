using System;
using Compiler;
using Compiler.Diagnostics;
using Compiler.Syntax;
using Xunit;

namespace Compiler.Test
{
    public class DiagnosicTest
    {

        [Fact]
        public static void Report_Invlid_Decimal_Point()
        {
            var text = "1234[.]";
            AssertDiagnostic(text, ErrorMessage.InvalidDecimalPoint);
        }

        [Fact]
        public static void Report_Never_Closed_String()
        {
            var text = "['hello]";
            AssertDiagnostic(text, ErrorMessage.NeverClosedStringLiteral);
        }

        [Fact]
        public static void Report_Never_Closed_Parenthesis()
        {
            var text = "var i = 1 + [((10 -3) * 3]";
            AssertDiagnostic(text, ErrorMessage.NeverClosedParenthesis);
        }

        [Fact]
        public static void Report_Never_Closed_Curly()
        {
            var text = @"
               [{
                   int i = 0
                   string str = 'fett'
            ]";
            AssertDiagnostic(text, ErrorMessage.NeverClosedCurlyBrackets);
        }

        [Theory]
        [InlineData("var [14.03] = 0", SyntaxTokenKind.Identifier)]
        [InlineData("var i [134] 0", SyntaxTokenKind.Equal)]
        public static void Report_Expected_Token(string text, SyntaxTokenKind expectedKind)
        {
            AssertDiagnostic(text, ErrorMessage.ExpectedToken, expectedKind);
        }

        [Theory]
        [InlineData()]
        public static void Report_Unexpected_Token(){}


        private static void AssertDiagnostic(string text, ErrorMessage message, params object[] values)
        {
            AssertDiagnostic(text, string.Format(DiagnosticBag.ErrorFormats[(int)message], values));
        }

        private static void AssertDiagnostic(string text, string expected)
        {
            var annotatedText = AnnotatedText.Parse(text);
            var compilation = Compilation.Compile(annotatedText.Text);
            var res = compilation.Evaluate();
            var diagnostics = AnnotatedText.UnindentLines(expected);

            if (diagnostics.Length != annotatedText.Spans.Length)
                throw new Exception($"Marks and diagnostic must be same amount <{annotatedText.Spans.Length}> <{diagnostics.Length}>");

            Assert.Equal(diagnostics.Length, compilation.Diagnostics.Length);

            for (int i = 0; i < diagnostics.Length; i++)
            {
                var expectedMessage = diagnostics[i];
                var actualMessage = compilation.Diagnostics[i].Message;
                var expectedSpan = annotatedText.Spans[i];
                var actualSpan = compilation.Diagnostics[i].Span;

                Assert.Equal(expectedMessage, actualMessage);
                Assert.Equal(expectedSpan, actualSpan);
            }
        }
    }
}