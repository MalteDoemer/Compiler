using Compiler.Text;
using System.Linq;
using System.Collections.Immutable;
using System.Text;

namespace Compiler.Syntax
{
    internal sealed class CompilationUnitSyntax : SyntaxNode
    {
        public CompilationUnitSyntax(TextSpan span, ImmutableArray<MemberSyntax> members, bool isValid = true)
        {
            Span = span;
            Members = members;
            IsValid = isValid;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.CompilationUnitSyntax;
        public override TextSpan Span { get; }
        public override bool IsValid { get; }
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