using System;
using System.Collections.Generic;
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
            compilation = Compilation.CompileScript(text, compilation);

            if (compilation.Diagnostics.HasErrors)
                compilation.Diagnostics.WriteTo(Console.Out);
            else
                compilation.Evaluate();
        }

        protected override object RenderLine(IReadOnlyList<string> lines, int lineIndex, object state)
        {
            var srcText = new SourceText(lines[lineIndex]);

            if (srcText.ToString().StartsWith('@'))
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

            compilation = Compilation.CompileScript(text, compilation);

            if (compilation.Diagnostics.Where(d =>
            {
                if (d.Message == DiagnosticBag.ErrorFormats[(int)ErrorMessage.NeverClosedCurlyBrackets] ||
                    d.Message == DiagnosticBag.ErrorFormats[(int)ErrorMessage.NeverClosedParenthesis] ||
                    d.Message == DiagnosticBag.ErrorFormats[(int)ErrorMessage.NeverClosedStringLiteral] ||
                    d.Message == string.Format(DiagnosticBag.ErrorFormats[(int)ErrorMessage.UnExpectedToken], SyntaxTokenKind.End))
                    return true;
                else
                    return false;
            }).Count() > 0) return false;

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
    }
}