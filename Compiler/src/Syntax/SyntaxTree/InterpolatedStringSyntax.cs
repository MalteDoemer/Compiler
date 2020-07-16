using Compiler.Text;
using System.Collections.Immutable;


namespace Compiler.Syntax
{
    internal sealed class InterpolatedStringSyntax : ExpressionSyntax
    {
        public InterpolatedStringSyntax(SyntaxToken token, ImmutableArray<string> literals, ImmutableArray<ExpressionSyntax> expressions, bool isValid, TextLocation location)
        {
            Token = token;
            Literals = literals;
            Expressions = expressions;
            IsValid = isValid;
            Location = location;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.InterpolatedStringSyntax;
        public override TextLocation Location { get; }
        public SyntaxToken Token { get; }
        public ImmutableArray<string> Literals { get; }
        public ImmutableArray<ExpressionSyntax> Expressions { get; }
        public override bool IsValid { get; }

        public override string ToString() => "fett";
    }
}