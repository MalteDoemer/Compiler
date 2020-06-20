namespace Compiler.Syntax
{
    internal sealed class VariableExpressionSyntax : ExpressionSyntax
    {

        public VariableExpressionSyntax(SyntaxToken name)
        {
            Name = name;
        }

        public override int Pos => Name.pos;
        public SyntaxToken Name { get; }
        public override string ToString() => $"{Name.value}";
    }
}