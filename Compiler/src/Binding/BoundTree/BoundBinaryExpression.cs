using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundBinaryExpression : BoundExpression
    {
        public BoundBinaryExpression(BoundBinaryOperator op, BoundExpression left, BoundExpression right, TypeSymbol resultType, bool isValid)
        {
            ResultType = resultType;
            IsValid = isValid;
            Op = op;
            Left = left;
            Right = right;
            if (isValid)
                Constant = ConstantFolder.ComputeConstantBinary(op, left, right);
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundBinaryExpression;
        public override TypeSymbol ResultType { get; }
        public override BoundConstant Constant { get; }
        public override bool IsValid { get; }
        public BoundBinaryOperator Op { get; }
        public BoundExpression Left { get; }
        public BoundExpression Right { get; }
    }
}