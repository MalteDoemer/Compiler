using System;
using System.Collections.Generic;
using Compiler.Text;

namespace Compiler.Syntax
{
    public class SyntaxToken : SyntaxNode
    {
        public SyntaxTokenKind TokenKind { get; }
        public object? Value { get; }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.SyntaxToken;

        internal SyntaxToken(SyntaxTokenKind kind, TextLocation? location, object? value, bool isValid = true) : base(isValid, location)
        {
            TokenKind = kind;
            Value = value;
        }

        public override IEnumerable<SyntaxNode> GetChildren() => Array.Empty<SyntaxNode>();
    }
}