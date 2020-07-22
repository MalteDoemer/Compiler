using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression(object? value, TypeSymbol symbol, bool isValid) : base(isValid)
        {
            Constant = new BoundConstant(value);
            ResultType = symbol;
        }
        public override BoundNodeKind Kind => BoundNodeKind.BoundLiteralExpression;
        public override BoundConstant Constant { get; }
        public object? Value => Constant.Value;
        public override TypeSymbol ResultType { get; }
    }
}