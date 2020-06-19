using System.Linq;
using Compiler.Diagnostics;

namespace Compiler.Syntax
{

    internal abstract class SyntaxTreeNode
    {
        public abstract int pos { get; }
    }

    internal abstract class SyntaxExpression : SyntaxTreeNode
    {

    }

    internal sealed class InvalidExpression : SyntaxExpression
    {
        public InvalidExpression(SyntaxToken invalidToken)
        {
            InvalidToken = invalidToken;
        }

        public override int pos => InvalidToken.pos;

        public SyntaxToken InvalidToken { get; }
    }

    internal sealed class LiteralExpression : SyntaxExpression
    {
        public LiteralExpression(SyntaxToken literal)
        {
            Literal = literal;
        }

        public SyntaxToken Literal { get; }
        public override int pos => Literal.pos;
    }

    internal sealed class VariableExpression : SyntaxExpression
    {

        public VariableExpression(SyntaxToken name)
        {
            Name = name;
        }

        public override int pos => Name.pos;

        public SyntaxToken Name { get; }
    }

    internal sealed class UnaryExpression : SyntaxExpression
    {
        public UnaryExpression(SyntaxToken op, SyntaxExpression expression)
        {
            Op = op;
            Expression = expression;
        }

        public override int pos => Op.pos;

        public SyntaxToken Op { get; }
        public SyntaxExpression Expression { get; }
    }

    internal sealed class BinaryExpression : SyntaxExpression
    {
        public BinaryExpression(SyntaxToken op, SyntaxExpression left, SyntaxExpression right)
        {
            Op = op;
            Left = left;
            Right = right;
        }

        public SyntaxToken Op { get; }
        public SyntaxExpression Left { get; }
        public SyntaxExpression Right { get; }

        public override int pos => Op.pos;
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


        public SyntaxExpression ParseBinaryExpression(int lvl = SyntaxFacts.MaxPrecedence)
        {
            if (lvl == 0) return ParsePrimaryExpression();

            var left = ParseBinaryExpression(lvl - 1);

            while (current.kind.GetBinaryPrecedence() == lvl)
            {
                var op = Advance();
                var right = ParseBinaryExpression(lvl - 1);
                left = new BinaryExpression(op, left, right);
            }

            return left;

        }

        private SyntaxExpression ParsePrimaryExpression()
        {
            if (current.kind.IsLiteralExpression())
                return new LiteralExpression(Advance());
            else if (current.kind == SyntaxTokenKind.Identifier)
                return ParseIdentifier();
            else if (current.kind.IsUnaryOperator()) 
                return new UnaryExpression(Advance(), ParsePrimaryExpression());
            else if (current.kind == SyntaxTokenKind.LParen)
                return ParseParenthesizedExpression();
            else 
            {
                diagnostics.ReportUnexpectedToken(current);
                return new InvalidExpression(Advance());
            }
        }
        private SyntaxExpression ParseParenthesizedExpression()
        {
            pos++;
            var expr = ParseBinaryExpression();
            MatchToken(SyntaxTokenKind.RParen);
            return expr;
        }

        private SyntaxExpression ParseIdentifier()
        {
            return new VariableExpression(Advance());
        }
    }
}