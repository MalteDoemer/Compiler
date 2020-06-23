using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class VariableExpressionSyntax : ExpressionSyntax
    {

        public VariableExpressionSyntax(SyntaxToken name)
        {
            Name = name;
        }

        public SyntaxToken Name { get; }

        public override TextSpan Span => Name.Span;

        public override string ToString()
        {
            return Name.Value.ToString();
        }
    }
}