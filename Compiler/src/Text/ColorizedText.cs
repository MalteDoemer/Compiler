using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Compiler.Binding;
using Compiler.Syntax;

namespace Compiler.Text
{
    public sealed class ColorizedText : IEnumerable<ColorizedSpan>
    {
        public ColorizedText(SourceText text, ImmutableArray<ColorizedSpan> spans)
        {
            Text = text;
            Spans = spans;
        }

        public static ColorizedText ColorizeTokens(SourceText text)
        {
            var lexer = new Lexer(text, true);
            var tokens = lexer.Tokenize(verbose: true).ToImmutableArray();
            var builder = ImmutableArray.CreateBuilder<ColorizedSpan>(tokens.Length);

            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];

                switch (token.TokenKind)
                {
                    case SyntaxTokenKind.Float:
                    case SyntaxTokenKind.Int:
                        builder.Add(new ColorizedToken(token, ConsoleColor.DarkGreen));
                        break;
                    case SyntaxTokenKind.IfKeyword:
                    case SyntaxTokenKind.ElseKeyword:
                    case SyntaxTokenKind.WhileKeyword:
                    case SyntaxTokenKind.DoKeyword:
                    case SyntaxTokenKind.ForKeyword:
                    case SyntaxTokenKind.BreakKewyword:
                    case SyntaxTokenKind.ContinueKeyword:
                    case SyntaxTokenKind.ReturnKeyword:
                        builder.Add(new ColorizedToken(token, ConsoleColor.Magenta));
                        break;
                    case SyntaxTokenKind.True:
                    case SyntaxTokenKind.False:
                    case SyntaxTokenKind.VarKeyword:
                    case SyntaxTokenKind.VoidKeyword:
                    case SyntaxTokenKind.AnyKeyword:
                    case SyntaxTokenKind.IntKeyword:
                    case SyntaxTokenKind.FloatKeyword:
                    case SyntaxTokenKind.StringKeyword:
                    case SyntaxTokenKind.BoolKeyword:
                    case SyntaxTokenKind.ConstKeyword:
                    case SyntaxTokenKind.FunctionDefinitionKeyword:
                        builder.Add(new ColorizedToken(token, ConsoleColor.Blue));
                        break;
                    case SyntaxTokenKind.String:
                        builder.Add(new ColorizedToken(token, ConsoleColor.DarkCyan));
                        break;
                    case SyntaxTokenKind.Comment:
                        builder.Add(new ColorizedToken(token, ConsoleColor.DarkGray));
                        break;
                    case SyntaxTokenKind.Identifier:
                        ConsoleColor color;
                        if (i < tokens.Length - 1 && tokens[i + 1].TokenKind == SyntaxTokenKind.LParen)
                            color = ConsoleColor.Yellow;
                        else
                            color = ConsoleColor.Cyan;
                        builder.Add(new ColorizedToken(token, color));
                        break;
                    default:
                        builder.Add(new ColorizedToken(token, ConsoleColor.Gray));
                        break;
                }
            }

            return new ColorizedText(text, builder.MoveToImmutable());
        }

        public SourceText Text { get; }
        public ImmutableArray<ColorizedSpan> Spans { get; }

        public ColorizedSpan this[int i] { get => Spans[i]; }
        public IEnumerator<ColorizedSpan> GetEnumerator() { foreach (var span in Spans) yield return span; }
        IEnumerator IEnumerable.GetEnumerator() { foreach (var span in Spans) yield return span; }

        public override string ToString() => Text.ToString();
        public string ToString(TextSpan span) => Text.ToString(span);
        public string ToString(int start, int len) => Text.ToString(start, len);
        public void WriteTo(TextWriter writer) => writer.WriteColorizedText(this);
    }

    public class ColorizedSpan
    {
        public ColorizedSpan()
        {
            Span = TextSpan.Undefined;
            Color = ConsoleColor.Gray;
        }

        public ColorizedSpan(TextSpan span, ConsoleColor color)
        {
            Span = span;
            Color = color;
        }

        public ColorizedSpan(int start, int len, ConsoleColor color)
        {
            Span = TextSpan.FromLength(start, len);
            Color = color;
        }

        public virtual TextSpan Span { get; }
        public virtual ConsoleColor Color { get; }
    }

    public sealed class ColorizedToken : ColorizedSpan
    {
        internal ColorizedToken(SyntaxToken token, ConsoleColor color)
        {
            Token = token;
            Color = color;
        }

        public override ConsoleColor Color { get; }
        public override TextSpan Span => Token.Location.Span;
        internal SyntaxToken Token { get; }
    }
}