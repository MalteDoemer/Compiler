using System;
using System.Collections.Generic;
using System.Linq;
using Compiler.Binding;
using Compiler.Diagnostics;
using Compiler.Syntax;
using Compiler.Text;

namespace Compiler
{
    public static class Program
    {
        private static readonly Dictionary<string, VariableSymbol> variables = new Dictionary<string, VariableSymbol>();
        private static Compilation compilation = null;

        public static void Main(string[] args)
        {
            var env = new Dictionary<string, (TypeSymbol type, dynamic value)>();
            try
            {
                while (true)
                {
                    ColorWrite("≫ ", ConsoleColor.Green);
                    var inp = Console.ReadLine();
                    if (inp == "exit") break;
                    else if (inp == "cls") Console.Clear();
                    else if (inp == "reset") compilation = null;
                    else
                    {
                        Evaluate(inp);
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }

        private static void Evaluate(string inp)
        {
            var src = new SourceText(inp);
            var tree = SyntaxTree.ParseSyntaxTree(src);
            compilation = compilation == null ? new Compilation(tree) : compilation.ContinueWith(tree);
            var res = compilation.Evaluate(variables);

            if (tree.Diagnostics.Errors > 0)
            {
                foreach (var err in tree.Diagnostics.GetErrors())
                {
                    if (err.Message.EndsWith("<End>.") || err.Message == "Never closed curly brackets.")
                    {
                        ColorWrite("·", ConsoleColor.Green);
                        var next = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(next))
                        {
                            ReportError(src, err);
                            continue;
                        }

                        Evaluate(inp + next);
                        break;
                    }
                    else ReportError(src, err);
                }
            }
            else
            {
                if (res == null) ColorWriteLine("null", ResolveColor(res));
                else if (res is bool && res == true) ColorWriteLine("true", ResolveColor(res));
                else if (res is bool && res == false) ColorWriteLine("false", ResolveColor(res));
                else ColorWriteLine(res, ResolveColor(res));
            }
        }

        private static void ReportError(SourceText src, Diagnostic err)
        {
            if (err.HasPositon)
            {
                var prefix = src.ToString(0, err.Span.Start);
                var errorText = src.ToString(err.Span);
                var posfix = src.ToString(err.Span.End, src.Length - err.Span.End);
                var line = src.GetLineNumber(err.Span.Start);

                NewLine();
                NewLine();
                ColorWrite($"{err.Kind} in line {line}", ConsoleColor.Gray);
                NewLine();
                NewLine();
                ColorWrite($"{prefix}", ConsoleColor.Gray);
                ColorWrite(errorText, ConsoleColor.Red);
                ColorWrite(posfix, ConsoleColor.Gray);

                NewLine();
                for (int i = 0; i < prefix.Length; i++)
                    Console.Write(' ');
                for (int i = 0; i < errorText.Length; i++)
                    ColorWrite('~', ConsoleColor.Red);
                NewLine();
                NewLine();

                ColorWrite(err.Message, ConsoleColor.Gray);
                NewLine();
                NewLine();
            }
            else
                ColorWrite($"\n\n{err.Kind}: {err.Message}\n\n", ConsoleColor.Red);
        }

        private static ConsoleColor ResolveColor(dynamic val)
        {
            if (val is string) return ConsoleColor.DarkCyan;
            if (val is long) return ConsoleColor.Cyan;
            if (val is double) return ConsoleColor.Cyan;
            if (val is bool) return ConsoleColor.Blue;
            if (val == null) return ConsoleColor.Blue;
            return ConsoleColor.Gray;
        }

        private static void ColorWrite(dynamic val, ConsoleColor col)
        {
            Console.ForegroundColor = col;
            Console.Write(val);
            Console.ResetColor();
        }

        private static void ColorWriteLine(dynamic val, ConsoleColor col)
        {
            Console.ForegroundColor = col;
            Console.WriteLine(val);
            Console.ResetColor();
        }

        private static void NewLine()
        {
            Console.WriteLine();
        }
    }
}
