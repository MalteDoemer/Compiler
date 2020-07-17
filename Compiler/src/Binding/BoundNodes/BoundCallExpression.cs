using System.Collections.Immutable;
using Compiler.Symbols;
using Compiler.Syntax;

namespace Compiler.Binding
{
    internal sealed class BoundCallExpression : BoundExpression
    {
        public BoundCallExpression(FunctionSymbol symbol, ImmutableArray<BoundExpression> arguments, bool isValid) : base(isValid)
        {
            Symbol = symbol;
            Arguments = arguments;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundCallExpression;
        public override TypeSymbol ResultType => Symbol.ReturnType;
        public FunctionSymbol Symbol { get; }
        public ImmutableArray<BoundExpression> Arguments { get; }
    }
}