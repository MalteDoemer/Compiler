using System.Collections.Generic;
using System.Collections.Immutable;
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

        public static SyntaxTree ParseSyntaxTree(SourceText sourceText, bool isScript) => new SyntaxTree(sourceText, isScript);

        
    }
}