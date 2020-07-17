using System.Collections.Immutable;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundBlockStatement : BoundStatement
    {
        public BoundBlockStatement(ImmutableArray<BoundStatement> statements, bool isValid) : base(isValid)
        {
            Statements = statements;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundBlockStatement;
        public ImmutableArray<BoundStatement> Statements { get; }
    }
}