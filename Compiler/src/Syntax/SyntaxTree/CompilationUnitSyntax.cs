using Compiler.Text;
using System.Linq;
using System.Collections.Immutable;
using System.Text;

namespace Compiler.Syntax
{
    internal sealed class CompilationUnitSyntax : SyntaxNode
    {
        public CompilationUnitSyntax(TextSpan span, ImmutableArray<MemberSyntax> members)
        {
            Span = span;
            Members = members;
        }

        public override TextSpan Span { get; }
        public ImmutableArray<MemberSyntax> Members { get; }

        public override bool IsValid => Members.Where(m => !m.IsValid).Count() == 0;

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var m in Members)
                builder.Append(m.ToString() + '\n');
            return builder.ToString();
        }
    }

}