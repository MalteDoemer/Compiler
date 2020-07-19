using System;
using System.Linq;
using System.Collections.Generic;
using Compiler;
using Compiler.Text;
using Compiler.Syntax;

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
        private string text = "";

        protected override void EvaluateSubmission(string text)
        {
            this.text = previous + text;
            var src = new SourceText(this.text, "<stdin>");
            var compilation = Compilation.CompileScript(src, Compilation.StandardReferencePaths);
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
            var compilation = Compilation.CompileScript(src, Compilation.StandardReferencePaths);

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
            var compilation = Compilation.CompileScript(new SourceText(text, "<stdin>"), Compilation.StandardReferencePaths);
            Console.WriteLine();
            compilation.Diagnostics.WriteTo(Console.Out);

            if (!compilation.Diagnostics.HasErrors)
                compilation.WriteBoundTree(Console.Out, function);
        }

        [MetaCommand("dump", "Displays the bound tree")]
        private void DumpCommand()
        {
            var compilation = Compilation.CompileScript(new SourceText(text, "<stdin>"), Compilation.StandardReferencePaths);
            Console.WriteLine();
            compilation.Diagnostics.WriteTo(Console.Out);

            if (!compilation.Diagnostics.HasErrors)
                compilation.WriteBoundTree(Console.Out);
        }

        [MetaCommand("print", "Displays the Syntax tree")]
        private void PrintCommand()
        {
            Console.WriteLine();
            var src = new SourceText(text, "<stdin>");
            var tree = SyntaxTree.ParseSyntaxTree(src, true);
            tree.WriteTo(Console.Out);
        }
    }
}