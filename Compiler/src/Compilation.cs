using System.Collections.Immutable;
using Compiler.Syntax;
using Compiler.Text;
using Compiler.Binding;
using System.Collections.Generic;
using Compiler.Diagnostics;
using System.IO;
using System;
using System.Linq;
using Compiler.Emit;

namespace Compiler
{

    public sealed class Compilation
    {
        public readonly static string[] StandardReferencePaths =
        {
            @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\3.1.0\ref\netcoreapp3.1\System.Console.dll",
            @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\3.1.0\ref\netcoreapp3.1\System.Runtime.dll",
            @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\3.1.0\ref\netcoreapp3.1\System.Runtime.Extensions.dll",
        };

        private readonly BoundProgram program;
        private readonly Compilation previous;

        private Dictionary<string, object> globals;
        private readonly string[] referencePaths;
        private bool isScript;

        private Compilation(Compilation previous, IEnumerable<SourceText> sourceTexts, Dictionary<string, object> globals, string[] referencePaths, bool isScript)
        {
            this.globals = globals;
            this.referencePaths = referencePaths;
            this.isScript = isScript;
            this.previous = previous;
            this.SourceTexts = sourceTexts;

            var diagnosticBuilder = ImmutableArray.CreateBuilder<Diagnostic>();
            var trees = new List<SyntaxTree>();

            foreach (var text in sourceTexts)
            {
                var tree = SyntaxTree.ParseSyntaxTree(text, isScript);
                diagnosticBuilder.AddRange(tree.GetDiagnostics());
                trees.Add(tree);
            }


            var previousProgram = previous == null ? null : previous.program;
            program = Binder.BindProgram(previousProgram, isScript, trees);
            diagnosticBuilder.AddRange(program.Diagnostics);

            Diagnostics = new DiagnosticReport(diagnosticBuilder.ToImmutable());
        }

        public DiagnosticReport Diagnostics { get; }
        public IEnumerable<SourceText> SourceTexts { get; }

        public void Evaluate()
        {
            if (Diagnostics.HasErrors) return;
            var tempDir = Path.GetTempPath() + "\\gsharp\\Hello.dll";
            var emitter = new Emiter(program, "Hello", referencePaths);
            emitter.Emit(tempDir);
            var assembly = System.Reflection.Assembly.LoadFile(tempDir);
            assembly.EntryPoint.Invoke(null, null);
        }

        public DiagnosticReport Emit(string moduleName, string outputPath)
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

        public static Compilation Compile(SourceText[] text, string[] referencePaths) => new Compilation(null, text, new Dictionary<string, object>(), referencePaths, false);

        public static Compilation CompileScript(SourceText text, string[] referencePaths, Compilation previous = null)
        {
            var env = previous == null ? new Dictionary<string, object>() : previous.globals;
            return new Compilation(previous, new[] { text }, env, referencePaths, true);
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
