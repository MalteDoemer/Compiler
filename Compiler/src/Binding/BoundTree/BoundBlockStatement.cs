using System.Collections.Immutable;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundBlockStatement : BoundStatement
    {
        public BoundBlockStatement(ImmutableArray<BoundStatement> statements, bool isValid)
        {
            Statements = statements;
            IsValid = isValid;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundBlockStatement;
        public override bool IsValid { get; set; }
        public ImmutableArray<BoundStatement> Statements { get; }
    }
}