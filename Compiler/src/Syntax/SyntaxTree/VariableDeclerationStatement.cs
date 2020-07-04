using System.Text;
using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class VariableDeclarationStatement : StatementSyntax
    {
        public VariableDeclarationStatement(SyntaxToken varKeyword, SyntaxToken identifier, TypeClauseSyntax typeClause , SyntaxToken equalToken, ExpressionSyntax expression, bool isValid = true)
        {
            VarKeyword = varKeyword;
            Identifier = identifier;
            TypeClause = typeClause;
            EqualToken = equalToken;
            Expression = expression;
            IsValid = isValid;
        }

        public override TextSpan Span => VarKeyword.Span + Expression.Span;
        public override bool IsValid { get; }
        public SyntaxToken VarKeyword { get; }
        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax TypeClause { get; }
        public SyntaxToken EqualToken { get; }
        public ExpressionSyntax Expression { get; }

        public override string ToString() => $"{VarKeyword.Value} {Identifier.Value}{TypeClause} = {Expression}";
    }
}