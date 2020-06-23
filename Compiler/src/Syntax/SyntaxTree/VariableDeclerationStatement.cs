using System.Text;
using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class VariableDeclerationStatement : StatementSyntax
    {
        public VariableDeclerationStatement(SyntaxToken typeToken, SyntaxToken identifier,SyntaxToken equalToken, ExpressionSyntax expression)
        {
            TypeToken = typeToken;
            Identifier = identifier;
            EqualToken = equalToken;
            Expression = expression;
        }

        public override TextSpan Span => TypeToken.Span + Expression.Span;

        public SyntaxToken TypeToken { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken EqualToken { get; }
        public ExpressionSyntax Expression { get; }

        public override string ToString()
        {
            return $"{TypeToken.Value} {Identifier.Value} = {Expression}";
        }
    }
}