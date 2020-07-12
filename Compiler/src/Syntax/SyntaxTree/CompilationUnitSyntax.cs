using Compiler.Text;
using System.Linq;
using System.Collections.Immutable;
using System.Text;

namespace Compiler.Syntax
{
    internal sealed class CompilationUnitSyntax : SyntaxNode
    {
        public CompilationUnitSyntax(SourceText text, ImmutableArray<MemberSyntax> members, bool isValid = true)
        {
            Text = text;
            Members = members;
            IsValid = isValid;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.CompilationUnitSyntax;
        public override TextSpan Span => TextSpan.FromBounds(0, Text.Length);
        public override bool IsValid { get; }
        public SourceText Text { get; }
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