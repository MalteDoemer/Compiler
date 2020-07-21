using System.Collections.Generic;
using System.Collections.Immutable;
using Compiler.Text;



namespace Compiler.Syntax
{
    public class CaseStatementSyntax : StatementSyntax
    {
        public CaseStatementSyntax(SyntaxToken caseToken, ExpressionSyntax expression, SyntaxToken colonToken, ImmutableArray<StatementSyntax> statements, bool isValid, TextLocation location) : base(isValid, location)
        {
            CaseToken = caseToken;
            Expression = expression;
            ColonToken = colonToken;
            Statements = statements;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.CaseStatementSyntax;

        public SyntaxToken CaseToken { get; }
        public ExpressionSyntax Expression { get; }
        public SyntaxToken ColonToken { get; }
        public ImmutableArray<StatementSyntax> Statements { get; }
        public bool IsDefalut => Expression == null;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return CaseToken;
            if (!IsDefalut)
                yield return Expression;
            yield return ColonToken;
            foreach (var statement in Statements)
                yield return statement;
        }
    }
}