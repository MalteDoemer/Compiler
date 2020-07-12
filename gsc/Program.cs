using System;
using System.Linq;
using System.IO;
using Compiler;
using Compiler.Text;
using System.Collections.Generic;

namespace gsc
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
                Console.Out.ColorWrite("[ERROR]: no paths provided", ConsoleColor.Red);
            else
            {
                var paths = new List<string>();
                foreach (var arg in args)
                {
                    if (File.Exists(arg))
                        paths.Add(arg);
                    else if (Directory.Exists(arg))
                        paths.AddRange(Directory.EnumerateFiles(arg));
                    else
                        Console.Out.ColorWrite($"[ERROR]: The path <{arg}> is not a valid file or directory.");
                }
                InterpretFiles(paths);
            }
        }

        private static void InterpretFiles(IEnumerable<string> paths)
        {
            var sourceTexts = new List<SourceText>();
            foreach (var path in paths)
            {
                var text = File.ReadAllText(path);
                sourceTexts.Add(new SourceText(text, path));
            }

            var compilation = Compilation.Compile(sourceTexts.ToArray());
            compilation.Diagnostics.WriteTo(Console.Out);
            compilation.Evaluate();
        }
    }
}
