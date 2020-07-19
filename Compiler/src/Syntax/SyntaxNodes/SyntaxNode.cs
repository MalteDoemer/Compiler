using Compiler.Text;



namespace Compiler.Syntax
{

    internal abstract class SyntaxNode
    {
        protected SyntaxNode(bool isValid, TextLocation location)
        {
            IsValid = isValid;
            Location = location;
        }

        public TextLocation Location { get; }
        public bool IsValid { get; }
        public abstract SyntaxNodeKind Kind { get; }
    }
}