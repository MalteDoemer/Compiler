using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class VariableExpressionSyntax : ExpressionSyntax
    {

        public VariableExpressionSyntax(SyntaxToken name, bool isValid, TextLocation location)
        {
            Name = name;
            IsValid = isValid;
            Location = location;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.VariableExpressionSyntax;
        public override bool IsValid { get; }
        public override TextLocation Location { get; }
        public SyntaxToken Name { get; }

        public override string ToString() => Name.Value.ToString();
    }
}