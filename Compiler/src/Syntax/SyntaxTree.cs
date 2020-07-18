using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Compiler.Diagnostics;
using Compiler.Text;

[assembly: InternalsVisibleTo("Compiler.Compiler.Tests")]

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

        public static SyntaxTree ParseSyntaxTree(SourceText sourceText, bool isScript) => new SyntaxTree(sourceText, isScript);


        public static ImmutableArray<SyntaxToken> Tokenize(SourceText sourceText)
        {
            var lexer = new Lexer(sourceText, true);
            return lexer.Tokenize().ToImmutableArray();
        }

        internal static LiteralExpressionSyntax CreateSyntaxNode()
        {
            return new LiteralExpressionSyntax(null, false, new TextLocation(new SourceText("fett", "fett"), TextSpan.FromBounds(0, 0))); 
        }
    }
}