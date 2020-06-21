using System.Collections.Generic;
using System.Collections.Immutable;
using Compiler.Binding;
using Compiler.Diagnostics;
using Compiler.Text;

namespace Compiler.Syntax
{
    public sealed class SyntaxTree
    {
        internal CompilationUnitSyntax Root { get; }
        public SourceText Text { get; }
        public DiagnosticBag Diagnostics { get; }

        private SyntaxTree(SourceText text)
        {
            Text = text;
            Diagnostics = new DiagnosticBag();
            var parser = new Parser(text, Diagnostics);
            Root = parser.ParseCompilationUnit();
        }

        public static SyntaxTree ParseSyntaxTree(SourceText text)
        {
            return new SyntaxTree(text);
        }

        public static ImmutableArray<SyntaxToken> ParseTokens(SourceText text)
        {
            var bag = new DiagnosticBag();
            return new Lexer(text, bag).Tokenize().ToImmutableArray();
        }

        public override string ToString() => Root.ToString();

    }
}