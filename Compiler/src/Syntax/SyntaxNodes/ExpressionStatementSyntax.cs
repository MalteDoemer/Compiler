using System.Collections.Generic;
using Compiler.Text;

namespace Compiler.Syntax
{
    public sealed class ExpressionStatementSyntax : StatementSyntax
    {
        internal ExpressionStatementSyntax(ExpressionSyntax expression, bool isValid, TextLocation? location) : base(isValid, location)
        {
            Expression = expression;
        }
        
        public override SyntaxNodeKind Kind => SyntaxNodeKind.ExpressionStatementSyntax;
        public ExpressionSyntax Expression { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Expression;
        }
    }
}