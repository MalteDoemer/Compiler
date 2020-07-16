using System.Collections.Immutable;
using Compiler.Syntax;
using Compiler.Text;
using Compiler.Binding;
using System.Collections.Generic;
using Compiler.Diagnostics;
using System.IO;
using Compiler.Symbols;
using System;
using System.Linq;
using Compiler.Emit;

namespace Compiler
{

    public sealed class Compilation
    {
        private readonly BoundProgram program;
        private readonly Compilation previous;

        private Dictionary<string, object> globals;
        private bool isScript;

        private Compilation(Compilation previous, IEnumerable<SourceText> sourceTexts, Dictionary<string, object> globals, bool isScript)
        {
            this.globals = globals;
            this.isScript = isScript;
            this.previous = previous;
            this.SourceTexts = sourceTexts;

            var diagnosticBuilder = ImmutableArray.CreateBuilder<Diagnostic>();
            var units = new List<CompilationUnitSyntax>();

            foreach (var text in sourceTexts)
            {
                var parser = new Parser(text, isScript);
                var unit = parser.ParseCompilationUnit();
                diagnosticBuilder.AddRange(parser.GetDiagnostics());
                units.Add(unit);
            }


            var previousProgram = previous == null ? null : previous.program;
            program = Binder.BindProgram(previousProgram, isScript, units);
            diagnosticBuilder.AddRange(program.Diagnostics);

            Diagnostics = new DiagnosticReport(diagnosticBuilder.ToImmutable());
        }

        public DiagnosticReport Diagnostics { get; }
        public IEnumerable<SourceText> SourceTexts { get; }

        public void Evaluate()
        {
            if (Diagnostics.HasErrors) return;

            var evaluator = new Evaluator(program, globals);
            evaluator.Evaluate();
        }

        public object EvaluateExpression()
        {
            if (Diagnostics.HasErrors) return null;

            var evaluator = new Evaluator(program, globals);
            return evaluator.Evaluate();
        }

        public DiagnosticReport Emit(string moduleName, string outputPath, string[] referencePaths)
        {
            if (Diagnostics.HasErrors)
                return Diagnostics;

            var emitter = new Emiter(program, moduleName, referencePaths);
            emitter.Emit(outputPath);
            return new DiagnosticReport(Diagnostics.Concat(emitter.GetDiagnostics()));
        }

        public void WriteBoundTree(TextWriter writer, string functionName = null)
        {
            if (functionName == null)
                writer.WriteBoundNode(program);
            else
            {
                var symbols = program.GetFunctionSymbols().Where(s => s.Name == functionName);
                if (!symbols.Any())
                    writer.ColorWrite($"The function {functionName} does not exist.", ConsoleColor.Red);
                else
                {
                    writer.WriteBoundNode(program.GetFunctionBody(symbols.First()));
                    writer.WriteLine();
                }
            }
        }

        public void WriteControlFlowGraph(TextWriter writer, string functionName)
        {
            var symbols = program.GetFunctionSymbols().Where(s => s.Name == functionName);
            if (!symbols.Any())
                writer.ColorWrite($"The function {functionName} does not exist.", ConsoleColor.Red);
            var cfg = ControlFlowGraph.Create(program.GetFunctionBody(symbols.First()));
            writer.WriteControlFlowGraph(cfg);
        }

        public static Compilation Compile(params SourceText[] text) => new Compilation(null, text, new Dictionary<string, object>(), false);

        public static Compilation CompileScript(SourceText text, Compilation previous = null)
        {
            var env = previous == null ? new Dictionary<string, object>() : previous.globals;
            return new Compilation(previous, new[] { text }, env, true);
        }

        public static ImmutableArray<SyntaxToken> Tokenize(SourceText text)
        {
            var lexer = new Lexer(text, true);
            return lexer.Tokenize().ToImmutableArray();
        }

        public static string SyntaxTreeToString(SourceText text)
        {
            var parser = new Parser(text, true);
            var root = parser.ParseCompilationUnit();
            return root.ToString();
        }
    }
}
