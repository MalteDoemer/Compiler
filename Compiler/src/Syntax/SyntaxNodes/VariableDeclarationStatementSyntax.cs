using System.Collections.Generic;
using System.Text;
using Compiler.Text;

namespace Compiler.Syntax
{
    public sealed class VariableDeclarationStatementSyntax : StatementSyntax
    {
        internal VariableDeclarationStatementSyntax(SyntaxToken varKeyword, SyntaxToken identifier, TypeClauseSyntax typeClause, SyntaxToken equalToken, ExpressionSyntax expression, bool isValid, TextLocation location) : base(isValid, location)
        {
            VarKeyword = varKeyword;
            Identifier = identifier;
            TypeClause = typeClause;
            EqualToken = equalToken;
            Expression = expression;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.VariableDeclarationStatementSyntax;
        public SyntaxToken VarKeyword { get; }
        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax TypeClause { get; }
        public SyntaxToken EqualToken { get; }
        public ExpressionSyntax Expression { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return VarKeyword;
            yield return Identifier;
            if (TypeClause.IsExplicit)
                yield return TypeClause;
            yield return EqualToken;
            yield return Expression;
        }
    }
}