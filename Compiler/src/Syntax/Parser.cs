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

        private SyntaxToken current { get => pos < tokens.Length ? tokens[pos] : tokens[tokens.Length - 1]; }
        private int pos;

        public Parser(SourceText source, ImmutableArray<SyntaxToken> tokens)
        {
            this.source = source;
            this.tokens = tokens;
            diagnostics = new DiagnosticBag();
        }

        public IEnumerable<Diagnostic> GetDiagnostics() => diagnostics;

        private SyntaxToken MatchToken(SyntaxTokenKind kind)
        {
            if (kind == current.Kind) return Advance();
            else
            {
                diagnostics.ReportSyntaxError(ErrorMessage.ExpectedToken, current.Span, kind);
                var res = new SyntaxToken(kind, current.Span.Start, current.Span.Length, current.Value, false);
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
            var unit = new CompilationUnitSyntax(TextSpan.FromLength(0, source.Length), stmt);
            if (stmt.IsValid && current.Kind != SyntaxTokenKind.End) diagnostics.ReportSyntaxError(ErrorMessage.ExpectedToken, current.Span, SyntaxTokenKind.End);
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
            else if (current.Kind == SyntaxTokenKind.WhileKeyword)
                return ParseWhileStatement();
            else if (current.Kind == SyntaxTokenKind.ForKeyword)
                return ParseForStatement();
            else if (current.Kind == SyntaxTokenKind.DoKeyword)
                return ParseDoWhileStatement();
            else return ParseExpressionStatement();
        }

        private StatementSyntax ParseDoWhileStatement()
        {
            var doToken = MatchToken(SyntaxTokenKind.DoKeyword);

            if (!doToken.IsValid)
                return new InvalidStatementSyntax(doToken.Span);

            var stmt = ParseStatement();

            if (!stmt.IsValid)
                return new InvalidStatementSyntax(stmt.Span);

            var whileToken = MatchToken(SyntaxTokenKind.WhileKeyword);

            if (!whileToken.IsValid)
                return new InvalidStatementSyntax(whileToken.Span);

            var condition = ParseExpression();

            return new DoWhileStatementSyntax(doToken, stmt, whileToken, condition);
        }

        private StatementSyntax ParseForStatement()
        {
            var forToken = MatchToken(SyntaxTokenKind.ForKeyword);
            if (!forToken.IsValid)
                return new InvalidStatementSyntax(forToken.Span);

            var variableDecleration = ParseVariableDecleration();

            if (!variableDecleration.IsValid)
                return new InvalidStatementSyntax(variableDecleration.Span);

            var comma1 = MatchToken(SyntaxTokenKind.Comma);

            if (!comma1.IsValid)
                return new InvalidStatementSyntax(comma1.Span);

            var condition = ParseExpression();

            if (!condition.IsValid)
                return new InvalidStatementSyntax(condition.Span);

            var comma2 = MatchToken(SyntaxTokenKind.Comma);

            if (!comma2.IsValid)
                return new InvalidStatementSyntax(comma2.Span);

            var increment = ParseExpression();

            if (!increment.IsValid)
                return new InvalidStatementSyntax(increment.Span); ;

            var body = ParseStatement();

            if (!body.IsValid)
                return new InvalidStatementSyntax(body.Span);

            return new ForStatementSyntax(forToken, variableDecleration, condition, increment, body);
        }

        private StatementSyntax ParseIfStatement()
        {
            var ifToken = MatchToken(SyntaxTokenKind.IfKeyword);
            if (!ifToken.IsValid)
                return new InvalidStatementSyntax(ifToken.Span);

            var expr = ParseExpression();
            if (!expr.IsValid)
                return new InvalidStatementSyntax(expr.Span);

            var statement = ParseStatement();
            if (!statement.IsValid)
                return new InvalidStatementSyntax(expr.Span);

            var elseClause = ParseElseClause();
            return new IfStatementSyntax(ifToken, expr, statement, elseClause);
        }

        private StatementSyntax ParseWhileStatement()
        {
            var whileToken = MatchToken(SyntaxTokenKind.WhileKeyword);
            if (!whileToken.IsValid)
                return new InvalidStatementSyntax(whileToken.Span);

            var condition = ParseExpression();
            if (!condition.IsValid)
                return new InvalidStatementSyntax(condition.Span);

            var body = ParseStatement();
            if (!body.IsValid)
                return new InvalidStatementSyntax(body.Span);

            return new WhileStatementSyntax(whileToken, condition, body);
        }

        private StatementSyntax ParseExpressionStatement()
        {
            var expression = ParseExpression();
            if (SyntaxFacts.IsValidExpression(expression))
                return new ExpressionStatement(expression);
            else return new InvalidStatementSyntax(expression.Span);
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
            if (!typeToken.IsValid)
                return new InvalidStatementSyntax(typeToken.Span);

            var identifier = MatchToken(SyntaxTokenKind.Identifier);
            if (!identifier.IsValid)
                return new InvalidStatementSyntax(identifier.Span);

            var equalToken = MatchToken(SyntaxTokenKind.Equal);
            if (!equalToken.IsValid)
                return new InvalidStatementSyntax(equalToken.Span);

            var expr = ParseExpression();
            if (!expr.IsValid)
                return new InvalidStatementSyntax(expr.Span);

            return new VariableDeclerationStatement(typeToken, identifier, equalToken, expr);
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
            var start = current.Span.Start;
            MatchToken(SyntaxTokenKind.LParen);
            var expr = ParseExpression();
            if (current.Kind != SyntaxTokenKind.RParen)
                diagnostics.ReportSyntaxError(ErrorMessage.NeverClosedParenthesis, TextSpan.FromBounds(start, current.Span.End));
            else MatchToken(SyntaxTokenKind.RParen);
            return expr;
        }

        private ExpressionSyntax ParseIdentifier()
        {
            var identifier = MatchToken(SyntaxTokenKind.Identifier);

            switch (current.Kind)
            {
                case SyntaxTokenKind.Equal:
                    var equalToken = Advance();
                    var expr1 = ParseExpression();
                    return new AssignmentExpressionSyntax(identifier, equalToken, expr1);
                case SyntaxTokenKind.PlusEqual:
                case SyntaxTokenKind.MinusEqual:
                case SyntaxTokenKind.StarEqual:
                case SyntaxTokenKind.SlashEqual:
                case SyntaxTokenKind.AmpersandEqual:
                case SyntaxTokenKind.PipeEqual:
                    var op1 = Advance();
                    var expr2 = ParseExpression();
                    return new AdditionalAssignmentExpression(identifier, op1, expr2);
                case SyntaxTokenKind.PlusPlus:
                case SyntaxTokenKind.MinusMinus:
                    var op2 = Advance();
                    return new PostIncDecExpression(identifier, op2);
                case SyntaxTokenKind.LParen:
                    return ParseFunctionCall(identifier);
                default:
                    return new VariableExpressionSyntax(identifier);
            }
        }

        private ExpressionSyntax ParseFunctionCall(SyntaxToken identifier)
        {
            var argBuilder = ImmutableArray.CreateBuilder<ExpressionSyntax>();
            var commaBuilder = ImmutableArray.CreateBuilder<SyntaxToken>();
            var lparen = MatchToken(SyntaxTokenKind.LParen);

            while (current.Kind != SyntaxTokenKind.RParen)
            {
                if (current.Kind == SyntaxTokenKind.End)
                {
                    var span = TextSpan.FromBounds(lparen.Span.Start, current.Span.End);
                    diagnostics.ReportSyntaxError(ErrorMessage.NeverClosedParenthesis, span);
                    return new InvalidExpressionSyntax(span);
                }

                argBuilder.Add(ParseExpression());

                if (current.Kind != SyntaxTokenKind.RParen)
                    commaBuilder.Add(MatchToken(SyntaxTokenKind.Comma));
            }
            var rparen = MatchToken(SyntaxTokenKind.RParen);
            
            return new CallExpressionSyntax(identifier, new ArgumentList(lparen, argBuilder.ToImmutable(), commaBuilder.ToImmutable(), rparen));
        }

        private ElseStatementSyntax ParseElseClause()
        {
            if (current.Kind != SyntaxTokenKind.ElseKeyword)
                return null;
            var elseKeyword = Advance();
            var statement = ParseStatement();
            return new ElseStatementSyntax(elseKeyword, statement);
        }


    }
}