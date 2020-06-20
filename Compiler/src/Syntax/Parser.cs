using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Compiler.Diagnostics;

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

        public Parser(string text, DiagnosticBag diagnostics)
        {
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

        public ExpressionSyntax ParseExpression(int lvl = SyntaxFacts.MaxPrecedence)
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
            return new VariableExpressionSyntax(Advance());
        }
    }
}