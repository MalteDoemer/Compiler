using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Compiler.Binding;
using Compiler.Syntax;

namespace Compiler.Text
{
    public sealed class Colorizer
    {
        public static ColorizedText ColorizeTokens(SourceText text)
        {
            var lexer = new Lexer(text, true);
            var tokens = lexer.Tokenize(verbose: true).ToImmutableArray();
            var builder = ImmutableArray.CreateBuilder<ColorizedSpan>(tokens.Length);

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

            return new ColorizedText(text, builder.MoveToImmutable());
        }


        internal static ColorizedText ColorizeBoundNode(BoundNode node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.BoundProgram:
                    return ColorizeBoundProgram((BoundProgram)node);
                case BoundNodeKind.BoundLiteralExpression:
                    return ColorizeBoundLiteralExpression((BoundLiteralExpression)node);
                case BoundNodeKind.BoundVariableExpression:
                    return ColorizeBoundVariableExpression((BoundVariableExpression)node);
                case BoundNodeKind.BoundUnaryExpression:
                    return ColorizeBoundUnaryExpression((BoundUnaryExpression)node);
                case BoundNodeKind.BoundBinaryExpression:
                    return ColorizeBoundBinaryExpression((BoundBinaryExpression)node);
                case BoundNodeKind.BoundCallExpression:
                    return ColorizeBoundCallExpression((BoundCallExpression)node);
                case BoundNodeKind.BoundConversionExpression:
                    return ColorizeBoundConversionExpression((BoundConversionExpression)node);
                case BoundNodeKind.BoundAssignmentExpression:
                    return ColorizeBoundAssignmentExpression((BoundAssignmentExpression)node);
                case BoundNodeKind.BoundBlockStatement:
                    return ColorizeBoundBlockStatement((BoundBlockStatement)node);
                case BoundNodeKind.BoundExpressionStatement:
                    return ColorizeBoundExpressionStatement((BoundExpressionStatement)node);
                case BoundNodeKind.BoundVariableDeclarationStatement:
                    return ColorizeBoundVariableDeclarationStatement((BoundVariableDeclarationStatement)node);
                case BoundNodeKind.BoundIfStatement:
                    return ColorizeBoundIfStatement((BoundIfStatement)node);
                case BoundNodeKind.BoundForStatement:
                    return ColorizeBoundForStatement((BoundForStatement)node);
                case BoundNodeKind.BoundWhileStatement:
                    return ColorizeBoundWhileStatement((BoundWhileStatement)node);
                case BoundNodeKind.BoundDoWhileStatement:
                    return ColorizeBoundDoWhileStatement((BoundDoWhileStatement)node);
                case BoundNodeKind.BoundConditionalGotoStatement:
                    return ColorizeBoundConditionalGotoStatement((BoundConditionalGotoStatement)node);
                case BoundNodeKind.BoundGotoStatement:
                    return ColorizeBoundGotoStatement((BoundGotoStatement)node);
                case BoundNodeKind.BoundLabelStatement:
                    return ColorizeBoundLabelStatement((BoundLabelStatement)node);
                case BoundNodeKind.BoundInvalidExpression:
                    return ColorizeBoundInvalidExpression((BoundInvalidExpression)node);
                case BoundNodeKind.BoundReturnStatement:
                    return ColorizeBoundReturnStatement((BoundReturnStatement)node);
                default: throw new Exception("Unexpected kind");
            }
        }

        private static ColorizedText ColorizeBoundProgram(BoundProgram node)
        {
            throw new NotImplementedException();
        }

        private static ColorizedText ColorizeBoundLiteralExpression(BoundLiteralExpression node)
        {
            throw new NotImplementedException();
        }

        private static ColorizedText ColorizeBoundVariableExpression(BoundVariableExpression node)
        {
            throw new NotImplementedException();
        }

        private static ColorizedText ColorizeBoundUnaryExpression(BoundUnaryExpression node)
        {
            throw new NotImplementedException();
        }

        private static ColorizedText ColorizeBoundBinaryExpression(BoundBinaryExpression node)
        {
            throw new NotImplementedException();
        }

        private static ColorizedText ColorizeBoundCallExpression(BoundCallExpression node)
        {
            throw new NotImplementedException();
        }

        private static ColorizedText ColorizeBoundConversionExpression(BoundConversionExpression node)
        {
            throw new NotImplementedException();
        }

        private static ColorizedText ColorizeBoundAssignmentExpression(BoundAssignmentExpression node)
        {
            throw new NotImplementedException();
        }

        private static ColorizedText ColorizeBoundBlockStatement(BoundBlockStatement node)
        {
            throw new NotImplementedException();
        }

        private static ColorizedText ColorizeBoundExpressionStatement(BoundExpressionStatement node)
        {
            throw new NotImplementedException();
        }

        private static ColorizedText ColorizeBoundVariableDeclarationStatement(BoundVariableDeclarationStatement node)
        {
            throw new NotImplementedException();
        }

        private static ColorizedText ColorizeBoundIfStatement(BoundIfStatement node)
        {
            throw new NotImplementedException();
        }

        private static ColorizedText ColorizeBoundForStatement(BoundForStatement node)
        {
            throw new NotImplementedException();
        }

        private static ColorizedText ColorizeBoundWhileStatement(BoundWhileStatement node)
        {
            throw new NotImplementedException();
        }

        private static ColorizedText ColorizeBoundDoWhileStatement(BoundDoWhileStatement node)
        {
            throw new NotImplementedException();
        }

        private static ColorizedText ColorizeBoundConditionalGotoStatement(BoundConditionalGotoStatement node)
        {
            throw new NotImplementedException();
        }

        private static ColorizedText ColorizeBoundGotoStatement(BoundGotoStatement node)
        {
            throw new NotImplementedException();
        }

        private static ColorizedText ColorizeBoundLabelStatement(BoundLabelStatement node)
        {
            throw new NotImplementedException();
        }

        private static ColorizedText ColorizeBoundInvalidExpression(BoundInvalidExpression node)
        {
            throw new NotImplementedException();
        }

        private static ColorizedText ColorizeBoundReturnStatement(BoundReturnStatement node)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class ColorizedText : IEnumerable<ColorizedSpan>
    {
        public ColorizedText(SourceText text, ImmutableArray<ColorizedSpan> spans)
        {
            Text = text;
            Spans = spans;
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

        public ColorizedText Concat(ColorizedText other)
        {
            var text = Text.Text + other.Text;
            return Colorizer.ColorizeTokens(text);
        }

    }

    public class ColorizedSpan
    {
        public virtual ConsoleColor Color { get; }
        public virtual TextSpan Span { get; }
    }

    public sealed class ColorizedToken : ColorizedSpan
    {
        public ColorizedToken(SyntaxToken token, ConsoleColor color)
        {
            Token = token;
            Color = color;
        }

        public override ConsoleColor Color { get; }
        public override TextSpan Span => Token.Span;
        public SyntaxToken Token { get; }
    }
}