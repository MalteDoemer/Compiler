using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Compiler.Diagnostics;

namespace Compiler.Syntax
{

    internal abstract class SyntaxNode
    {
        public abstract int Pos { get; }
    }


    internal abstract class ExpressionSyntax : SyntaxNode
    {

    }

    internal sealed class InvalidExpressionSyntax : ExpressionSyntax
    {
        public InvalidExpressionSyntax(SyntaxToken invalidToken)
        {
            InvalidToken = invalidToken;
        }

        public override int Pos => InvalidToken.pos;

        public SyntaxToken InvalidToken { get; }
    }

    internal sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        public LiteralExpressionSyntax(SyntaxToken literal)
        {
            Literal = literal;
        }

        public SyntaxToken Literal { get; }
        public override int Pos => Literal.pos;
    }

    internal sealed class VariableExpressionSyntax : ExpressionSyntax
    {

        public VariableExpressionSyntax(SyntaxToken name)
        {
            Name = name;
        }

        public override int Pos => Name.pos;

        public SyntaxToken Name { get; }
    }

    internal sealed class UnaryExpressionSyntax : ExpressionSyntax
    {
        public UnaryExpressionSyntax(SyntaxToken op, ExpressionSyntax expression)
        {
            Op = op;
            Expression = expression;
        }

        public override int Pos => Op.pos;

        public SyntaxToken Op { get; }
        public ExpressionSyntax Expression { get; }
    }

    internal sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        public BinaryExpressionSyntax(SyntaxToken op, ExpressionSyntax left, ExpressionSyntax right)
        {
            Op = op;
            Left = left;
            Right = right;
        }

        public SyntaxToken Op { get; }
        public ExpressionSyntax Left { get; }
        public ExpressionSyntax Right { get; }

        public override int Pos => Op.pos;
    }

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
            if (kind == current.kind) return Advance();
            else
            {
                diagnostics.ReportUnexpectedToken(current, kind);
                var res = new SyntaxToken(kind, current.pos, current.value);
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

            while (current.kind.GetBinaryPrecedence() == lvl)
            {
                var op = Advance();
                var right = ParseExpression(lvl - 1);
                left = new BinaryExpressionSyntax(op, left, right);
            }

            return left;

        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            if (current.kind.IsLiteralExpression())
                return new LiteralExpressionSyntax(Advance());
            else if (current.kind == SyntaxTokenKind.Identifier)
                return ParseIdentifier();
            else if (current.kind.IsUnaryOperator()) 
                return new UnaryExpressionSyntax(Advance(), ParsePrimaryExpression());
            else if (current.kind == SyntaxTokenKind.LParen)
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