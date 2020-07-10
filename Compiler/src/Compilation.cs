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

namespace Compiler
{

    public sealed class Compilation
    {
        private readonly BoundProgram program;
        private readonly Compilation previous;

        private Dictionary<string, object> globals;
        private bool isScript;

        private Compilation(Compilation previous, SourceText text, Dictionary<string, object> globals, bool isScript)
        {
            this.globals = globals;
            this.isScript = isScript;
            this.Text = text;
            this.previous = previous;

            var lexer = new Lexer(text, isScript);
            var tokens = lexer.Tokenize().ToImmutableArray();

            var parser = new Parser(text, tokens, isScript);
            var unit = parser.ParseCompilationUnit();

            var previousProgram = previous == null ? null : previous.program;
            program = Binder.BindProgram(previousProgram, isScript, unit);

            var builder = ImmutableArray.CreateBuilder<Diagnostic>();
            builder.AddRange(lexer.GetDiagnostics());
            builder.AddRange(parser.GetDiagnostics());
            builder.AddRange(program.Diagnostics);

            Diagnostics = new DiagnosticReport(builder.ToImmutable());
        }

        public SourceText Text { get; }
        public DiagnosticReport Diagnostics { get; }


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

        public void WriteBoundTree(TextWriter writer, string functionName = null)
        {
            if (functionName == null)
            {
                writer.WriteBoundNode(program.GlobalStatements);
                writer.WriteLine();
            }
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

        public void WriteControlFlowGraph(TextWriter writer, string functionName = null)
        {
            ControlFlowGraph cfg;

            if (functionName == null)
                cfg = ControlFlowGraph.Create(program.GlobalStatements);
            else
            {
                var symbols = program.GetFunctionSymbols().Where(s => s.Name == functionName);
                if (!symbols.Any())
                    writer.ColorWrite($"The function {functionName} does not exist.", ConsoleColor.Red);
                cfg = ControlFlowGraph.Create(program.GetFunctionBody(symbols.First()));
            }

            writer.WriteControlFlowGraph(cfg);
        }


        public static Compilation Compile(SourceText text) => new Compilation(null, text, new Dictionary<string, object>(), false);

        public static Compilation CompileScript(SourceText text, Compilation previous = null)
        {
            var env = previous == null ? new Dictionary<string, object>() : previous.globals;
            return new Compilation(previous, text, env, true);
        }

        public static ImmutableArray<SyntaxToken> Tokenize(SourceText text)
        {
            var lexer = new Lexer(text, true);
            return lexer.Tokenize().ToImmutableArray();
        }

        public static string SyntaxTreeToString(SourceText text)
        {
            var lexer = new Lexer(text, true);
            var tokens = lexer.Tokenize().ToImmutableArray();
            var parser = new Parser(text, tokens, true);
            var root = parser.ParseCompilationUnit();
            return root.ToString();
        }
    }
}
