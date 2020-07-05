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
        }


        public BoundBinaryOperator Op { get; }
        public BoundExpression Left { get; }
        public BoundExpression Right { get; }

        public override TypeSymbol ResultType { get; }
        public override bool IsValid { get; }
    }
}