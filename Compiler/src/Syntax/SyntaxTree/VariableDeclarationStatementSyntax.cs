using System.Text;
using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class VariableDeclarationStatementSyntax : StatementSyntax
    {
        public VariableDeclarationStatementSyntax(SyntaxToken varKeyword, SyntaxToken identifier, TypeClauseSyntax typeClause, SyntaxToken equalToken, ExpressionSyntax expression, bool isValid = true)
        {
            VarKeyword = varKeyword;
            Identifier = identifier;
            TypeClause = typeClause;
            EqualToken = equalToken;
            Expression = expression;
            IsValid = isValid;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.VariableDeclarationStatementSyntax;
        public override TextSpan Location => VarKeyword.Span + Expression.Location;
        public override bool IsValid { get; }
        public SyntaxToken VarKeyword { get; }
        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax TypeClause { get; }
        public SyntaxToken EqualToken { get; }
        public ExpressionSyntax Expression { get; }

        public override string ToString() => $"{VarKeyword.Value} {Identifier.Value}{TypeClause} = {Expression}";
    }
}