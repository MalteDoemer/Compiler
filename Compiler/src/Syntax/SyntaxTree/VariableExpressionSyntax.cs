using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class VariableExpressionSyntax : ExpressionSyntax
    {

        public VariableExpressionSyntax(SyntaxToken name, bool isValid = true)
        {
            Name = name;
            IsValid = isValid;
        }

        public override bool IsValid { get; } 
        public override TextSpan Span => Name.Span;
        public SyntaxToken Name { get; }

        public override string ToString() => Name.Value.ToString();
    }
}