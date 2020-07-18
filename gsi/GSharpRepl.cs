using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compiler.Diagnostics;
using Compiler.Syntax;
using Compiler.Text;

namespace Compiler
{
    public sealed class GSharpRepl : ReplBase
    {
        private Compilation compilation;

        public GSharpRepl() : base()
        {
        }

        protected override void EvaluateSubmission(string text)
        {
            //compilation = Compilation.CompileScript(new SourceText(text, "<stdin>"), Compilation.StandardReferencePaths, compilation);
            //compilation.Evaluate();
        }

        protected override object RenderLine(IReadOnlyList<string> lines, int lineIndex, object state)
        {
            var srcText = new SourceText(lines[lineIndex], null);

            if (srcText.ToString().StartsWith('#'))
            {
                Console.WriteLine(srcText);
                return null;
            }

            var colorizedText = Colorizer.ColorizeTokens(srcText);

            colorizedText.WriteTo(Console.Out);

            return null;
        }

        protected override bool IsCompleteSubmission(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            //compilation = Compilation.CompileScript(new SourceText(text, null), Compilation.StandardReferencePaths, compilation);

            // if (compilation.Diagnostics.Where(d =>
            // {
            //     if (d.Message == DiagnosticBag.ErrorFormats[(int)ErrorMessage.NeverClosedCurlyBrackets] ||
            //         d.Message == DiagnosticBag.ErrorFormats[(int)ErrorMessage.NeverClosedParenthesis] ||
            //         d.Message == DiagnosticBag.ErrorFormats[(int)ErrorMessage.NeverClosedStringLiteral] ||
            //         d.Message == string.Format(DiagnosticBag.ErrorFormats[(int)ErrorMessage.UnexpectedToken], SyntaxTokenKind.End))
            //         return true;
            //     else
            //         return false;
            // }).Count() > 0) return false;

            return true;
        }

        [MetaCommand("exit", "Exits the REPL")]
        private void EvaluateExit()
        {
            Environment.Exit(0);
        }

        [MetaCommand("cls", "Clears the screen")]
        private void EvaluateCls()
        {
            Console.Clear();
        }

        [MetaCommand("showBoundTree", "shows the lowered bound tree of the function")]
        private void EvaluateShowBoundTree(string function)
        {
            if (compilation != null)
                compilation.WriteBoundTree(Console.Out, function);
        }

        [MetaCommand("graph", "Emits the ControlFlowGraph for the function")]
        private void EvaluateGraph(string function)
        {
            var appPath = Environment.GetCommandLineArgs()[0];
            var appDir = Path.GetDirectoryName(appPath);
            var cfgPath = Path.Combine(appDir, "cfg.dot");

            using (var writer = new StreamWriter(cfgPath))
                if (compilation != null)
                    compilation.WriteControlFlowGraph(writer, function);
        }

    }
}