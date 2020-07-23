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
        private SyntaxToken current { get => Peek(0); }
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

        private TextLocation LocFromBounds(int start, int end) => new TextLocation(source, TextSpan.FromBounds(start, end));

        private TextLocation LocFromLength(int start, int length) => new TextLocation(source, TextSpan.FromLength(start, length));

        private SyntaxToken MatchToken(params SyntaxTokenKind[] kinds)
        {
            foreach (var kind in kinds)
                if (current.TokenKind == kind) return Advance();

            ReportError(ErrorMessage.ExpectedToken, current.Location, string.Join("/", kinds));
            var res = new SyntaxToken(kinds.FirstOrDefault(), current.Location, current.Value, false);
            pos++;
            return res;
        }

        private SyntaxToken Advance()
        {
            var res = current;
            pos++;
            return res;
        }

        private SyntaxToken Peek(int offset)
        {
            var index = pos + offset;
            if (index < tokens.Length)
                return tokens[index];
            return tokens[tokens.Length - 1];
        }

        public CompilationUnitSyntax ParseCompilationUnit()
        {
            var members = ParseMembers();
            return new CompilationUnitSyntax(members, isTreeValid, LocFromBounds(0, source.Length));
        }

        private ImmutableArray<MemberSyntax> ParseMembers()
        {
            var builder = ImmutableArray.CreateBuilder<MemberSyntax>();

            while (current.TokenKind != SyntaxTokenKind.EndOfFile)
            {
                var member = ParseMember();
                builder.Add(member);
            }
            return builder.ToImmutable();
        }

        private MemberSyntax ParseMember()
        {
            if (current.TokenKind == SyntaxTokenKind.FuncKeyword)
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
            if (current.TokenKind == SyntaxTokenKind.RParen)
                parameters = SeperatedSyntaxList<ParameterSyntax>.Empty;
            else
                parameters = ParseSeperatedSyntaxList<ParameterSyntax>(ParseParameter, SyntaxTokenKind.Comma);

            var rparen = MatchToken(SyntaxTokenKind.RParen);
            var returnType = ParseOptionalTypeClause();
            var body = ParseBlockStatement();

            var start = functionKeyword.Location.Span.Start;
            var end = body.Location.Span.End;
            var loc = LocFromBounds(start, end);
            return new FunctionDeclarationSyntax(functionKeyword, identifier, lparen, parameters, rparen, returnType, body, isTreeValid, loc);
        }

        private ParameterSyntax ParseParameter()
        {
            var identifier = MatchToken(SyntaxTokenKind.Identifier);
            var type = ParseOptionalTypeClause();
            var loc = LocFromBounds(identifier.Location.Span.Start, type.Location.Span.End);
            return new ParameterSyntax(identifier, type, isTreeValid, loc);
        }

        private StatementSyntax ParseStatement()
        {
            isStatementValid = true;
            switch (current.TokenKind)
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
                case SyntaxTokenKind.LetKeyword:
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
            if (current.TokenKind == SyntaxTokenKind.VoidKeyword)
            {
                var voidToken = MatchToken(SyntaxTokenKind.VoidKeyword);
                return new ReturnStatementSyntax(returnKeyword, null, voidToken, isTreeValid, LocFromBounds(returnKeyword.Location.Start, voidToken.Location.End));
            }
            var expr = ParseExpression();
            var span2 = TextSpan.FromBounds(returnKeyword.Location.Span.Start, expr.Location.Span.End);
            return new ReturnStatementSyntax(returnKeyword, expr, null, isTreeValid, LocFromBounds(returnKeyword.Location.Start, expr.Location.End));
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
            var loc = LocFromBounds(doToken.Location.Start, condition.Location.End);
            return new DoWhileStatementSyntax(doToken, body, whileToken, condition, isTreeValid, loc);
        }

        private ForStatementSyntax ParseForStatement()
        {
            var forToken = MatchToken(SyntaxTokenKind.ForKeyword);
            var variableDeclaration = ParseVariableDeclaration();
            var condition = ParseExpression();
            var increment = ParseExpression();
            var body = ParseStatement();
            var loc = LocFromBounds(forToken.Location.Start, body.Location.End);
            return new ForStatementSyntax(forToken, variableDeclaration, condition, increment, body, isTreeValid, loc);
        }

        private IfStatementSyntax ParseIfStatement()
        {
            var ifToken = MatchToken(SyntaxTokenKind.IfKeyword);
            var condition = ParseExpression();
            var body = ParseStatement();
            var elseClause = ParseElseClause();
            var last = elseClause ?? body;
            var loc = LocFromBounds(ifToken.Location.Start, last.Location.End);
            return new IfStatementSyntax(ifToken, condition, body, elseClause, isTreeValid, loc);
        }

        private WhileStatementSyntax ParseWhileStatement()
        {
            var whileToken = MatchToken(SyntaxTokenKind.WhileKeyword);
            var condition = ParseExpression();
            var body = ParseStatement();
            var loc = LocFromBounds(whileToken.Location.Start, body.Location.End);
            return new WhileStatementSyntax(whileToken, condition, body, isTreeValid, loc);
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
            while (current.TokenKind != SyntaxTokenKind.RCurly)
            {
                if (current.TokenKind == SyntaxTokenKind.EndOfFile)
                {
                    ReportError(ErrorMessage.NeverClosedCurlyBrackets, LocFromBounds(lcurly.Location.Start, current.Location.Start));
                    break;
                }

                var stmt = ParseStatement();
                builder.Add(stmt);
            }

            var rcurly = MatchToken(SyntaxTokenKind.RCurly);
            var loc = LocFromBounds(lcurly.Location.Start, lcurly.Location.End);
            return new BlockStatmentSyntax(lcurly, builder.ToImmutable(), rcurly, isTreeValid, loc);
        }

        private VariableDeclarationStatementSyntax ParseVariableDeclaration()
        {
            var declareKeyword = MatchToken(SyntaxTokenKind.VarKeyword, SyntaxTokenKind.LetKeyword);
            var identifier = MatchToken(SyntaxTokenKind.Identifier);
            var type = ParseOptionalTypeClause();
            var equalToken = MatchToken(SyntaxTokenKind.Equal);
            var expr = ParseExpression();
            var loc = LocFromBounds(declareKeyword.Location.Start, expr.Location.End);
            return new VariableDeclarationStatementSyntax(declareKeyword, identifier, type, equalToken, expr, isTreeValid, loc);
        }

        private TypeClauseSyntax ParseOptionalTypeClause()
        {
            if (current.TokenKind == SyntaxTokenKind.Colon)
                return ParseTypeClause();

            var loc = LocFromLength(current.Location.Span.Start, 0);
            var colon = new SyntaxToken(SyntaxTokenKind.Colon, loc, ':');
            var typeToken = new SyntaxToken(SyntaxTokenKind.ObjKeyword, loc, SyntaxTokenKind.ObjKeyword.GetText()!);
            return new TypeClauseSyntax(colon, new PreDefinedTypeSyntax(typeToken, isTreeValid, typeToken.Location), false, isTreeValid, loc);
        }

        private TypeClauseSyntax ParseTypeClause()
        {
            var colon = MatchToken(SyntaxTokenKind.Colon);
            var typeToken = MatchToken(SyntaxTokenKind.ObjKeyword,
                                       SyntaxTokenKind.IntKeyword,
                                       SyntaxTokenKind.FloatKeyword,
                                       SyntaxTokenKind.BoolKeyword,
                                       SyntaxTokenKind.StringKeyword,
                                       SyntaxTokenKind.VoidKeyword);

            var typeSyntax = new PreDefinedTypeSyntax(typeToken, isTreeValid, typeToken.Location);
            var loc = LocFromBounds(colon.Location.Start, typeToken.Location.End);
            return new TypeClauseSyntax(colon, typeSyntax, true, isTreeValid, loc);
        }

        private ExpressionSyntax ParseExpression() => ParseExpression(SyntaxFacts.MaxPrecedence);

        private ExpressionSyntax ParseExpression(int lvl)
        {
            if (lvl == 0) return ParsePrimaryExpression();

            var left = ParseExpression(lvl - 1);

            while (current.TokenKind.GetBinaryPrecedence() == lvl)
            {
                var op = Advance();
                var right = ParseExpression(lvl - 1);
                var loc = LocFromBounds(left.Location.Start, right.Location.End);
                left = new BinaryExpressionSyntax(op, left, right, isTreeValid, loc);
            }

            return left;
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            if (SyntaxFacts.IsLiteralExpression(current.TokenKind))
            {
                var literal = Advance();
                return new LiteralExpressionSyntax(literal, isTreeValid, literal.Location);
            }
            else if (current.TokenKind == SyntaxTokenKind.Identifier)
                return ParseIdentifier();
            else if (current.TokenKind.IsUnaryOperator())
            {
                var op = Advance();
                var expr = ParsePrimaryExpression();
                var loc = LocFromBounds(op.Location.Start, expr.Location.End);
                return new UnaryExpressionSyntax(op, expr, isTreeValid, loc);
            }
            else if (current.TokenKind == SyntaxTokenKind.LParen)
                return ParseParenthesizedExpression();
            else if (SyntaxFacts.IsTypeKeyword(current.TokenKind))
                return ParseFunctionCall(Advance());
            else
            {
                var token = Advance();
                ReportError(ErrorMessage.UnexpectedToken, token.Location, token.TokenKind);
                return new LiteralExpressionSyntax(token, false, token.Location);
            }
        }

        private ExpressionSyntax ParseParenthesizedExpression()
        {
            var start = current.Location.Span.Start;
            var lparen = MatchToken(SyntaxTokenKind.LParen);
            var expr = ParseExpression();
            if (current.TokenKind != SyntaxTokenKind.RParen)
                ReportError(ErrorMessage.NeverClosedParenthesis, LocFromBounds(start, current.Location.Span.End));
            var rparen = MatchToken(SyntaxTokenKind.RParen);
            var loc = LocFromBounds(start, current.Location.End);
            return new ParenthesizedExpression(lparen, expr, rparen, isStatementValid, loc);
        }

        private ExpressionSyntax ParseIdentifier()
        {
            var identifier = MatchToken(SyntaxTokenKind.Identifier);
            var start = identifier.Location.Start;

            switch (current.TokenKind)
            {
                case SyntaxTokenKind.Equal:
                    var equalToken = Advance();
                    var expr1 = ParseExpression();
                    var loc1 = LocFromBounds(start, expr1.Location.End);
                    return new AssignmentExpressionSyntax(identifier, equalToken, expr1, isTreeValid, loc1);
                case SyntaxTokenKind.PlusEqual:
                case SyntaxTokenKind.MinusEqual:
                case SyntaxTokenKind.StarEqual:
                case SyntaxTokenKind.SlashEqual:
                case SyntaxTokenKind.AmpersandEqual:
                case SyntaxTokenKind.PipeEqual:
                    var op1 = Advance();
                    var expr2 = ParseExpression();
                    var loc2 = LocFromBounds(start, expr2.Location.End);
                    return new AdditionalAssignmentExpressionSyntax(identifier, op1, expr2, isTreeValid, loc2);
                case SyntaxTokenKind.PlusPlus:
                case SyntaxTokenKind.MinusMinus:
                    var op2 = Advance();
                    var loc3 = LocFromBounds(start, op2.Location.End);
                    return new PostIncDecExpressionSyntax(identifier, op2, isTreeValid, loc3);
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

            if (current.TokenKind != SyntaxTokenKind.RParen)
                arguments = ParseSeperatedSyntaxList<ExpressionSyntax>(ParseExpression, SyntaxTokenKind.Comma);
            else
                arguments = SeperatedSyntaxList<ExpressionSyntax>.Empty;
            var rparen = MatchToken(SyntaxTokenKind.RParen);

            var loc = LocFromBounds(identifier.Location.Start, rparen.Location.End);
            return new CallExpressionSyntax(identifier, lparen, arguments, rparen, isTreeValid, loc);
        }

        private ElseStatementSyntax? ParseElseClause()
        {
            if (current.TokenKind != SyntaxTokenKind.ElseKeyword)
                return null;
            var elseKeyword = Advance();
            var statement = ParseStatement();
            var loc = LocFromBounds(elseKeyword.Location.Start, statement.Location.End);
            return new ElseStatementSyntax(elseKeyword, statement, isTreeValid, loc);
        }

        private delegate T ParseDelegate<T>() where T : SyntaxNode;

        private SeperatedSyntaxList<T> ParseSeperatedSyntaxList<T>(ParseDelegate<T> function, SyntaxTokenKind seperator) where T : SyntaxNode
        {
            var nodes = ImmutableArray.CreateBuilder<T>();
            var seperators = ImmutableArray.CreateBuilder<SyntaxToken>();

            while (true)
            {
                if (current.TokenKind == SyntaxTokenKind.EndOfFile)
                {
                    ReportError(ErrorMessage.UnexpectedToken, current.Location, current.TokenKind);
                    break;
                }

                nodes.Add(function());

                if (current.TokenKind != seperator)
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