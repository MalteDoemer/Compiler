using System.Text;
using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class VariableDeclarationStatement : StatementSyntax
    {
        public VariableDeclarationStatement(SyntaxToken typeToken, SyntaxToken identifier,SyntaxToken equalToken, ExpressionSyntax expression)
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

        public override bool IsValid => TypeToken.IsValid && Identifier.IsValid && EqualToken.IsValid && Expression.IsValid;

        public override string ToString()
        {
            return $"{TypeToken.Value} {Identifier.Value} = {Expression}";
        }
    }
}