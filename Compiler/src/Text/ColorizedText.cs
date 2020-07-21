using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Compiler.Binding;
using Compiler.Syntax;

namespace Compiler.Text
{
    public sealed class ColorizedText : IEnumerable<ColorizedToken>
    {
        public ColorizedText(SourceText text, ImmutableArray<ColorizedToken> spans)
        {
            Text = text;
            Tokens = spans;
        }


        public static ColorizedText ColorizeTokens(SourceText text)
        {
            var lexer = new Lexer(text, true);
            var tokens = lexer.Tokenize(verbose: true).ToImmutableArray();
            var builder = ImmutableArray.CreateBuilder<ColorizedToken>(tokens.Length);

            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                var next = i != tokens.Length - 1 ? tokens[i + 1] : null;
                builder.Add(ColorizeToken(token, next));
            }

            return new ColorizedText(text, builder.MoveToImmutable());
        }

        internal static ColorizedToken ColorizeToken(SyntaxToken token, SyntaxToken next)
        {
            switch (token.TokenKind)
            {
                case SyntaxTokenKind.Float:
                case SyntaxTokenKind.Int:
                    return new ColorizedToken(token, ConsoleColor.DarkGreen);
                case SyntaxTokenKind.IfKeyword:
                case SyntaxTokenKind.ElseKeyword:
                case SyntaxTokenKind.WhileKeyword:
                case SyntaxTokenKind.DoKeyword:
                case SyntaxTokenKind.ForKeyword:
                case SyntaxTokenKind.BreakKewyword:
                case SyntaxTokenKind.ContinueKeyword:
                case SyntaxTokenKind.ReturnKeyword:
                    return new ColorizedToken(token, ConsoleColor.Magenta);
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
                    return new ColorizedToken(token, ConsoleColor.Blue);
                case SyntaxTokenKind.String:
                    return new ColorizedToken(token, ConsoleColor.DarkCyan);
                case SyntaxTokenKind.Comment:
                    return new ColorizedToken(token, ConsoleColor.DarkGray);
                case SyntaxTokenKind.Identifier:
                    ConsoleColor color;
                    if (next != null && next.TokenKind == SyntaxTokenKind.LParen)
                        color = ConsoleColor.Yellow;
                    else
                        color = ConsoleColor.Cyan;
                    return new ColorizedToken(token, color);
                default:
                    return new ColorizedToken(token, ConsoleColor.Gray);
            }
        }

        public SourceText Text { get; }
        public ImmutableArray<ColorizedToken> Tokens { get; }

        public ColorizedToken this[int i] { get => Tokens[i]; }
        public IEnumerator<ColorizedToken> GetEnumerator() { foreach (var span in Tokens) yield return span; }
        IEnumerator IEnumerable.GetEnumerator() { foreach (var span in Tokens) yield return span; }

        public override string ToString() => Text.ToString();
        public string ToString(TextSpan span) => Text.ToString(span);
        public string ToString(int start, int len) => Text.ToString(start, len);
        public void WriteTo(TextWriter writer) => writer.WriteColorizedText(this);
    }

    public sealed class ColorizedToken
    {
        internal ColorizedToken(SyntaxToken token, ConsoleColor color)
        {
            Token = token;
            Color = color;
        }

        public ConsoleColor Color { get; }
        public TextSpan Span => Token.Location.Span;
        internal SyntaxToken Token { get; }
    }
}