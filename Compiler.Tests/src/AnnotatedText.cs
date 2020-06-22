using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using Compiler.Binding;
using Compiler.Diagnostics;
using Compiler.Syntax;
using Compiler.Text;
using Xunit;

namespace Compiler
{
    public class AnnotatedText
    {
        public AnnotatedText(string text, ImmutableArray<TextSpan> spans)
        {
            Text = text;
            Spans = spans;
        }

        public string Text { get; }
        public ImmutableArray<TextSpan> Spans { get; }

        public static AnnotatedText Parse(string text)
        {
            text = Unindent(text);

            var textBuilder = new StringBuilder();
            var spanBuilder = ImmutableArray.CreateBuilder<TextSpan>();

            var startStack = new Stack<int>();

            var pos = 0;
            foreach (var c in text)
            {
                if (c == '[')
                {
                    startStack.Push(pos);

                }
                else if (c == ']')
                {
                    if (startStack.Count <= 0) throw new ArgumentException("Brackets don't match up");

                    var start = startStack.Pop();
                    var span = TextSpan.FromBounds(start, pos);
                    spanBuilder.Add(span);
                }
                else
                {
                    pos++;
                    textBuilder.Append(c);
                }
            }

            if (startStack.Count != 0) throw new ArgumentException("Brackets don't match up");


            return new AnnotatedText(textBuilder.ToString(), spanBuilder.ToImmutable());
        }

        // public static string[] UnindentLines(string text)
        // {
        //     var lines = new List<string>();

        //     using (var reader = new StringReader(text))
        //     {
        //         string line;
        //         while ((line = reader.ReadLine()) != null)
        //             lines.Add(line);
        //     }
        //     var minIndent = int.MaxValue;
        //     foreach (var line in lines)
        //     {
        //         var indent = line.Length - line.TrimStart().Length;
        //         minIndent = Math.Min(minIndent, indent);
        //     }

        //     for (int i = 0; i < lines.Count; i++)
        //     {
        //         if (lines[i].Trim().Length == 0) lines[i] = string.Empty;
        //         lines[i] = lines[i].Substring(minIndent);
        //     }

        //     while (lines.Count > 0 && lines[0].Length == 0)
        //         lines.RemoveAt(0);

        //     while (lines.Count > 0 && lines[lines.Count - 1].Length == 0)
        //         lines.RemoveAt(lines.Count - 1);

        //     return lines.ToArray();
        // }

        public static string[] UnindentLines(string text)
        {
            var lines = new List<string>();

            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    lines.Add(line);
            }

            var minIndentation = int.MaxValue;
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                if (line.Trim().Length == 0)
                {
                    lines[i] = string.Empty;
                    continue;
                }

