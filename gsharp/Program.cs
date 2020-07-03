using System;
using System.IO;
using Mono.Options;
using System.Linq;
using Compiler.Text;
using Compiler.Diagnostics;

namespace Compiler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var optionSet = (OptionSet)null;
            optionSet = new OptionSet(){
                "Usage: gsharp [OPTIONS] [PATH]",
                "",
                "Options:",
                {"h|help", "Display help.", _ => optionSet.WriteOptionDescriptions(Console.Out) },
                {"run", "Interpret the specified file", p => InterpretFile(p) },
                {"repl", "Run the REPL version of gsharp", _ => StartRepl() },
            };

            try
            {
                optionSet.Parse(args);
            }
            catch (OptionException e)
            {
                ColorWriteLine($"\ngsharp: {e.Message}", ConsoleColor.Red);
                Console.WriteLine("Try 'gsharp --help' for more information");
                Environment.Exit(-1);
            }
        }

        private static void StartRepl()
        {
            var repl = new GSharpRepl();

            try
            {
                repl.Run();
            }
            catch (Exception e)
            { 
                ColorWrite(e, ConsoleColor.Red);
                Console.ReadLine();
            }

        }

        private static void InterpretFile(string path)
        {
            if (File.Exists(path))
            {
                var text = File.ReadAllText(path);
                var compilation = Compilation.Compile(text);

                if (compilation.Diagnostics.Any()) ReportDiagnostics(compilation);
                else compilation.Evaluate();
            }
            else
            {
                ColorWriteLine("\ngsharp: The specified file does not exist", ConsoleColor.Red);
                Environment.Exit(-1);
            }
        }

        public static void ReportDiagnostics(Compilation compilation)
        {
            foreach (var d in compilation.Diagnostics)
                ReportError(compilation.Text, d);
        }

        private static void ReportError(SourceText src, Diagnostic err)
        {
            if (err.HasPositon)
            {
                var prefix = src.ToString(0, err.Span.Start);
                var errorText = src.ToString(err.Span);
                var posfix = src.ToString(err.Span.End, src.Length - err.Span.End);
                var line = src.GetLineIndex(err.Span.Start);

                NewLine();
                NewLine();
                ColorWrite($"{err.Kind} in line {line}", ConsoleColor.Gray);
                NewLine();
                NewLine();
                ColorWrite($"{prefix}", ConsoleColor.Gray);
                ColorWrite(errorText, ConsoleColor.Red);
                ColorWrite(posfix, ConsoleColor.Gray);

                NewLine();
                NewLine();
                ColorWrite(err.Message, ConsoleColor.Gray);
                NewLine();
                NewLine();
            }
            else
                ColorWrite($"\n\n{err.Kind}: {err.Message}\n\n", ConsoleColor.Red);
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

        private static void NewLine() => Console.WriteLine();
    }
}
