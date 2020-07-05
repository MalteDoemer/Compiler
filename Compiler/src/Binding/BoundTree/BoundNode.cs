using Compiler.Text;

namespace Compiler.Binding
{
    internal abstract class BoundNode
    {
        public abstract bool IsValid { get; } 
    }
}