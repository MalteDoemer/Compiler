using System;
using System.Linq;
using System.Collections.Generic;
using Compiler.Binding;
using Compiler.Diagnostics;
using Compiler.Syntax;
using Xunit;
using Compiler.Symbols;

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
        public static void Report_Cannot_Assing_To_Const_As_Global()
        {
            var text = @"
                const c = 0
                [c] = 20
            ";
            AssertDiagnostic(text, ErrorMessage.CannotAssignToConst, "c");
        }

        [Fact]
        public static void Report_Cannot_Assing_To_Const_As_Local()
        {
            var text = @"
                def test() {
                    const c = false
                    [c] = true
                }
            ";
            AssertDiagnostic(text, ErrorMessage.CannotAssignToConst, "c");
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

        [Theory]
        [InlineData("var [14.03] = 0", SyntaxTokenKind.Identifier)]
        [InlineData("var i [134] 0", SyntaxTokenKind.Equal)]
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
            var boundOp = BindBinaryOperator(op);
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
            var boundOp = BindUnaryOperator(op);
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

        [Fact]
        public static void Report_Expression_Cannot_Be_Void()
        {
            var text = "var i = [print('')]";
            AssertDiagnostic(text, ErrorMessage.CannotBeVoid);
        }

        [Fact]
        public static void No_Cascading_Errors_In_BlockStatments()
        {
            var text = @"
                {
                    var done = true
                    do {
                        do{
                            do {
                                do {
                                    do {
                                        do {
                                            do {
                                                do{
                                                    1 [+] false
                                                } while (!done)
                                            } while (!done)
                                        } while (!done)
                                    } while (!done)
                                } while (!done)
                            } while (!done)
                        } while (!done)
                    } while (!done)
                }
            ";

            AssertDiagnostic(text, ErrorMessage.UnsupportedBinaryOperator, '+', TypeSymbol.Int.Name, TypeSymbol.Bool.Name);
        }


        private static void AssertDiagnostic(string text, ErrorMessage message, params object[] values)
        {
            AssertDiagnostic(text, string.Format(DiagnosticBag.ErrorFormats[(int)message], values));
        }

        private static void AssertDiagnostic(string text, string expected)
        {
            var annotatedText = AnnotatedText.Parse(text);
            var compilation = Compilation.CompileScript(annotatedText.Text);
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
            yield return TypeSymbol.Int;
            yield return TypeSymbol.Float;
            yield return TypeSymbol.Bool;
            yield return TypeSymbol.String;
        }

        private static string GetSampleText(TypeSymbol symbol)
        {
            if (symbol == TypeSymbol.Int) return "36";
            else if (symbol == TypeSymbol.Float) return "123.45";
            else if (symbol == TypeSymbol.Bool) return "false";
            else if (symbol == TypeSymbol.String) return "'Fett'";
            else return "";
        }

        private static BoundBinaryOperator? BindBinaryOperator(SyntaxTokenKind op)
        {
            switch (op)
            {
                case SyntaxTokenKind.Plus: return BoundBinaryOperator.Addition;
                case SyntaxTokenKind.Minus: return BoundBinaryOperator.Subtraction;
                case SyntaxTokenKind.Star: return BoundBinaryOperator.Multiplication;
                case SyntaxTokenKind.Slash: return BoundBinaryOperator.Division;
                case SyntaxTokenKind.StarStar: return BoundBinaryOperator.Power;
                case SyntaxTokenKind.SlashSlah: return BoundBinaryOperator.Root;
                case SyntaxTokenKind.Percentage: return BoundBinaryOperator.Modulo;
                case SyntaxTokenKind.Ampersand: return BoundBinaryOperator.BitwiseAnd;
                case SyntaxTokenKind.Pipe: return BoundBinaryOperator.BitwiseOr;
                case SyntaxTokenKind.Hat: return BoundBinaryOperator.BitwiseXor;
                case SyntaxTokenKind.EqualEqual: return BoundBinaryOperator.EqualEqual;
                case SyntaxTokenKind.NotEqual: return BoundBinaryOperator.NotEqual;
                case SyntaxTokenKind.LessThan: return BoundBinaryOperator.LessThan;
                case SyntaxTokenKind.LessEqual: return BoundBinaryOperator.LessEqual;
                case SyntaxTokenKind.GreaterThan: return BoundBinaryOperator.GreaterThan;
                case SyntaxTokenKind.GreaterEqual: return BoundBinaryOperator.GreaterEqual;
                case SyntaxTokenKind.AmpersandAmpersand: return BoundBinaryOperator.LogicalAnd;
                case SyntaxTokenKind.PipePipe: return BoundBinaryOperator.LogicalOr;
                case SyntaxTokenKind.PlusEqual: return BoundBinaryOperator.Addition;
                case SyntaxTokenKind.MinusEqual: return BoundBinaryOperator.Subtraction;
                case SyntaxTokenKind.StarEqual: return BoundBinaryOperator.Multiplication;
                case SyntaxTokenKind.SlashEqual: return BoundBinaryOperator.Division;
                case SyntaxTokenKind.AmpersandEqual: return BoundBinaryOperator.BitwiseAnd;
                case SyntaxTokenKind.PipeEqual: return BoundBinaryOperator.BitwiseOr;
                case SyntaxTokenKind.PlusPlus: return BoundBinaryOperator.Addition;
                case SyntaxTokenKind.MinusMinus: return BoundBinaryOperator.Subtraction;
                default: return null;
            }
        }

        private static BoundUnaryOperator? BindUnaryOperator(SyntaxTokenKind op)
        {
            switch (op)
            {
                case SyntaxTokenKind.Plus: return BoundUnaryOperator.Identety;
                case SyntaxTokenKind.Minus: return BoundUnaryOperator.Negation;
                case SyntaxTokenKind.Bang: return BoundUnaryOperator.LogicalNot;
                case SyntaxTokenKind.Tilde: return BoundUnaryOperator.BitwiseNot;
                default: return null;
            }
        }

    }
}