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
        private readonly DiagnosticBag lexerDiagnosics;

        private readonly SourceText source;
        private readonly ImmutableArray<SyntaxToken> tokens;
        private readonly bool isScript;

        private bool isTreeValid = true;
        private bool isStatementValid = true;
        private SyntaxToken current { get => Peak(0); }
        private int pos;

        public Parser(SourceText source, bool isScript)
        {
            this.source = source;
            this.isScript = isScript;
            this.diagnostics = new DiagnosticBag();
            var lexer = new Lexer(source, isScript);
            this.tokens = lexer.Tokenize().ToImmutableArray();
            this.lexerDiagnosics = (DiagnosticBag)lexer.GetDiagnostics();
        }

        public IEnumerable<Diagnostic> GetDiagnostics() => diagnostics.Concat(lexerDiagnosics);

        private void ReportError(ErrorMessage message, TextLocation location, params object[] values)
        {
            if (isStatementValid)
                diagnostics.ReportError(message, location, values);
            isStatementValid = false;
            isTreeValid = false;
        }

        private SyntaxToken MatchToken(SyntaxTokenKind kind, params SyntaxTokenKind[] others)
        {
            if (current.Kind == kind) return Advance();

            foreach (var kind2 in others)
                if (current.Kind == kind2) return Advance();

            ReportError(ErrorMessage.ExpectedToken, current.Location, kind);
            var res = new SyntaxToken(kind, current.Location, current.Value, false);
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
            return new CompilationUnitSyntax(members, isTreeValid, new TextLocation(source, TextSpan.FromBounds(0, source.Length)));
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
                ReportError(ErrorMessage.InvalidGlobalStatement, stmt.Location);

            return new GlobalStatementSynatx(stmt, isTreeValid, stmt.Location);
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

            var start = functionKeyword.Location.Span.Start;
            var end = body.Location.Span.End;
            var span = TextSpan.FromBounds(start, end);

            return new FunctionDeclarationSyntax(functionKeyword, identifier, lparen, parameters, rparen, returnType, body, isTreeValid, new TextLocation(source, span));
        }

        private ParameterSyntax ParseParameter()
        {
            var identifier = MatchToken(SyntaxTokenKind.Identifier);
            var type = ParseOptionalTypeClause();
            return new ParameterSyntax(identifier, type, isTreeValid, new TextLocation(source, TextSpan.FromBounds(identifier.Location.Span.Start, type.Location.Span.End)));
        }

        private StatementSyntax ParseStatement()
        {
            isStatementValid = true;
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
            {
                var voidToken = MatchToken(SyntaxTokenKind.VoidKeyword);
                var span = TextSpan.FromBounds(returnKeyword.Location.Span.Start, voidToken.Location.Span.End);
                return new ReturnStatementSyntax(returnKeyword, null, voidToken, isTreeValid, new TextLocation(source, span));
            }
            var expr = ParseExpression();
            var span2 = TextSpan.FromBounds(returnKeyword.Location.Span.Start, expr.Location.Span.End);
            return new ReturnStatementSyntax(returnKeyword, expr, null, isTreeValid, new TextLocation(source, span2));
        }

        private StatementSyntax ParseContinueStatement()
        {
            var continueKewyword = MatchToken(SyntaxTokenKind.ContinueKeyword);
            return new ContinueStatementSyntax(continueKewyword, isTreeValid, continueKewyword.Location);
        }

        private StatementSyntax ParseBreakStatement()
        {
            var breakKewyword = MatchToken(SyntaxTokenKind.BreakKewyword);
            return new BreakStatementSyntax(breakKewyword, isTreeValid, breakKewyword.Location);
        }

        private DoWhileStatementSyntax ParseDoWhileStatement()
        {
            var doToken = MatchToken(SyntaxTokenKind.DoKeyword);
            var body = ParseStatement();
            var whileToken = MatchToken(SyntaxTokenKind.WhileKeyword);
            var condition = ParseExpression();
            var span = TextSpan.FromBounds(doToken.Location.Span.Start, condition.Location.Span.End);
            return new DoWhileStatementSyntax(doToken, body, whileToken, condition, isTreeValid, new TextLocation(source, span));
        }

        private ForStatementSyntax ParseForStatement()
        {
            var forToken = MatchToken(SyntaxTokenKind.ForKeyword);
            var variableDeclaration = ParseVariableDeclaration();
            var condition = ParseExpression();
            var increment = ParseExpression();
            var body = ParseStatement();
            var span = TextSpan.FromBounds(forToken.Location.Span.Start, body.Location.Span.End);
            return new ForStatementSyntax(forToken, variableDeclaration, condition, increment, body, isTreeValid, new TextLocation(source, span));
        }

        private IfStatementSyntax ParseIfStatement()
        {
            var ifToken = MatchToken(SyntaxTokenKind.IfKeyword);
            var condition = ParseExpression();
            var body = ParseStatement();
            var elseClause = ParseElseClause();
            var endStmt = elseClause ?? body;

            var span = TextSpan.FromBounds(ifToken.Location.Span.Start, endStmt.Location.Span.End);
            return new IfStatementSyntax(ifToken, condition, body, elseClause, isTreeValid, new TextLocation(source, span));
        }

        private WhileStatementSyntax ParseWhileStatement()
        {
            var whileToken = MatchToken(SyntaxTokenKind.WhileKeyword);
            var condition = ParseExpression();
            var body = ParseStatement();
            var span = TextSpan.FromBounds(whileToken.Location.Span.Start, body.Location.Span.End);
            return new WhileStatementSyntax(whileToken, condition, body, isTreeValid, new TextLocation(source, span));
        }

        private ExpressionStatementSyntax ParseExpressionStatement()
        {
            var expression = ParseExpression();
            if (!SyntaxFacts.IsExpressionStatement(expression, isScript))
                ReportError(ErrorMessage.InvalidStatement, expression.Location);
            return new ExpressionStatementSyntax(expression, isTreeValid, expression.Location);
        }

        private BlockStatmentSyntax ParseBlockStatement()
        {
            var lcurly = MatchToken(SyntaxTokenKind.LCurly);

            var builder = ImmutableArray.CreateBuilder<StatementSyntax>();
            while (current.Kind != SyntaxTokenKind.RCurly)
            {
                if (current.Kind == SyntaxTokenKind.End)
                {
                    ReportError(ErrorMessage.NeverClosedCurlyBrackets, new TextLocation(source, TextSpan.FromBounds(lcurly.Location.Span.Start, current.Location.Span.Start)));
                    break;
                }

                var stmt = ParseStatement();
                builder.Add(stmt);
            }

            var rcurly = MatchToken(SyntaxTokenKind.RCurly);
            var span = TextSpan.FromBounds(lcurly.Location.Span.Start, rcurly.Location.Span.End);
            return new BlockStatmentSyntax(lcurly, builder.ToImmutable(), rcurly, isTreeValid, new TextLocation(source, span));
        }

        private VariableDeclarationStatementSyntax ParseVariableDeclaration()
        {
            var declareKeyword = MatchToken(SyntaxTokenKind.VarKeyword, SyntaxTokenKind.ConstKeyword);
            var identifier = MatchToken(SyntaxTokenKind.Identifier);
            var type = ParseOptionalTypeClause();
            var equalToken = MatchToken(SyntaxTokenKind.Equal);
            var expr = ParseExpression();
            var span = TextSpan.FromBounds(declareKeyword.Location.Span.Start, expr.Location.Span.End);
            return new VariableDeclarationStatementSyntax(declareKeyword, identifier, type, equalToken, expr, isTreeValid, new TextLocation(source, span));
        }

        private TypeClauseSyntax ParseOptionalTypeClause()
        {
            if (current.Kind == SyntaxTokenKind.Colon && Peak(1).Kind.IsTypeKeyword())
                return ParseTypeClause();

            var loc = new TextLocation(source, current.Location.Span.Start, 0);
            var colon = new SyntaxToken(SyntaxTokenKind.Colon, loc, ':');
            var typeToken = new SyntaxToken(SyntaxTokenKind.AnyKeyword, loc, SyntaxTokenKind.AnyKeyword.GetStringRepresentation());
            return new TypeClauseSyntax(colon, typeToken, isExplicit: false, isValid: isTreeValid, loc);
        }

        private TypeClauseSyntax ParseTypeClause()
        {
            var colon = MatchToken(SyntaxTokenKind.Colon);
            var typeToken = MatchToken(SyntaxTokenKind.AnyKeyword, SyntaxFacts.GetTypeKeywords().ToArray());
            var span = TextSpan.FromBounds(colon.Location.Span.Start, typeToken.Location.Span.End);
            return new TypeClauseSyntax(colon, typeToken, isExplicit: true, isTreeValid, new TextLocation(source, span));
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
                var span = TextSpan.FromBounds(left.Location.Span.Start, right.Location.Span.End);
                left = new BinaryExpressionSyntax(op, left, right, isTreeValid, new TextLocation(source, span));
            }

            return left;
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            if (SyntaxFacts.IsLiteralExpression(current.Kind))
            {
                var literal = Advance();
                return new LiteralExpressionSyntax(literal, isTreeValid, literal.Location);
            }
            else if (current.Kind == SyntaxTokenKind.Identifier)
                return ParseIdentifier();
            else if (current.Kind.IsUnaryOperator())
            {
                var op = Advance();
                var expr = ParsePrimaryExpression();
                var span = TextSpan.FromBounds(op.Location.Span.Start, expr.Location.Span.End);
                return new UnaryExpressionSyntax(op, expr, isTreeValid, new TextLocation(source, span));
            }
            else if (current.Kind == SyntaxTokenKind.LParen)
                return ParseParenthesizedExpression();
            else if (SyntaxFacts.IsTypeKeyword(current.Kind))
                return ParseFunctionCall(Advance());
            else if (current.Kind == SyntaxTokenKind.InterpolatedString)
                return ParseInterpolatedString();
            else
            {
                var token = Advance();
                ReportError(ErrorMessage.UnexpectedToken, token.Location, token.Kind);
                return new LiteralExpressionSyntax(token, false, token.Location);
            }
        }

        private ExpressionSyntax ParseInterpolatedString()
        {
            var interpolated = (InterpolatedString)current.Value;
            var strings = interpolated.Text.Split("{}");
            var builder = ImmutableArray.CreateBuilder<ExpressionSyntax>();

            Console.WriteLine(string.Join(", ", strings));

            foreach (var section in interpolated.Sections)
            {

            }

            return new LiteralExpressionSyntax(new SyntaxToken(SyntaxTokenKind.String, current.Location, "yeet"), true, current.Location);
        }

        private ExpressionSyntax ParseParenthesizedExpression()
        {
            var start = current.Location.Span.Start;
            MatchToken(SyntaxTokenKind.LParen);
            var expr = ParseExpression();
            if (current.Kind != SyntaxTokenKind.RParen)
                ReportError(ErrorMessage.NeverClosedParenthesis, new TextLocation(source, TextSpan.FromBounds(start, current.Location.Span.End)));
            MatchToken(SyntaxTokenKind.RParen);
            return expr;
        }

        private ExpressionSyntax ParseIdentifier()
        {
            var identifier = MatchToken(SyntaxTokenKind.Identifier);
            var start = identifier.Location.Span.Start;

            switch (current.Kind)
            {
                case SyntaxTokenKind.Equal:
                    var equalToken = Advance();
                    var expr1 = ParseExpression();
                    var span1 = TextSpan.FromBounds(start, expr1.Location.Span.End);
                    return new AssignmentExpressionSyntax(identifier, equalToken, expr1, isTreeValid, new TextLocation(source, span1));
                case SyntaxTokenKind.PlusEqual:
                case SyntaxTokenKind.MinusEqual:
                case SyntaxTokenKind.StarEqual:
                case SyntaxTokenKind.SlashEqual:
                case SyntaxTokenKind.AmpersandEqual:
                case SyntaxTokenKind.PipeEqual:
                    var op1 = Advance();
                    var expr2 = ParseExpression();
                    var span2 = TextSpan.FromBounds(start, expr2.Location.Span.End);
                    return new AdditionalAssignmentExpressionSyntax(identifier, op1, expr2, isTreeValid, new TextLocation(source, span2));
                case SyntaxTokenKind.PlusPlus:
                case SyntaxTokenKind.MinusMinus:
                    var op2 = Advance();
                    var span3 = TextSpan.FromBounds(start, op2.Location.Span.End);
                    return new PostIncDecExpressionSyntax(identifier, op2, isTreeValid, new TextLocation(source, span3));
                case SyntaxTokenKind.LParen:
                    return ParseFunctionCall(identifier);
                default:
                    return new VariableExpressionSyntax(identifier, isTreeValid, identifier.Location);
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

            var span = TextSpan.FromBounds(identifier.Location.Span.Start, rparen.Location.Span.End);
            return new CallExpressionSyntax(identifier, lparen, arguments, rparen, isTreeValid, new TextLocation(source, span));
        }

        private ElseStatementSyntax ParseElseClause()
        {
            if (current.Kind != SyntaxTokenKind.ElseKeyword)
                return null;
            var elseKeyword = Advance();
            var statement = ParseStatement();
            var span = TextSpan.FromBounds(elseKeyword.Location.Span.Start, statement.Location.Span.End);
            return new ElseStatementSyntax(elseKeyword, statement, isTreeValid, new TextLocation(source, span));
        }

        private delegate T ParseDelegate<T>() where T : SyntaxNode;

        private SeperatedSyntaxList<T> ParseSeperatedSyntaxList<T>(ParseDelegate<T> function, SyntaxTokenKind seperator) where T : SyntaxNode
        {
            var nodes = ImmutableArray.CreateBuilder<T>();
            var seperators = ImmutableArray.CreateBuilder<SyntaxToken>();

            while (true)
            {
                if (current.Kind == SyntaxTokenKind.End)
                {
                    ReportError(ErrorMessage.UnexpectedToken, current.Location, current.Kind);
                    break;
                }

                nodes.Add(function());

                if (current.Kind != seperator)
                    break;

                seperators.Add(MatchToken(seperator));
            }
            TextSpan span;

            if (nodes.Count == 0)
                span = TextSpan.Undefined;
            else if (nodes.Count == 1)
                span = nodes[0].Location.Span;
            else
            {
                var start = nodes.First().Location.Span.Start;
                var end = nodes.Last().Location.Span.End;
                span = TextSpan.FromBounds(start, end);
            }

            return new SeperatedSyntaxList<T>(nodes.ToImmutable(), seperators.ToImmutable(), isTreeValid, new TextLocation(source, span));
        }
    }
}