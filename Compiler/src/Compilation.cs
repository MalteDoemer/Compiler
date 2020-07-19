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
        private readonly string[] referencePaths;
        private readonly bool isScript;

        private Compilation(IEnumerable<SourceText> sourceTexts, string[] referencePaths, bool isScript)
        {
            this.referencePaths = referencePaths;
            this.isScript = isScript;
            this.SourceTexts = sourceTexts;

            var diagnosticBuilder = ImmutableArray.CreateBuilder<Diagnostic>();
            var trees = new List<SyntaxTree>();

            foreach (var text in sourceTexts)
            {
                var tree = SyntaxTree.ParseSyntaxTree(text, isScript);
                diagnosticBuilder.AddRange(tree.GetDiagnostics());
                trees.Add(tree);
            }

            program = Binder.BindProgram(isScript, trees.Select(t => t.Root));
            diagnosticBuilder.AddRange(program.Diagnostics);

            Diagnostics = new DiagnosticReport(diagnosticBuilder.ToImmutable());
        }

        public DiagnosticReport Diagnostics { get; }
        public IEnumerable<SourceText> SourceTexts { get; }

        public DiagnosticReport Emit(string moduleName, string outputPath)
        {
            if (Diagnostics.HasErrors)
                return Diagnostics;

            var emitter = new Emiter(program, moduleName, referencePaths);
            emitter.Emit();
            emitter.WriteTo(outputPath);
            return new DiagnosticReport(Diagnostics.Concat(emitter.GetDiagnostics()));
        }

        public void Evaluate()
        {
            if (Diagnostics.HasErrors) return;

            using (var ms = new MemoryStream())
            {
                var emitter = new Emiter(program, "Hello", referencePaths);
                emitter.Emit();
                emitter.WriteTo(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var data = ms.ToArray();
                var assembly = System.Reflection.Assembly.Load(data);
                assembly.EntryPoint.Invoke(null, null);
            }
        }

        public void WriteBoundTree(TextWriter writer, string functionName = null)
        {
            if (functionName == null)
                writer.WriteBoundNode(program);
            else
            {
                var symbols = program.Functions.Keys.Where(s => s.Name == functionName);
                if (!symbols.Any())
                    writer.ColorWrite($"The function {functionName} does not exist.", ConsoleColor.Red);
                else
                {
                    writer.WriteBoundNode(program.Functions[symbols.First()]);
                    writer.WriteLine();
                }
            }
        }

        public void WriteControlFlowGraph(TextWriter writer, string functionName)
        {
            var symbols = program.Functions.Keys.Where(s => s.Name == functionName);
            if (!symbols.Any())
                writer.ColorWrite($"The function {functionName} does not exist.", ConsoleColor.Red);
            var cfg = ControlFlowGraph.Create(program.Functions[symbols.First()]);
            cfg.WriteTo(writer);
        }

        public static Compilation Compile(SourceText[] text, string[] referencePaths) => new Compilation(text, referencePaths, false);

        public static Compilation CompileScript(SourceText text, string[] referencePaths) => new Compilation(new[] { text }, referencePaths, true);

        public string[] GetFunctionDeclarations()
        {

            var declarations = program.Functions.Keys.Where(f => f.Syntax != null).ToArray();
            var res = new string[declarations.Length];

            for (int i = 0; i < declarations.Length; i++)
            {
                var decl = declarations[i];
                var location = decl.Syntax.Location;
                res[i] = location.ToString();
            }

            return res;
        }
    }
}
