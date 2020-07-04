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
        private readonly bool isScript;

        private SyntaxToken ahead { get => Peak(1); }
        private SyntaxToken current { get => Peak(0); }
        private int pos;

        public Parser(SourceText source, ImmutableArray<SyntaxToken> tokens, bool isScript)
        {
            this.source = source;
            this.tokens = tokens;
            this.isScript = isScript;
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

        private SyntaxToken Peak(int offset)
        {
            if (pos + offset < tokens.Length)
                return tokens[pos + offset];
            else return tokens[tokens.Length];
        }

        public CompilationUnitSyntax ParseCompilationUnit()
        {
            var members = ParseMembers();
            return new CompilationUnitSyntax(TextSpan.FromBounds(0, source.Length), members);
        }

        private ImmutableArray<MemberSyntax> ParseMembers()
        {
            var builder = ImmutableArray.CreateBuilder<MemberSyntax>();

            while (current.Kind != SyntaxTokenKind.EndOfFile)
            {
                var member = ParseMember();
                builder.Add(member);
            }
            return builder.ToImmutable();
        }

        private MemberSyntax ParseMember()
        {
            if (Peak(0).Kind.IsTypeKeyword() && Peak(1).Kind == SyntaxTokenKind.Identifier && Peak(2).Kind == SyntaxTokenKind.LParen)
                return ParseFunctionDeclaration();
            else return ParseGlobalStatement();
        }

        private MemberSyntax ParseGlobalStatement()
        {
            var stmt = ParseStatement();

            if (SyntaxFacts.IsGlobalStatement(stmt, isScript))
                return new GlobalStatementSynatx(stmt);

            else return new InvalidMemberSyntax(stmt.Span);

        }

        private MemberSyntax ParseFunctionDeclaration()
        {
            var returnType = Advance();

            if (!returnType.IsValid)
                return new InvalidMemberSyntax(returnType.Span);

            var identifier = MatchToken(SyntaxTokenKind.Identifier);

            if (!identifier.IsValid)
                return new InvalidMemberSyntax(identifier.Span);

            var lparen = MatchToken(SyntaxTokenKind.LParen);

            if (!lparen.IsValid)
                return new InvalidMemberSyntax(lparen.Span);


            var paramBuilder = ImmutableArray.CreateBuilder<ParameterSyntax>();
            var commaBuilder = ImmutableArray.CreateBuilder<SyntaxToken>();

            while (current.Kind != SyntaxTokenKind.RParen)
            {
                if (current.Kind == SyntaxTokenKind.EndOfFile)
                {
                    var span = TextSpan.FromBounds(lparen.Span.Start, current.Span.End);
                    diagnostics.ReportSyntaxError(ErrorMessage.NeverClosedParenthesis, span);
                    return new InvalidMemberSyntax(TextSpan.Invalid);
                }

                if (!current.Kind.IsTypeKeyword())
                    return new InvalidMemberSyntax(current.Span);

                var typeToken = Advance();
                var name = MatchToken(SyntaxTokenKind.Identifier);

                if (!name.IsValid)
                    return new InvalidMemberSyntax(name.Span);

                paramBuilder.Add(new ParameterSyntax(typeToken, name));

                if (current.Kind != SyntaxTokenKind.RParen)
                    commaBuilder.Add(MatchToken(SyntaxTokenKind.Comma));
            }

            var parameters = new SeperatedSyntaxList<ParameterSyntax>(paramBuilder.ToImmutable(), commaBuilder.ToImmutable());

            var rparen = MatchToken(SyntaxTokenKind.RParen);

            if (!rparen.IsValid)
                return new InvalidMemberSyntax(rparen.Span);

            var body = ParseBlockStatement();

            if (!body.IsValid)
                return new InvalidMemberSyntax(body.Span);

            return new FunctionDeclarationSyntax(returnType, identifier, lparen, parameters, rparen, (BlockStatment)body);
        }

        private StatementSyntax ParseStatement()
        {
            if (current.Kind == SyntaxTokenKind.LCurly)
                return ParseBlockStatement();
            else if (current.Kind == SyntaxTokenKind.IfKeyword)
                return ParseIfStatement();
            else if (current.Kind == SyntaxTokenKind.WhileKeyword)
                return ParseWhileStatement();
            else if (current.Kind == SyntaxTokenKind.ForKeyword)
                return ParseForStatement();
            else if (current.Kind == SyntaxTokenKind.DoKeyword)
                return ParseDoWhileStatement();
            else if (current.Kind.IsTypeKeyword())
                return ParseVariableDeclaration();
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

            var variableDeclaration = ParseVariableDeclaration();

            if (!variableDeclaration.IsValid)
                return new InvalidStatementSyntax(variableDeclaration.Span);

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

            return new ForStatementSyntax(forToken, variableDeclaration, condition, increment, body);
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
            if (expression is InvalidExpressionSyntax)
                return new InvalidStatementSyntax(expression.Span);

            if (SyntaxFacts.IsExpressionStatement(expression, isScript))
                return new ExpressionStatement(expression);
            else
            {
                diagnostics.ReportSyntaxError(ErrorMessage.InvalidStatement, expression.Span);
                return new InvalidStatementSyntax(expression.Span);
            }
        }

        private StatementSyntax ParseBlockStatement()
        {
            StatementSyntax returnError(TextSpan span)
            {
                if (current.Kind == SyntaxTokenKind.RCurly)
                    pos++;
                return new InvalidStatementSyntax(span);
            }

            var builder = ImmutableArray.CreateBuilder<StatementSyntax>();
            var lcurly = MatchToken(SyntaxTokenKind.LCurly);
            if (!lcurly.IsValid)
                returnError(lcurly.Span);

            while (current.Kind != SyntaxTokenKind.RCurly)
            {
                if (current.Kind == SyntaxTokenKind.EndOfFile)
                {
                    var span = TextSpan.FromBounds(lcurly.Span.Start, current.Span.Start);
                    diagnostics.ReportSyntaxError(ErrorMessage.NeverClosedCurlyBrackets, span);
                    return returnError(span);
                }

                var stmt = ParseStatement();

                if (!stmt.IsValid)
                    return returnError(stmt.Span);

                builder.Add(stmt);
            }

            var rcurly = MatchToken(SyntaxTokenKind.RCurly);

            if (!rcurly.IsValid)
                return returnError(rcurly.Span);

            return new BlockStatment(lcurly, builder.ToImmutable(), rcurly);
        }

        private StatementSyntax ParseVariableDeclaration()
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

            return new VariableDeclarationStatement(typeToken, identifier, equalToken, expr);
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
            else if (SyntaxFacts.IsTypeKeyword(current.Kind))
                return ParseFunctionCall(Advance());
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
                if (current.Kind == SyntaxTokenKind.EndOfFile)
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

            return new CallExpressionSyntax(identifier, lparen, new SeperatedSyntaxList<ExpressionSyntax>(argBuilder.ToImmutable(), commaBuilder.ToImmutable()), rparen);
        }

        private ElseStatementSyntax ParseElseClause()
        {
            if (current.Kind != SyntaxTokenKind.ElseKeyword)
                return null;
            var elseKeyword = Advance();
            var statement = ParseStatement();
            return new ElseStatementSyntax(elseKeyword, statement);
        }

        private TypeClauseSyntax ParseOptionalTypeClause()
        {
            if (current.Kind == SyntaxTokenKind.Colon && ahead.Kind.IsTypeKeyword())
                return ParseTypeClause();
            else return null;
        }

        private TypeClauseSyntax ParseTypeClause()
        {
            var colonToken = MatchToken(SyntaxTokenKind.Colon);
            var typeToken = Advance();

            return new TypeClauseSyntax(colonToken, typeToken);
        }
    }
}