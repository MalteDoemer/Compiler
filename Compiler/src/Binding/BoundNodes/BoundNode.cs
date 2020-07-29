using System.IO;
using Compiler.Symbols;
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

        public bool IsValid { get; }

        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                writer.WriteBoundNode(this);
                return writer.ToString();
            }
        }
    }

    internal sealed class BoundStatementExpression : BoundExpression
    {
        public BoundStatementExpression(BoundStatement statement, TypeSymbol resultType, bool isValid) : base(isValid)
        {
            Statement = statement;
            ResultType = resultType;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundStatementExpression;
        public override TypeSymbol ResultType { get; }
        public BoundStatement Statement { get; }
    }
}