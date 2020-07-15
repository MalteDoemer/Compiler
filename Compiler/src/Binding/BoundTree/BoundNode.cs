using System.IO;
using Compiler.Text;

namespace Compiler.Binding
{
    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }
        public abstract bool IsValid { get; }
        public virtual BoundConstant Constant { get; }
        public bool HasConstant => Constant != null;

        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                writer.WriteBoundNode(this);
                return writer.ToString();
            }
        }
    }
}