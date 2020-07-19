using System.Collections.Generic;
using Compiler.Text;



namespace Compiler.Syntax
{
    internal sealed class BreakStatementSyntax : StatementSyntax
    {
        public BreakStatementSyntax(SyntaxToken breakToken, bool isValid, TextLocation location) : base(isValid, location)
        {
            BreakToken = breakToken;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.BreakStatementSyntax;
        public SyntaxToken BreakToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return BreakToken;
        }
    }
}