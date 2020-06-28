using System;
using System.Linq;
using System.Collections.Generic;
using Compiler;
using Compiler.Binding;
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
        [InlineData("1 [1]", SyntaxTokenKind.End)]
        public static void Report_Expected_Token(string text, SyntaxTokenKind expectedKind)
        {
            AssertDiagnostic(text, ErrorMessage.ExpectedToken, expectedKind);
        }

        [Theory]
        [InlineData("[§]", SyntaxTokenKind.Invalid)]
        [InlineData("[else]", SyntaxTokenKind.ElseKeyword)]
        public static void Report_Unexpected_Token(string text, SyntaxTokenKind unExpectedKind)
        {
            AssertDiagnostic(text, ErrorMessage.UnExpectedToken, unExpectedKind);
        }


        [Theory]
        [MemberData(nameof(GetBinaryData))]
        public static void Report_Binary_Operators(SyntaxTokenKind op, TypeSymbol t1, TypeSymbol t2)
        {
            var boundOp = BindFacts.BindBinaryOperator(op);
            Assert.NotNull(boundOp);

            var resType = BindFacts.ResolveBinaryType((BoundBinaryOperator)boundOp, t1, t2);

            if (resType != null)
                return;

            var typeText1 = GetSampleText(t1);
            var typeText2 = GetSampleText(t2);
            var operatorText = SyntaxFacts.GetStringRepresentation(op);
            var text = $"{typeText1} [{operatorText}] {typeText2}";

            AssertDiagnostic(text, ErrorMessage.UnsupportedBinaryOperator, operatorText, t1, t2);
        }

        [Theory]
        [MemberData(nameof(GetUnaryData))]
        public static void Report_Unary_Operators(SyntaxTokenKind op, TypeSymbol type)
        {
            var boundOp = BindFacts.BindUnaryOperator(op);
            Assert.NotNull(boundOp);

            var resType = BindFacts.ResolveUnaryType(boundOp, type);

            if (resType != null)
                return;

            var typeText = GetSampleText(type);
            var operatorText = SyntaxFacts.GetStringRepresentation(op);
            var text = $"[{operatorText}]{typeText}";

            AssertDiagnostic(text, ErrorMessage.UnsupportedUnaryOperator, operatorText, type);
        }

        [Theory]
        [MemberData(nameof(GetRandomNames), 20)]
        public static void Report_Unresolved_Identifier_In_Assignment(string identifierName)
        {
            var text = $"[{identifierName}] = 1 + 2 * 3";
            AssertDiagnostic(text, ErrorMessage.UnresolvedIdentifier, identifierName);
        }

        [Theory]
        [MemberData(nameof(GetRandomNames), 20)]
        public static void Report_Unresolved_Identifier_In_Expression(string identifierName)
        {
            var text = $"var i = [{identifierName}]";
            AssertDiagnostic(text, ErrorMessage.UnresolvedIdentifier, identifierName);
        }

        [Theory]
        [MemberData(nameof(GetRandomNames), 20)]
        public static void Report_Variable_Already_Declared(string identifierName)
        {
            var text = $@"
                {{
                    var {identifierName} = 0 
                    var [{identifierName}] = 13
                }}
            ";
            AssertDiagnostic(text, ErrorMessage.VariableAlreadyDeclared, identifierName);
        }


        private static void AssertDiagnostic(string text, ErrorMessage message, params object[] values)
        {
            AssertDiagnostic(text, string.Format(DiagnosticBag.ErrorFormats[(int)message], values));
        }

        private static void AssertDiagnostic(string text, string expected)
        {
            var annotatedText = AnnotatedText.Parse(text);
            var compilation = Compilation.Compile(annotatedText.Text);
            compilation.Evaluate();
            var diagnostics = AnnotatedText.UnindentLines(expected);

            if (diagnostics.Length != annotatedText.Spans.Length)
                throw new Exception($"Marks and diagnostic must be same amount <{annotatedText.Spans.Length}> <{diagnostics.Length}>");

            Assert.Equal(diagnostics.Length, compilation.Diagnostics.Length);

            var len = Math.Min(diagnostics.Length, compilation.Diagnostics.Length);
            for (int i = 0; i < len; i++)
            {
                var expectedMessage = diagnostics[i];
                var actualMessage = compilation.Diagnostics[i].Message;
                var expectedSpan = annotatedText.Spans[i];
                var actualSpan = compilation.Diagnostics[i].Span;

                Assert.Equal(expectedMessage, actualMessage);
                Assert.Equal(expectedSpan, actualSpan);
            }
        }

        public static IEnumerable<object[]> GetRandomNames(int amount)
        {
            var random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUW_";
            for (int i = 0; i < amount; i++)
                yield return new object[] { new string(Enumerable.Repeat(chars, random.Next(1, 10)).Select(s => s[random.Next(s.Length)]).ToArray()) };
        }

        public static IEnumerable<object[]> GetUnaryData()
        {
            foreach (var op in SyntaxFacts.GetUnaryOperators())
                foreach (var type in GetTypes())
                    yield return new object[] { op, type };
        }

        public static IEnumerable<object[]> GetBinaryData()
        {
            foreach (var op in SyntaxFacts.GetBinaryOperators())
                foreach (var t1 in GetTypes())
                    foreach (var t2 in GetTypes())
                        yield return new object[] { op, t1, t2 };
        }

        private static IEnumerable<TypeSymbol> GetTypes()
        {
            var types = (TypeSymbol[])Enum.GetValues(typeof(TypeSymbol));

            return types.Where(t => t != TypeSymbol.ErrorType);
        }

        private static string GetSampleText(TypeSymbol symbol)
        {
            switch (symbol)
            {
                case TypeSymbol.Bool: return "false";
                case TypeSymbol.Int: return "36";
                case TypeSymbol.Float: return "123.45";
                case TypeSymbol.String: return "'Fett'";
                default: return null;
            }
        }
    }
}