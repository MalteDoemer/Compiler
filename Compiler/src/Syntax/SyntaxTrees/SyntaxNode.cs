using Compiler.Text;



namespace Compiler.Syntax
{

    internal abstract class SyntaxNode
    {
        protected SyntaxNode(bool isValid, TextLocation location)
        {
        }

        public TextLocation Location { get; }
        public bool IsValid { get; }
        public abstract SyntaxNodeKind Kind { get; }

        public abstract override string ToString();

    }
}