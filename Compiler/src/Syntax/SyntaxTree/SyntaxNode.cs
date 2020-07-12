using Compiler.Text;



namespace Compiler.Syntax
{

    internal abstract class SyntaxNode
    {
        public abstract TextLocation Location { get; }
        public abstract SyntaxNodeKind Kind { get; }
        public abstract bool IsValid { get; }

        public abstract override string ToString();

    }
}