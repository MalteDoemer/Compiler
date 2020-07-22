using System.Collections.Immutable;

namespace Compiler.Binding
{
    internal sealed class BoundSwitchStatement : BoundStatement
    {
        public BoundSwitchStatement(BoundExpression expression, ImmutableArray<BoundExpression> caseExpressions,  ImmutableArray<BoundStatement> caseStatements, BoundLabel breakLabel, bool isValid) : base(isValid)
        {
            Expression = expression;
            CaseExpressions = caseExpressions;
            CaseStatements = caseStatements;
            BreakLabel = breakLabel;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BoundSwitchStatement;

        public BoundExpression Expression { get; }
        public ImmutableArray<BoundExpression> CaseExpressions { get; }
        public ImmutableArray<BoundStatement> CaseStatements { get; }
        public BoundLabel BreakLabel { get; }
    }
}