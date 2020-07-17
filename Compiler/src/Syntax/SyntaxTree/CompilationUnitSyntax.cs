using Compiler.Text;
using System.Linq;
using System.Collections.Immutable;
using System.Text;

namespace Compiler.Syntax
{
    internal sealed class CompilationUnitSyntax : SyntaxNode
    {
        public CompilationUnitSyntax(ImmutableArray<MemberSyntax> members, bool isValid, TextLocation location) : base(isValid, location)
        {
            Members = members;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.CompilationUnitSyntax;
        public ImmutableArray<MemberSyntax> Members { get; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var m in Members)
                builder.Append(m.ToString() + '\n');
            return builder.ToString();
        }
    }

}