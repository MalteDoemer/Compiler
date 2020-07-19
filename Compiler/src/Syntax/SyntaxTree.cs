using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Compiler.Diagnostics;
using Compiler.Text;

namespace Compiler.Syntax
{
    public sealed class SyntaxTree : IDiagnostable
    {
        private readonly ImmutableArray<Diagnostic> diagnostics;

        public IEnumerable<Diagnostic> GetDiagnostics() => diagnostics;

        private SyntaxTree(SourceText text, bool isScript)
        {
            Text = text;
            var parser = new Parser(text, isScript);
            Root = parser.ParseCompilationUnit();
            diagnostics = parser.GetDiagnostics().ToImmutableArray();
        }

        public SourceText Text { get; }

        internal CompilationUnitSyntax Root { get; }

        private void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = false)
        {
            // ├──
            // └──
            // ├──

        }

        public static SyntaxTree ParseSyntaxTree(SourceText sourceText, bool isScript) => new SyntaxTree(sourceText, isScript);

        public static ImmutableArray<SyntaxTokenKind> Tokenize(SourceText sourceText)
        {
            var lexer = new Lexer(sourceText, true);
            return lexer.Tokenize().Select(t => t.TokenKind).ToImmutableArray();
        }

    }
}