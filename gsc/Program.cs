using System;
using System.Linq;
using System.IO;
using Compiler;
using Compiler.Text;

namespace gsc
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1)
                InterpretFile(args.Single());
        }

        private static void InterpretFile(string path)
        {
            if (File.Exists(path))
            {
                var text = File.ReadAllText(path);
                var compilation = Compilation.Compile(new SourceText(text, path));

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
