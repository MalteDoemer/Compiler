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
        private readonly SourceText source;
        private readonly ImmutableArray<SyntaxToken> tokens;
        private int pos;
        private SyntaxToken current
        {
            get
            {
                if (pos < tokens.Length) return tokens[pos];
                else return tokens[tokens.Length - 1];
            }
        }
        private bool IsFinished { get => current.Kind == SyntaxTokenKind.End; }

        public Parser(SourceText source, ImmutableArray<SyntaxToken> tokens)
        {
            this.source = source;
            this.tokens = tokens;
            diagnostics = new DiagnosticBag();
            pos = 0;
        }

        public IEnumerable<Diagnostic> GetDiagnostics() => diagnostics;

        private SyntaxToken MatchToken(SyntaxTokenKind kind)
        {
            if (kind == current.Kind) return Advance();
            else
            {
                diagnostics.ReportSyntaxError(ErrorMessage.ExpectedToken, current.Span, kind);
                var res = new SyntaxToken(kind, current.Span.Start, current.Span.Lenght, current.Value);
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

        public CompilationUnitSyntax ParseCompilationUnit()
        {
            var stmt = ParseStatement();
            var unit = new CompilationUnitSyntax(new TextSpan(0, source.Length), stmt);
            if (!IsFinished) diagnostics.ReportSyntaxError(ErrorMessage.ExpectedToken, current.Span, SyntaxTokenKind.End);
            return unit;
        }

        private StatementSyntax ParseStatement()
        {
            if (current.Kind.IsTypeKeyword())
                return ParseVariableDecleration();
            else if (current.Kind == SyntaxTokenKind.LCurly)
                return ParseBlockStatement();
            else if (current.Kind == SyntaxTokenKind.IfKeyword)
                return ParseIfStatement();
            else return ParseExpressionStatement();
        }

        private StatementSyntax ParseIfStatement()
        {
            var ifToken = MatchToken(SyntaxTokenKind.IfKeyword);
            var expr = ParseExpression();
            var statement = ParseStatement();
            var elseClause = ParseElseClause();
            return new IfStatement(ifToken, expr, statement, elseClause);
        }

        private ElseStatement ParseElseClause()
        {
            if (current.Kind != SyntaxTokenKind.ElseKeyword)
                return null;
            var elseKeyword = Advance();
            var statement = ParseStatement();
            return new ElseStatement(elseKeyword, statement);
        }

        private StatementSyntax ParseExpressionStatement()
        {
            var expression = ParseExpression();
            return new ExpressionStatement(expression);
        }

        private StatementSyntax ParseBlockStatement()
        {
            var lcurly = MatchToken(SyntaxTokenKind.LCurly);

            var builder = ImmutableArray.CreateBuilder<StatementSyntax>();

            while (current.Kind != SyntaxTokenKind.RCurly)
            {
                if (current.Kind == SyntaxTokenKind.End)
                {
                    var span = TextSpan.FromBounds(lcurly.Span.Start, current.Span.Start);
                    diagnostics.ReportSyntaxError(ErrorMessage.NeverClosedCurlyBrackets, span);
                    return new InvalidStatementSyntax(span);
                }
                builder.Add(ParseStatement());
            }
            var rcurly = MatchToken(SyntaxTokenKind.RCurly);
            return new BlockStatment(lcurly, builder.ToImmutable(), rcurly);
        }

        private StatementSyntax ParseVariableDecleration()
        {
            var typeToken = Advance();
            var identifier = MatchToken(SyntaxTokenKind.Identifier);
            var equalToken = MatchToken(SyntaxTokenKind.Equal);
            var expr = (ExpressionStatement)ParseExpressionStatement();

            return new VariableDeclerationStatement(typeToken, identifier, equalToken, expr.Expression);
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
            if (SyntaxFacts.IsLiteralExpression(current.Kind))
                return new LiteralExpressionSyntax(Advance());
            else if (current.Kind == SyntaxTokenKind.Identifier)
                return ParseIdentifier();
            else if (current.Kind.IsUnaryOperator())
                return new UnaryExpressionSyntax(Advance(), ParsePrimaryExpression());
            else if (current.Kind == SyntaxTokenKind.LParen)
                return ParseParenthesizedExpression();
            else
            {
                diagnostics.ReportSyntaxError(ErrorMessage.UnExpectedToken, current.Span, current.Kind);
                return new InvalidExpressionSyntax(Advance().Span);
            }
        }

        private ExpressionSyntax ParseParenthesizedExpression()
        {
            var start = pos;
            pos++;
            var expr = ParseExpression();
            if (current.Kind != SyntaxTokenKind.RParen)
                diagnostics.ReportSyntaxError(ErrorMessage.NeverClosedParenthesis, TextSpan.FromBounds(start, pos));
            else pos++;
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