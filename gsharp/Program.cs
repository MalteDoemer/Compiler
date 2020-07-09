using System;
using System.IO;
using System.Linq;
using Compiler.Text;
using Compiler.Diagnostics;
using System.Collections.Immutable;
using System.Collections.Generic;
using Compiler.Symbols;

namespace Compiler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 1 && File.Exists(args[0]))
                InterpretFile(args[0]);
            else StartRepl();
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ResetColor();
                Console.ReadLine();
            }

        }

        private static void InterpretFile(string path)
        {
            if (File.Exists(path))
            {
                var text = File.ReadAllText(path);
                var compilation = Compilation.Compile(text);

                if (compilation.Diagnostics.Any()) compilation.Diagnostics.WriteTo(Console.Out);
                else compilation.Evaluate();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\ngsharp: The specified file does not exist", ConsoleColor.Red);
                Console.ResetColor();
                Environment.Exit(-1);
            }
        }
    }
}
