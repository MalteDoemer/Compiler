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
            ResultType = symbol.ReturnType;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundCallExpression;
        public override TypeSymbol ResultType { get; }
        public FunctionSymbol Symbol { get; }
        public ImmutableArray<BoundExpression> Arguments { get; }
    }
}