                var indentation = line.Length - line.TrimStart().Length;
                minIndentation = Math.Min(minIndentation, indentation);
            }

            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length == 0)
                    continue;

                lines[i] = lines[i].Substring(minIndentation);
            }

            while (lines.Count > 0 && lines[0].Length == 0)
                lines.RemoveAt(0);

            while (lines.Count > 0 && lines[lines.Count - 1].Length == 0)
                lines.RemoveAt(lines.Count - 1);

            return lines.ToArray();
        }

        public static string Unindent(string text)
        {
            return string.Join(Environment.NewLine, UnindentLines(text));
        }
    }

    public class AnnotatedTest
    {
        [Fact]
        public static void Report_Varible_Redecleration()
        {
            var text = @"
                {
                    var x = 0
                    int y = 10
                    {
                        bool x = false
                        y = 20
                    }
                    int [y] = 50
                }
            ";
            AssertDiagnosics(text, SpecificErroKind.VariableAlreadyDeclared, 'y');
        }

        [Fact]
        public static void Report_Varible_Not_Declared()
        {
            var text = @"
                {
                    bool x = false
                    [value] = 20
                }
            ";
            AssertDiagnosics(text, SpecificErroKind.VariableNotDeclared, "value");
        }

        [Fact]
        public static void Report_Wrong_Assing_Type()
        {
            var text = @"
                {
                    var x = ""hello""
                    x [=] false
                }
            ";
            AssertDiagnosics(text, SpecificErroKind.WrongType, TypeSymbol.String.ToString(), TypeSymbol.Bool.ToString());
        }

        [Fact]
        public static void Report_Wrong_Decleration_Type()
        {
            var text = @"
                {
                    int i = [null]
                }
            ";
            AssertDiagnosics(text, SpecificErroKind.WrongType, TypeSymbol.Int.ToString(), TypeSymbol.NullType.ToString());
        }

        [Fact]
        public static void Report_Unexpected_Token_1()
        {
            var text = @"
                {
                    [§]
                }
            ";
            AssertDiagnosics(text, SpecificErroKind.UnexpectedToken1, SyntaxTokenKind.Invalid.ToString());
        }

        [Fact]
        public static void Report_Unexpected_Token_2()
        {
            var text = @"
                {
                    var i = ( 1 [1]
                }
            ";
            AssertDiagnosics(text, SpecificErroKind.UnexpectedToken2, SyntaxTokenKind.RParen.ToString(), SyntaxTokenKind.Int.ToString());
        }

        [Fact]
        public static void Report_Never_Closed_String()
        {
            var text = "\"[hello i bims.]";
            AssertDiagnosics(text, SpecificErroKind.NeverClosedString);
        }

        [Fact]
        public static void Report_Ivnalid_Decimal_Point()
        {
            var text = @"1241234[.] ";
            AssertDiagnosics(text, SpecificErroKind.InvalidDecimalPoint);
        }


        [Theory]
        [InlineData("false [+] 123","+", TypeSymbol.Bool, TypeSymbol.Int)]
        [InlineData("12.6 [+] true","+", TypeSymbol.Float, TypeSymbol.Bool)]
        [InlineData("\"hello\" [+] null","+", TypeSymbol.String, TypeSymbol.NullType)]
        [InlineData("123 [+] null","+", TypeSymbol.Int, TypeSymbol.NullType)]
        [InlineData("1234.123123 [+] null","+", TypeSymbol.Float, TypeSymbol.NullType)]

        [InlineData("false [-] 123","-", TypeSymbol.Bool, TypeSymbol.Int)]
        [InlineData("12.6 [-] true","-", TypeSymbol.Float, TypeSymbol.Bool)]
        [InlineData("\"hello\" [-] null","-", TypeSymbol.String, TypeSymbol.NullType)]
        [InlineData("123 [-] null","-", TypeSymbol.Int, TypeSymbol.NullType)]
        [InlineData("1234.123123 [-] null","-", TypeSymbol.Float, TypeSymbol.NullType)]
        
        [InlineData("false [*] 123","*", TypeSymbol.Bool, TypeSymbol.Int)]
        [InlineData("12.6 [*] true","*", TypeSymbol.Float, TypeSymbol.Bool)]
        [InlineData("\"hello\" [*] 3.6","*", TypeSymbol.String, TypeSymbol.Float)]
        [InlineData("\"hello\" [*] true", "*",TypeSymbol.String, TypeSymbol.Bool)]
        [InlineData("123 [*] null","*", TypeSymbol.Int, TypeSymbol.NullType)]
        [InlineData("1234.123123 [*] null","*", TypeSymbol.Float, TypeSymbol.NullType)]
        
        [InlineData("false [/] 123","/", TypeSymbol.Bool, TypeSymbol.Int)]
        [InlineData("12.6 [/] true","/", TypeSymbol.Float, TypeSymbol.Bool)]
        [InlineData("\"hello\" [/] 3.6","/", TypeSymbol.String, TypeSymbol.Float)]
        [InlineData("\"hello\" [/] true","/", TypeSymbol.String, TypeSymbol.Bool)]
        [InlineData("123 [/] null","/", TypeSymbol.Int, TypeSymbol.NullType)]
        [InlineData("1234.123123 [/] null","/", TypeSymbol.Float, TypeSymbol.NullType)]
        public static void Report_UnsupportedBinaryOperator(string text, string op, TypeSymbol left, TypeSymbol right)
        {
            AssertDiagnosics(text, SpecificErroKind.UnsupportedBinaryOperator, op,  left.ToString(), right.ToString());
        }


        public static void AssertDiagnosics(string text, string diagnosic)
        {
            var annotatedText = AnnotatedText.Parse(text);
            var tree = SyntaxTree.ParseSyntaxTree(new SourceText(annotatedText.Text));
            var compilation = new Compilation(tree);
            var res = compilation.Evaluate(new Dictionary<string, VariableSymbol>());
            var errors = compilation.Tree.Diagnostics.GetErrors().ToImmutableArray();

            var diagnosics = AnnotatedText.UnindentLines(diagnosic);

            if (annotatedText.Spans.Length != diagnosics.Length)
                throw new Exception($"Marks and diagnostic must be same amount <{annotatedText.Spans.Length}> <{diagnosics.Length}>");


            Assert.Equal(errors.Length, diagnosics.Length);

            for (int i = 0; i < diagnosics.Length; i++)
            {
                var expectedMessage = diagnosics[i];
                var actualMessage = errors[i].Message;
                var expectedSpan = annotatedText.Spans[i];
                var actualSpan = errors[i].Span;

                Assert.Equal(expectedMessage, actualMessage);
                Assert.Equal(expectedSpan, actualSpan);
            }
        }

        public static void AssertDiagnosics(string text, SpecificErroKind kind, params object[] values)
        {
            AssertDiagnosics(text, string.Format(DiagnosticBag.Formats[(int)kind], values));
        }
    }
}