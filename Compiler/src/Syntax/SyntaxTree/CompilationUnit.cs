using System.Collections.Immutable;
using System.Text;
using Compiler.Text;

namespace Compiler.Syntax
{

    internal sealed class CompilationUnitSyntax : SyntaxNode
    {
        public CompilationUnitSyntax(TextSpan span, ExpressionSyntax expression)
        {
            Span = span;
            Expression = expression;
        }

        public override TextSpan Span { get; }
        public ExpressionSyntax Expression { get; }

        public override string ToString()
        {
            return Expression.ToString();
        }
    }
}