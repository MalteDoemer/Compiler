using System.Collections.Generic;
using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class VariableExpressionSyntax : ExpressionSyntax
    {

        public VariableExpressionSyntax(SyntaxToken name, bool isValid, TextLocation location) : base(isValid, location)
        {
            Name = name;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.VariableExpressionSyntax;
        public SyntaxToken Name { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Name;
        }
    }
}