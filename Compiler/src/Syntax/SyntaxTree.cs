using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
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

        private void PrettyPrint(TextWriter writer, SyntaxNode node, string indent = "", bool isLast = true)
        {
            var marker = isLast ? "└──" : "├──";

            var token = node as SyntaxToken;

            writer.ColorWrite(indent, ConsoleColor.White);
            writer.ColorWrite(marker, ConsoleColor.White);
            if (token is null)
            {
                writer.ColorWrite(node.Kind, ConsoleColor.White);
            }
            else
            {
                writer.ColorWrite(token.Kind);
                var colorized = ColorizedText.ColorizeToken(token, null);
                writer.ColorWrite(" ");
                writer.ColorWrite(token.Location.ToString(), colorized.Color);
            }
            writer.WriteLine();
            indent += isLast ? "    " : "│   ";
            var lastChild = node.GetChildren().LastOrDefault();

            foreach (var child in node.GetChildren())
                PrettyPrint(writer, child, indent, child == lastChild);
        }

        public void WriteTo(TextWriter writer) => PrettyPrint(writer, Root);

        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                WriteTo(writer);
                return writer.ToString();
            }
        }

        public static SyntaxTree ParseSyntaxTree(SourceText sourceText, bool isScript) => new SyntaxTree(sourceText, isScript);

        public static ImmutableArray<SyntaxTokenKind> Tokenize(SourceText sourceText)
        {
            var lexer = new Lexer(sourceText, true);
            return lexer.Tokenize().Select(t => t.TokenKind).ToImmutableArray();
        }
    }
}