using System;
using System.Collections.Immutable;
using Compiler.Syntax;

namespace Compiler.Text
{
    public sealed class ColorizedText
    {
        public ColorizedText(SourceText text)
        {
            Tokens = ParseTokens(text);
            Text = text;
        }

        public ImmutableArray<ColorizedToken> Tokens { get; }
        public SourceText Text { get; }

        private static ImmutableArray<ColorizedToken> ParseTokens(SourceText text)
        {
            var lexer = new Lexer(text, true);
            var tokens = lexer.Tokenize(verbose: true).ToImmutableArray();
            var builder = ImmutableArray.CreateBuilder<ColorizedToken>(tokens.Length);

            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];

                switch (token.Kind)
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
                        if (i < tokens.Length - 1 && tokens[i + 1].Kind == SyntaxTokenKind.LParen)
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

            return builder.MoveToImmutable();
        }
    }

    public sealed class ColorizedToken
    {
        private SyntaxToken token;

        public ColorizedToken(SyntaxToken token, ConsoleColor color)
        {
            this.token = token;
            Color = color;
        }

        public ConsoleColor Color { get; }
        public TextSpan Span => token.Span;
        public SyntaxTokenKind kind => token.Kind;
    }
}