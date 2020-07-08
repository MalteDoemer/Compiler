using System.Collections.Immutable;
using Compiler.Symbols;
using Compiler.Syntax;

namespace Compiler.Binding
{
    internal sealed class BoundCallExpression : BoundExpression
    {
        public BoundCallExpression(FunctionSymbol symbol, ImmutableArray<BoundExpression> arguments, bool isValid)
        {
            Symbol = symbol;
            Arguments = arguments;
            IsValid = isValid;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundCallExpression;
        public override TypeSymbol ResultType => Symbol.ReturnType;
        public override bool IsValid { get; }
        public FunctionSymbol Symbol { get; }
        public ImmutableArray<BoundExpression> Arguments { get; }
    }
}