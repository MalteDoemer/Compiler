using System;
using System.Linq;
using System.Collections.Generic;
using Compiler;
using Compiler.Text;
using Compiler.Syntax;
using System.IO;

namespace gsi
{
    public sealed class GSharpRepl : ReplBase
    {
        private static string[] shouldContinueMessages =
        {
            "Never closed string literal.",
            "Never closed curly brackets.",
            "Never closed parenthesis.",
            "Unexpected token: 'EndOfFile'",
        };

        private readonly Stack<string> history = new Stack<string>();
        private string previous = "";
        private Compilation compilation;

        private string outDir = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        protected override void EvaluateSubmission(string text)
        {
            text = previous + text;
            var src = new SourceText(text, "<stdin>");
            compilation = Compilation.CompileScript(src, "Main", Compilation.StandardReferencePaths);
            Console.WriteLine();
            compilation.Evaluate();
            compilation.Diagnostics.WriteTo(Console.Out);
            history.Push(string.Join(Environment.NewLine, compilation.GetFunctionDeclarations()));
            previous = history.Peek();
        }

        protected override bool IsSubmissionComplete(string text)
        {
            text = previous + text;
            var src = new SourceText(text, "<stdin>");
            var compilation = Compilation.CompileScript(src, "Main", Compilation.StandardReferencePaths);

            return !compilation.Diagnostics.Any(d => shouldContinueMessages.Contains(d.Message));
        }

        protected override void RenderLine(IReadOnlyList<string> lines, int lineCount)
        {
            var line = lines[lineCount];
            var src = new SourceText(line, "<stdin>");
            var colorizedText = ColorizedText.ColorizeTokens(src);
            Console.Out.WriteColorizedText(colorizedText);
            Console.WriteLine();
        }

        [MetaCommand("cls", "Clears the screen")]
        private void ClsCommand()
        {
            Console.Clear();
        }

        [MetaCommand("exit", "Exits the repl")]
        private void ExitCommand()
        {
            Environment.Exit(0);
        }

        [MetaCommand("undo", "undos last submission")]
        private void UndoCommand()
        {
            history.TryPop(out var res);
            if (history.TryPeek(out previous))
                previous = "";
        }

        [MetaCommand("dumpf", "Displays the bound tree of the specified function")]
        private void DumpfCommand(string function)
        {
            Console.WriteLine();
            compilation.Diagnostics.WriteTo(Console.Out);

            if (!compilation.Diagnostics.HasErrors)
                compilation.WriteBoundTree(Console.Out, function);
        }

        [MetaCommand("dump", "Displays the bound tree")]
        private void DumpCommand()
        {
            Console.WriteLine();
            compilation.Diagnostics.WriteTo(Console.Out);

            if (!compilation.Diagnostics.HasErrors)
                compilation.WriteBoundTree(Console.Out);
        }

        [MetaCommand("print", "Displays the Syntax tree")]
        private void PrintCommand()
        {
            Console.WriteLine();
            var src = compilation.SourceTexts.Single();
            var tree = SyntaxTree.ParseSyntaxTree(src, true);
            tree.WriteTo(Console.Out);
        }

        [MetaCommand("graph", "Outputs the control-flow graph")]
        private void GraphCommand(string function)
        {
            using (var writer = new StreamWriter(outDir + $"\\{function}.dot"))
                compilation.WriteControlFlowGraph(writer, function);
        }

        [MetaCommand("set-out", "Sets the output directory for various functions")]
        private void GraphPathCommand(string path)
        {
            outDir = path;
        }

        [MetaCommand("emit", "Writes the assembly to disk")]
        private void EmitCommand()
        {
            compilation.Emit(outDir + "\\Main.dll");
        }
    }
}