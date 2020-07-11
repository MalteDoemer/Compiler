using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Compiler.Diagnostics;
using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Syntax
{
    internal class Parser : IDiagnostable
    {
        private readonly DiagnosticBag diagnostics;
        private readonly SourceText source;
        private readonly ImmutableArray<SyntaxToken> tokens;
        private readonly bool isScript;

        private bool isTreeValid = true;
        private SyntaxToken current { get => Peak(0); }
        private int pos;

        public Parser(SourceText source, ImmutableArray<SyntaxToken> tokens, bool isScript)
        {
            this.source = source;
            this.tokens = tokens;
            this.isScript = isScript;
            diagnostics = new DiagnosticBag(source);
        }

        public IEnumerable<Diagnostic> GetDiagnostics() => diagnostics;


        private void ReportError(ErrorKind kind, ErrorMessage message, TextSpan span, params object[] values)
        {
            if (isTreeValid)
                diagnostics.ReportDiagnostic(message, span, kind, ErrorLevel.Error, values);
            isTreeValid = false;
        }

        private SyntaxToken MatchToken(SyntaxTokenKind kind, params SyntaxTokenKind[] others)
        {
            if (current.Kind == kind) return Advance();

            foreach (var kind2 in others)
                if (current.Kind == kind2) return Advance();

            ReportError(ErrorKind.SyntaxError, ErrorMessage.ExpectedToken, current.Span, kind);
            var res = new SyntaxToken(kind, current.Span.Start, current.Span.Length, current.Value, false);
            pos++;
            return res;
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
            else return tokens[tokens.Length - 1];
        }

        public CompilationUnitSyntax ParseCompilationUnit()
        {
            var members = ParseMembers();
            return new CompilationUnitSyntax(source, members, isTreeValid);
        }

        private ImmutableArray<MemberSyntax> ParseMembers()
        {
            var builder = ImmutableArray.CreateBuilder<MemberSyntax>();

            while (current.Kind != SyntaxTokenKind.End)
            {
                var member = ParseMember();
                builder.Add(member);
            }
            return builder.ToImmutable();
        }

        private MemberSyntax ParseMember()
        {
            if (current.Kind == SyntaxTokenKind.FunctionDefinitionKeyword)
                return ParseFunctionDeclaration();
            else return ParseGlobalStatement();
        }

        private GlobalStatementSynatx ParseGlobalStatement()
        {
            var stmt = ParseStatement();
            if (!SyntaxFacts.IsGlobalStatement(stmt, isScript))
                ReportError(ErrorKind.SyntaxError, ErrorMessage.InvalidGlobalStatement, stmt.Span);

            return new GlobalStatementSynatx(stmt, isTreeValid);
        }

        private FunctionDeclarationSyntax ParseFunctionDeclaration()
        {
            var functionKeyword = Advance();
            var identifier = MatchToken(SyntaxTokenKind.Identifier);
            var lparen = MatchToken(SyntaxTokenKind.LParen);

            SeperatedSyntaxList<ParameterSyntax> parameters;
            if (current.Kind == SyntaxTokenKind.RParen)
                parameters = SeperatedSyntaxList<ParameterSyntax>.Empty;
            else
                parameters = ParseSeperatedSyntaxList<ParameterSyntax>(ParseParameter, SyntaxTokenKind.Comma);

            var rparen = MatchToken(SyntaxTokenKind.RParen);
            var returnType = ParseOptionalTypeClause();
            var body = ParseBlockStatement();
            return new FunctionDeclarationSyntax(functionKeyword, identifier, lparen, parameters, rparen, returnType, body, isTreeValid);
        }

        private ParameterSyntax ParseParameter()
        {
            var identifier = MatchToken(SyntaxTokenKind.Identifier);
            var type = ParseOptionalTypeClause();
            return new ParameterSyntax(identifier, type, isTreeValid);
        }

        private StatementSyntax ParseStatement()
        {
            switch (current.Kind)
            {
                case SyntaxTokenKind.LCurly:
                    return ParseBlockStatement();
                case SyntaxTokenKind.IfKeyword:
                    return ParseIfStatement();
                case SyntaxTokenKind.WhileKeyword:
                    return ParseWhileStatement();
                case SyntaxTokenKind.ForKeyword:
                    return ParseForStatement();
                case SyntaxTokenKind.DoKeyword:
                    return ParseDoWhileStatement();
                case SyntaxTokenKind.VarKeyword:
                case SyntaxTokenKind.ConstKeyword:
                    return ParseVariableDeclaration();
                case SyntaxTokenKind.BreakKewyword:
                    return ParseBreakStatement();
                case SyntaxTokenKind.ContinueKeyword:
                    return ParseContinueStatement();
                case SyntaxTokenKind.ReturnKeyword:
                    return ParseReturnStatement();
                default:
                    return ParseExpressionStatement();
            }
        }

        private StatementSyntax ParseReturnStatement()
        {
            var returnKeyword = MatchToken(SyntaxTokenKind.ReturnKeyword);
            if (current.Kind == SyntaxTokenKind.VoidKeyword)
                return new ReturnStatementSyntax(returnKeyword, null, MatchToken(SyntaxTokenKind.VoidKeyword), isTreeValid);
            return new ReturnStatementSyntax(returnKeyword, ParseExpression(), null, isTreeValid);
        }

        private StatementSyntax ParseContinueStatement()
        {
            var continueKewyword = MatchToken(SyntaxTokenKind.ContinueKeyword);
            return new ContinueStatementSyntax(continueKewyword, isTreeValid);
        }

        private StatementSyntax ParseBreakStatement()
        {
            var breakKewyword = MatchToken(SyntaxTokenKind.BreakKewyword);
            return new BreakStatementSyntax(breakKewyword, isTreeValid);
        }

        private DoWhileStatementSyntax ParseDoWhileStatement()
        {
            var doToken = MatchToken(SyntaxTokenKind.DoKeyword);
            var body = ParseStatement();
            var whileToken = MatchToken(SyntaxTokenKind.WhileKeyword);
            var condition = ParseExpression();
            return new DoWhileStatementSyntax(doToken, body, whileToken, condition, isTreeValid);
        }

        private ForStatementSyntax ParseForStatement()
        {
            var forToken = MatchToken(SyntaxTokenKind.ForKeyword);
            var variableDeclaration = ParseVariableDeclaration();
            //var comma1 = MatchToken(SyntaxTokenKind.Comma);
            var condition = ParseExpression();
            //var comma2 = MatchToken(SyntaxTokenKind.Comma);
            var increment = ParseExpression();
            var body = ParseStatement();
            return new ForStatementSyntax(forToken, variableDeclaration, condition, increment, body, isTreeValid);
        }

        private IfStatementSyntax ParseIfStatement()
        {
            var ifToken = MatchToken(SyntaxTokenKind.IfKeyword);
            var condition = ParseExpression();
            var body = ParseStatement();
            var elseClause = ParseElseClause();
            return new IfStatementSyntax(ifToken, condition, body, elseClause, isTreeValid);
        }

        private WhileStatementSyntax ParseWhileStatement()
        {
            var whileToken = MatchToken(SyntaxTokenKind.WhileKeyword);
            var condition = ParseExpression();
            var body = ParseStatement();
            return new WhileStatementSyntax(whileToken, condition, body, isTreeValid);
        }

        private ExpressionStatementSyntax ParseExpressionStatement()
        {
            var expression = ParseExpression();
            if (!SyntaxFacts.IsExpressionStatement(expression, isScript))
                ReportError(ErrorKind.SyntaxError, ErrorMessage.InvalidStatement, expression.Span);
            return new ExpressionStatementSyntax(expression, isTreeValid);
        }

        private BlockStatmentSyntax ParseBlockStatement()
        {
            var lcurly = MatchToken(SyntaxTokenKind.LCurly);

            var builder = ImmutableArray.CreateBuilder<StatementSyntax>();
            while (current.Kind != SyntaxTokenKind.RCurly)
            {
                if (current.Kind == SyntaxTokenKind.End)
                {
                    ReportError(ErrorKind.SyntaxError, ErrorMessage.NeverClosedCurlyBrackets, TextSpan.FromBounds(lcurly.Span.Start, current.Span.Start));
                    break;
                }

                var stmt = ParseStatement();
                builder.Add(stmt);
            }

            var rcurly = MatchToken(SyntaxTokenKind.RCurly);
            return new BlockStatmentSyntax(lcurly, builder.ToImmutable(), rcurly, isTreeValid);
        }

        private VariableDeclarationStatementSyntax ParseVariableDeclaration()
        {
            var declareKeyword = MatchToken(SyntaxTokenKind.VarKeyword, SyntaxTokenKind.ConstKeyword);
            var identifier = MatchToken(SyntaxTokenKind.Identifier);
            var type = ParseOptionalTypeClause();
            var equalToken = MatchToken(SyntaxTokenKind.Equal);
            var expr = ParseExpression();
            return new VariableDeclarationStatementSyntax(declareKeyword, identifier, type, equalToken, expr, isTreeValid);
        }

        private TypeClauseSyntax ParseOptionalTypeClause()
        {
            if (current.Kind == SyntaxTokenKind.Colon && Peak(1).Kind.IsTypeKeyword())
                return ParseTypeClause();

            var colon = new SyntaxToken(SyntaxTokenKind.Colon, current.Span.Start, 0, ':');
            var typeToken = new SyntaxToken(SyntaxTokenKind.AnyKeyword, current.Span.Start, 0, SyntaxTokenKind.AnyKeyword.GetStringRepresentation());
            return new TypeClauseSyntax(colon, typeToken, isExplicit: false, isValid: isTreeValid);
        }

        private TypeClauseSyntax ParseTypeClause()
        {
            var colon = MatchToken(SyntaxTokenKind.Colon);
            var typeToken = MatchToken(SyntaxTokenKind.AnyKeyword, SyntaxFacts.GetTypeKeywords().ToArray());
            return new TypeClauseSyntax(colon, typeToken, isExplicit: true, isTreeValid);
        }

        private ExpressionSyntax ParseExpression() => ParseExpression(SyntaxFacts.MaxPrecedence);

        private ExpressionSyntax ParseExpression(int lvl)
        {
            if (lvl == 0) return ParsePrimaryExpression();

            var left = ParseExpression(lvl - 1);

            while (current.Kind.GetBinaryPrecedence() == lvl)
            {
                var op = Advance();
                var right = ParseExpression(lvl - 1);
                left = new BinaryExpressionSyntax(op, left, right, isTreeValid);
            }

            return left;
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            if (SyntaxFacts.IsLiteralExpression(current.Kind))
                return new LiteralExpressionSyntax(Advance(), isTreeValid);
            else if (current.Kind == SyntaxTokenKind.Identifier)
                return ParseIdentifier();
            else if (current.Kind.IsUnaryOperator())
                return new UnaryExpressionSyntax(Advance(), ParsePrimaryExpression(), isTreeValid);
            else if (current.Kind == SyntaxTokenKind.LParen)
                return ParseParenthesizedExpression();
            else if (SyntaxFacts.IsTypeKeyword(current.Kind))
                return ParseFunctionCall(Advance());
            else
            {
                ReportError(ErrorKind.SyntaxError, ErrorMessage.UnExpectedToken, current.Span, current.Kind);
                return new LiteralExpressionSyntax(Advance(), false);
            }
        }

        private ExpressionSyntax ParseParenthesizedExpression()
        {
            var start = current.Span.Start;
            MatchToken(SyntaxTokenKind.LParen);
            var expr = ParseExpression();
            if (current.Kind != SyntaxTokenKind.RParen)
            {
                ReportError(ErrorKind.SyntaxError, ErrorMessage.NeverClosedParenthesis, TextSpan.FromBounds(start, current.Span.End));
                isTreeValid = false;
            }
            MatchToken(SyntaxTokenKind.RParen);
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
                    return new AssignmentExpressionSyntax(identifier, equalToken, expr1, isTreeValid);
                case SyntaxTokenKind.PlusEqual:
                case SyntaxTokenKind.MinusEqual:
                case SyntaxTokenKind.StarEqual:
                case SyntaxTokenKind.SlashEqual:
                case SyntaxTokenKind.AmpersandEqual:
                case SyntaxTokenKind.PipeEqual:
                    var op1 = Advance();
                    var expr2 = ParseExpression();
                    return new AdditionalAssignmentExpressionSyntax(identifier, op1, expr2, isTreeValid);
                case SyntaxTokenKind.PlusPlus:
                case SyntaxTokenKind.MinusMinus:
                    var op2 = Advance();
                    return new PostIncDecExpressionSyntax(identifier, op2, isTreeValid);
                case SyntaxTokenKind.LParen:
                    return ParseFunctionCall(identifier);
                default:
                    return new VariableExpressionSyntax(identifier, isTreeValid);
            }
        }

        private CallExpressionSyntax ParseFunctionCall(SyntaxToken identifier)
        {
            var lparen = MatchToken(SyntaxTokenKind.LParen);

            SeperatedSyntaxList<ExpressionSyntax> arguments;

            if (current.Kind != SyntaxTokenKind.RParen)
                arguments = ParseSeperatedSyntaxList<ExpressionSyntax>(ParseExpression, SyntaxTokenKind.Comma);
            else
                arguments = SeperatedSyntaxList<ExpressionSyntax>.Empty;
            var rparen = MatchToken(SyntaxTokenKind.RParen);
            return new CallExpressionSyntax(identifier, lparen, arguments, rparen, isTreeValid);
        }

        private ElseStatementSyntax ParseElseClause()
        {
            if (current.Kind != SyntaxTokenKind.ElseKeyword)
                return null;
            var elseKeyword = Advance();
            var statement = ParseStatement();
            return new ElseStatementSyntax(elseKeyword, statement, isTreeValid);
        }

        private delegate T ParseDelegate<T>();

        private SeperatedSyntaxList<T> ParseSeperatedSyntaxList<T>(ParseDelegate<T> function, SyntaxTokenKind seperator) where T : SyntaxNode
        {
            var nodes = ImmutableArray.CreateBuilder<T>();
            var seperators = ImmutableArray.CreateBuilder<SyntaxToken>();

            while (true)
            {
                if (current.Kind == SyntaxTokenKind.End)
                {
                    ReportError(ErrorKind.SyntaxError, ErrorMessage.UnExpectedToken, current.Span, current.Kind);
                    break;
                }

                nodes.Add(function());

                if (current.Kind != seperator)
                    break;

                seperators.Add(MatchToken(seperator));
            }
            return new SeperatedSyntaxList<T>(nodes.ToImmutable(), seperators.ToImmutable(), isTreeValid);
        }
    }
}