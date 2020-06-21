using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Compiler.Diagnostics;
using Compiler.Text;

namespace Compiler.Syntax
{
    internal class Parser
    {
        private readonly DiagnosticBag diagnostics;
        private readonly SyntaxToken[] tokens;
        private int pos;
        private SyntaxToken current
        {
            get
            {
                if (pos < tokens.Length) return tokens[pos];
                else return tokens[tokens.Length - 1];
            }
        }
        private SourceText Text { get; }
        public bool IsFinished { get => current.Kind == SyntaxTokenKind.End; }

        public Parser(SourceText text, DiagnosticBag diagnostics)
        {
            Text = text;
            this.diagnostics = diagnostics;
            var lexer = new Lexer(text, diagnostics);
            tokens = lexer.Tokenize().ToArray();
            pos = 0;
        }

        private SyntaxToken MatchToken(SyntaxTokenKind kind)
        {
            if (kind == current.Kind) return Advance();
            else
            {
                diagnostics.ReportUnexpectedToken(current, kind);
                var res = new SyntaxToken(kind, current.Pos, current.Lenght, current.Value);
                pos++;
                return res;
            }
        }

        private SyntaxToken Advance()
        {
            var res = current;
            pos++;
            return res;
        }

        
        public CompilationUnit ParseCompilationUnit()
        {
            var expr = ParseExpression();
            var unit = new CompilationUnit(new TextSpan(0, Text.Length), expr);
            if (!IsFinished) diagnostics.ReportUnexpectedToken(current, SyntaxTokenKind.End);
            return unit;
        }

        private ExpressionSyntax ParseExpression(int lvl = SyntaxFacts.MaxPrecedence)
        {
            if (lvl == 0) return ParsePrimaryExpression();

            var left = ParseExpression(lvl - 1);

            while (current.Kind.GetBinaryPrecedence() == lvl)
            {
                var op = Advance();
                var right = ParseExpression(lvl - 1);
                left = new BinaryExpressionSyntax(op, left, right);
            }

            return left;

        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            if (current.Kind.IsLiteralExpression())
                return new LiteralExpressionSyntax(Advance());
            else if (current.Kind == SyntaxTokenKind.Identifier)
                return ParseIdentifier();
            else if (current.Kind.IsUnaryOperator())
                return new UnaryExpressionSyntax(Advance(), ParsePrimaryExpression());
            else if (current.Kind == SyntaxTokenKind.LParen)
                return ParseParenthesizedExpression();
            else
            {
                diagnostics.ReportUnexpectedToken(current);
                return new InvalidExpressionSyntax(Advance());
            }
        }

        private ExpressionSyntax ParseParenthesizedExpression()
        {
            pos++;
            var expr = ParseExpression();
            MatchToken(SyntaxTokenKind.RParen);
            return expr;
        }

        private ExpressionSyntax ParseIdentifier()
        {
            var identifier = Advance();

            if (current.Kind == SyntaxTokenKind.Equal)
            {
                var equalToken = Advance();
                return new AssignmentExpressionSyntax(identifier, equalToken, ParseExpression());
            }
            else
                return new VariableExpressionSyntax(identifier);
        }
    }
}