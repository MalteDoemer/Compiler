using Compiler.Text;
using System.Linq;
using System.Collections.Immutable;
using System.Text;
using System.Collections.Generic;

namespace Compiler.Syntax
{
    public sealed class CompilationUnitSyntax : SyntaxNode
    {
        internal CompilationUnitSyntax(ImmutableArray<MemberSyntax> members, bool isValid, TextLocation? location) : base(isValid, location)
        {
            Members = members;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.CompilationUnitSyntax;
        public ImmutableArray<MemberSyntax> Members { get; }

        public override IEnumerable<SyntaxNode> GetChildren() => Members;
    }
}