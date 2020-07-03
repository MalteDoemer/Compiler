using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class InvalidMemberSyntax : MemberSyntax
    {
        public InvalidMemberSyntax(TextSpan span)
        {
            Span = span;
        }

        public override TextSpan Span { get; }

        public override bool IsValid => false;

        public override string ToString() => "Invalid";
    }

}