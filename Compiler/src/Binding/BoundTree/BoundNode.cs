using System.IO;
using Compiler.Text;

namespace Compiler.Binding
{
    internal abstract class BoundNode
    {
        protected BoundNode(bool isValid)
        {
            IsValid = isValid;
        }

        public abstract BoundNodeKind Kind { get; }
        public virtual BoundConstant Constant { get; }
        
        public bool IsValid { get; }
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