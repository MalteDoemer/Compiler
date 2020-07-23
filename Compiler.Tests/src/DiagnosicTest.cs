using System;
using System.Linq;
using System.Collections.Generic;
using Compiler.Binding;
using Compiler.Diagnostics;
using Compiler.Syntax;
using Xunit;
using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Test
{
    public class DiagnosicTest
    {

        // Lexer

        [Fact]
        public static void Report_Invlid_Decimal_Point()
        {
            var text = "1234[.]";
            AssertDiagnostic(text, ErrorMessage.InvalidDecimalPoint);
        }

        [Fact]
        public static void Report_Invlid_Escape_sequence()
        {
            var text = "var text = 'hello how are ?\\nOhh very nice[\\!]'";
            AssertDiagnostic(text, ErrorMessage.InvalidEscapeSequence, "!");
        }

        [Fact]
        public static void Report_Never_Closed_String()
        {
            var text = "['hello]";
            AssertDiagnostic(text, ErrorMessage.NeverClosedStringLiteral);
        }

        // Parser

        [Fact]
        public static void Report_Never_Closed_Curly()
        {
            var text = @"
                func test() [{
                    for var i = 0 i < 20 i++ {
                        println(i)
                    }]
            ";

            AssertDiagnostic(text, ErrorMessage.NeverClosedCurlyBrackets);
        }

        [Fact]
        public static void Report_Never_Closed_Parenthesis()
        {
            var text = "var i = 1 + [((10 -3) * 3]";
            AssertDiagnostic(text, ErrorMessage.NeverClosedParenthesis);
        }

        // Binder

        [Theory]
        [InlineData("var [14.03] = 0", SyntaxTokenKind.Identifier)]
        [InlineData("var i [134] 0", SyntaxTokenKind.Equal)]
        [InlineData("func f[+]) {}", SyntaxTokenKind.LParen)]
        public static void Report_Expected_Token(string text, SyntaxTokenKind expectedKind)
        {
            AssertDiagnostic(text, ErrorMessage.ExpectedToken, expectedKind);
        }

        [Theory]
        [InlineData("[§]", SyntaxTokenKind.Invalid)]
        [InlineData("[else]", SyntaxTokenKind.ElseKeyword)]
        [InlineData("print('',[]", SyntaxTokenKind.EndOfFile)]
        public static void Report_Unexpected_Token(string text, SyntaxTokenKind unExpectedKind)
        {
            AssertDiagnostic(text, ErrorMessage.UnexpectedToken, unExpectedKind);
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
        public static void Report_Unresolved_Identifier_In_Function_Call(string identifierName)
        {
            var text = $"var i = [{identifierName}](1,2,'fett')";
            AssertDiagnostic(text, ErrorMessage.UnresolvedIdentifier, identifierName);
        }

        [Theory]
        [InlineData("let i: int = ['fett']", "int", "str")]
        [InlineData("var state = true state = [36]", "bool", "int")]
        [InlineData("if ['this is a string'] {}", "bool", "str")]
        [InlineData("while [1] {}", "bool", "int")]
        public static void Report_Incompatible_Types(string text, string expected, string actual)
        {
            AssertDiagnostic(text, ErrorMessage.IncompatibleTypes, expected, actual);
        }

        [Fact]
        public static void Report_Wrong_Return_Type()
        {
            var text = @"
                func test() : int {
                    return [false]
                }
            ";
            AssertDiagnostic(text, ErrorMessage.IncompatibleTypes, TypeSymbol.Int, TypeSymbol.Bool);
        }

        [Fact]
        public static void Report_Cannot_Return_Void()
        {
            var text = @"
                func test() : int {
                    return [void]
                }
            ";
            AssertDiagnostic(text, ErrorMessage.IncompatibleTypes, TypeSymbol.Int, TypeSymbol.Void);
        }

        [Fact]
        public static void Report_Wrong_Return_Type_Void()
        {
            var text = @"
                func test() {
                    return [23]
                }
            ";
            AssertDiagnostic(text, ErrorMessage.IncompatibleTypes, TypeSymbol.Void, TypeSymbol.Int);
        }


        [Theory]
        [MemberData(nameof(GetBinaryData))]
        public static void Report_Illeagal_Binary_Operator(SyntaxTokenKind op, TypeSymbol t1, TypeSymbol t2)
        {
            var boundOp = BindBinaryOperator(op);
            Assert.NotEqual(BoundBinaryOperator.Invalid, boundOp);

            var resType = BindFacts.ResolveBinaryType(boundOp, t1, t2);

            if (resType != TypeSymbol.Invalid)
                return;

            var typeText1 = GetSampleText(t1);
            var typeText2 = GetSampleText(t2);
            var operatorText = SyntaxFacts.GetText(op);
            var text = $"{typeText1} [{operatorText}] {typeText2}";

            AssertDiagnostic(text, ErrorMessage.UnsupportedBinaryOperator, operatorText, t1, t2);
        }

        [Theory]
        [MemberData(nameof(GetUnaryData))]
        public static void Report_Illeagal_Unary_Operator(SyntaxTokenKind op, TypeSymbol type)
        {
            var boundOp = BindUnaryOperator(op);
            Assert.NotEqual(BoundUnaryOperator.Invalid, boundOp);

            var resType = BindFacts.ResolveUnaryType(boundOp, type);

            if (resType != TypeSymbol.Invalid)
                return;

            var typeText = GetSampleText(type);
            var operatorText = SyntaxFacts.GetText(op);
            var text = $"[{operatorText}]{typeText}";

            AssertDiagnostic(text, ErrorMessage.UnsupportedUnaryOperator, operatorText, type);
        }

        [Theory]
        [MemberData(nameof(GetRandomNames), 20)]
        public static void Report_Variable_Already_Declared(string identifierName)
        {
            var text = $@"
                    var {identifierName} = 0 
                    var [{identifierName}] = 13
            ";
            AssertDiagnostic(text, ErrorMessage.VariableAlreadyDeclared, identifierName);
        }

        [Theory]
        [InlineData("func test(a: int, b: str, c: float){} [test](1,'', 4.5, false, true)", "test", 3, 5)]
        [InlineData("[rand]()", "rand", 2, 0)]
        public static void Report_Wrong_Amount_Of_Arguments(string text, string name, int required, int recived)
        {
            AssertDiagnostic(text, ErrorMessage.WrongAmountOfArguments, name, required, recived);
        }

        [Fact]
        public static void Report_Expression_Cannot_Be_Void()
        {
            var text = "var i = [print('')]";
            AssertDiagnostic(text, ErrorMessage.CannotBeVoid);
        }

        [Theory]
        [InlineData("let i: int = [36.4]", "int", "float")]
        [InlineData("var text = 'test' text = [true]", "str", "bool")]
        [InlineData("len([36])", "str", "int")]
        [InlineData("rand(1, [4.5])", "int", "float")]
        public static void Report_Explicit_Conversion_Needed(string text, string to, string from)
        {
            AssertDiagnostic(text, ErrorMessage.MissingExplicitConversion, to, from);
        }

        [Theory]
        [InlineData("int(['hello'])", "str", "int")]
        [InlineData("bool([36])", "int", "bool")]
        [InlineData("bool(['not true'])", "str", "bool")]
        [InlineData("float(['not a number'])", "str", "float")]
        public static void Report_Cannot_Convert_Types(string text, string from, string to)
        {
            AssertDiagnostic(text, ErrorMessage.CannotConvert, from, to);
        }

        [Fact]
        public static void Report_Dublicated_Parameter()
        {
            var text = @"
                func test(a: int, b: float, c: str, [a: obj]): int {
                    return 36
                }
            ";

            AssertDiagnostic(text, ErrorMessage.DuplicatedParameters, "a");
        }

        [Theory]
        [MemberData(nameof(GetRandomNames), 20)]
        public static void Report_Function_Already_Declared(string identifierName)
        {
            var text = $@"
                func {identifierName}() {{ 
                    println ('test') 
                }}
                func [{identifierName}](): str {{ 
                    return 'test'
                }}
            ";
            AssertDiagnostic(text, ErrorMessage.FunctionAlreadyDeclared, identifierName);
        }

        [Fact]
        public static void Report_Cannot_Assing_To_Read_Only_As_Global()
        {
            var text = @"
                let c = 0
                [c] = 20
            ";
            AssertDiagnostic(text, ErrorMessage.CannotAssignToReadOnly, "c");
        }

        [Fact]
        public static void Report_Cannot_Assing_To_Read_Only_As_Local()
        {
            var text = @"
                func test() {
                    let c = false
                    [c] = true
                }
            ";
            AssertDiagnostic(text, ErrorMessage.CannotAssignToReadOnly, "c");
        }

        [Theory]
        [InlineData("break")]
        [InlineData("continue")]
        public static void Report_Break_Or_Continue_Onyl_In_Loop(string keyWord)
        {
            var text = $@"
                var i = 0
                if i < 10
                    [{keyWord}]
                else 
                    i = 10
            ";

            AssertDiagnostic(text, ErrorMessage.InvalidBreakOrContinue, keyWord);
        }

        [Fact]
        public static void Report_Cannot_Return_Outside_A_Function()
        {
            var text = @"
                [return 20]
            ";
            AssertDiagnostic(text, ErrorMessage.ReturnOnlyInFunction);
        }

        [Fact]
        public static void Report_Not_All_Paths_Return()
        {
            var text = @"
                func [test](n: int) : int {
                    while true{
                        if n == 36 
                            break
                        else if n == 1 
                            return 1
                        else 
                            n = 1
                    }
                }
            ";
            AssertDiagnostic(text, ErrorMessage.AllPathsMustReturn);
        }

        private static void AssertDiagnostic(string text, ErrorMessage message, params object[] values)
        {
            AssertDiagnostic(text, string.Format(DiagnosticBag.ErrorFormats[(int)message], values));
        }

        private static void AssertDiagnostic(string text, string message)
        {
            var annotatedText = AnnotatedText.Parse(text);
            var compilation = Compilation.CompileScript(new SourceText(annotatedText.Text, null), Compilation.StandardReferencePaths);


            if (annotatedText.Spans.Length < 1)
                throw new Exception($"Enter at least one diagnostic <{annotatedText.Spans.Length}>");
            else if (annotatedText.Spans.Length < 1)
                throw new Exception($"Only one diagnostic is should be enterd <{annotatedText.Spans.Length}>");

            var diagnostic = Assert.Single(compilation.Diagnostics.Errors);

            var actualMessage = diagnostic.Message;
            var expectedSpan = annotatedText.Spans[0];
            var actualSpan = diagnostic.Span;

            Assert.Equal(message, actualMessage);
            Assert.Equal(expectedSpan, actualSpan);
        }

        public static IEnumerable<object[]> GetRandomNames(int amount)
        {
            var random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUW_";
            string GetRandomName() => new string(Enumerable.Repeat(chars, random.Next(1, 10)).Select(s => s[random.Next(s.Length)]).ToArray());

            for (var i = 0; i < amount; i++)
            {
                var str = GetRandomName();

                while (!(SyntaxFacts.IsKeyWord(str) is null))
                    str = GetRandomName();

                yield return new object[] { str };
            }
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

        private static BoundBinaryOperator BindBinaryOperator(SyntaxTokenKind op)
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
                default: return BoundBinaryOperator.Invalid;
            }
        }

        private static BoundUnaryOperator BindUnaryOperator(SyntaxTokenKind op)
        {
            switch (op)
            {
                case SyntaxTokenKind.Plus: return BoundUnaryOperator.Identety;
                case SyntaxTokenKind.Minus: return BoundUnaryOperator.Negation;
                case SyntaxTokenKind.Bang: return BoundUnaryOperator.LogicalNot;
                case SyntaxTokenKind.Tilde: return BoundUnaryOperator.BitwiseNot;
                default: return BoundUnaryOperator.Invalid;
            }
        }

    }
}