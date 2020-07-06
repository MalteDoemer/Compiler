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
            diagnostics = new DiagnosticBag();
        }

        public IEnumerable<Diagnostic> GetDiagnostics() => diagnostics;

        private SyntaxToken MatchToken(SyntaxTokenKind kind, params SyntaxTokenKind[] others)
        {
            if (current.Kind == kind) return Advance();

            foreach (var kind2 in others)
                if (current.Kind == kind2) return Advance();


            if (isTreeValid)
                diagnostics.ReportSyntaxError(ErrorMessage.ExpectedToken, current.Span, kind);
            var res = new SyntaxToken(kind, current.Span.Start, current.Span.Length, current.Value, false);
            pos++;
            isTreeValid = false;
            return res;
        }

        // private SyntaxToken MatchToken(SyntaxTokenKind kind)
        // {
        //     if (kind == current.Kind) return Advance();
        //     else
        //     {
        //         if (isTreeValid)
        //             diagnostics.ReportSyntaxError(ErrorMessage.ExpectedToken, current.Span, kind);

        //         var res = new SyntaxToken(kind, current.Span.Start, current.Span.Length, current.Value, false);
        //         pos++;

        //         isTreeValid = false;
        //         return res;
        //     }
        // }

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
            return new CompilationUnitSyntax(TextSpan.FromBounds(0, source.Length), members, isTreeValid);
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
            {
                if (isTreeValid)
                    diagnostics.ReportSyntaxError(ErrorMessage.InvalidGlobalStatement, stmt.Span);
                isTreeValid = false;
            }

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
                default:
                    return ParseExpressionStatement();
            }
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
            var comma1 = MatchToken(SyntaxTokenKind.Comma);
            var condition = ParseExpression();
            var comma2 = MatchToken(SyntaxTokenKind.Comma);
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

        private ExpressionStatement ParseExpressionStatement()
        {
            var expression = ParseExpression();
            if (!SyntaxFacts.IsExpressionStatement(expression, isScript))
            {
                if (isTreeValid)
                    diagnostics.ReportSyntaxError(ErrorMessage.InvalidStatement, expression.Span);
                isTreeValid = false;
            }
            return new ExpressionStatement(expression, isTreeValid);
        }

        private BlockStatmentSyntax ParseBlockStatement()
        {
            var lcurly = MatchToken(SyntaxTokenKind.LCurly);

            var builder = ImmutableArray.CreateBuilder<StatementSyntax>();
            while (current.Kind != SyntaxTokenKind.RCurly)
            {
                if (current.Kind == SyntaxTokenKind.End)
                {
                    if (isTreeValid)
                    {
                        var span = TextSpan.FromBounds(lcurly.Span.Start, current.Span.Start);
                        diagnostics.ReportSyntaxError(ErrorMessage.NeverClosedCurlyBrackets, span);
                    }
                    isTreeValid = false;
                    break;
                }

                var stmt = ParseStatement();
                builder.Add(stmt);
            }

            var rcurly = MatchToken(SyntaxTokenKind.RCurly);
            return new BlockStatmentSyntax(lcurly, builder.ToImmutable(), rcurly, isTreeValid);
        }

        private VariableDeclarationStatement ParseVariableDeclaration()
        {
            var declareKeyword = MatchToken(SyntaxTokenKind.VarKeyword, SyntaxTokenKind.ConstKeyword);
            var identifier = MatchToken(SyntaxTokenKind.Identifier);
            var type = ParseOptionalTypeClause();
            var equalToken = MatchToken(SyntaxTokenKind.Equal);
            var expr = ParseExpression();
            return new VariableDeclarationStatement(declareKeyword, identifier, type, equalToken, expr, isTreeValid);
        }

        private TypeClauseSyntax ParseOptionalTypeClause()
        {
            if (current.Kind == SyntaxTokenKind.Colon && Peak(1).Kind.IsTypeKeyword())
                return ParseTypeClause();

            var colon = new SyntaxToken(SyntaxTokenKind.Colon, current.Span.Start, 0, ':');
            var typeToken = new SyntaxToken(SyntaxTokenKind.AnyKeyword, current.Span.Start, 0, SyntaxTokenKind.AnyKeyword.GetStringRepresentation());
            return new TypeClauseSyntax(colon, typeToken, isExplicit: false , isValid: isTreeValid);
        }

        private TypeClauseSyntax ParseTypeClause()
        {
            var colon = MatchToken(SyntaxTokenKind.Colon);
            var typeToken = MatchToken(SyntaxTokenKind.AnyKeyword, SyntaxFacts.GetTypeKeywords().ToArray());
            return new TypeClauseSyntax(colon, typeToken,isExplicit: true ,isTreeValid);
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
                if (isTreeValid)
                    diagnostics.ReportSyntaxError(ErrorMessage.UnExpectedToken, current.Span, current.Kind);

                isTreeValid = false;
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
                if (isTreeValid)
                    diagnostics.ReportSyntaxError(ErrorMessage.NeverClosedParenthesis, TextSpan.FromBounds(start, current.Span.End));
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
                    return new AdditionalAssignmentExpression(identifier, op1, expr2, isTreeValid);
                case SyntaxTokenKind.PlusPlus:
                case SyntaxTokenKind.MinusMinus:
                    var op2 = Advance();
                    return new PostIncDecExpression(identifier, op2, isTreeValid);
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
                    if (isTreeValid)
                        diagnostics.ReportSyntaxError(ErrorMessage.UnExpectedToken, current.Span, current.Kind);

                    isTreeValid = false;
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