﻿using System;
using System.Linq;
using System.IO;
using Compiler;
using Compiler.Text;
using System.Collections.Generic;
using Mono.Options;

namespace gsc
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var outputPath = (string)null;
            var moduleName = (string)null;
            var referencePaths = new List<string>();
            var sourcePaths = new List<string>();
            var needsHelp = false;

            var options = new OptionSet(){
                "usage: gsc <source-paths> [options]",
                "",
                {"r=", "The {path} of a assembly to referenc", r => referencePaths.Add(r) },
                {"o=", "The {path} of the output file", o => outputPath = o },
                {"m=", "The {name} of the module", m => moduleName = m },
                {"<>", s => sourcePaths.Add(s)},
                {"?|h|help", "Displays Help", h => needsHelp = true},
                "",
            };

            try
            {
                options.Parse(args);
            }
            catch (OptionException e)
            {
                WriteError(e.Message);
                return -1;
            }

            if (needsHelp)
            {
                options.WriteOptionDescriptions(Console.Out);
                return 0;
            }

            var paths = GetAllPaths(sourcePaths);
            if (paths == null) return -1;
            if (paths.Length == 0)
            {
                WriteError("No source files are provided.");
                return -1;
            }

            if (outputPath == null)
                outputPath = Path.ChangeExtension(paths[0], ".exe");

            if (moduleName == null)
                moduleName = Path.GetFileNameWithoutExtension(outputPath);

            var sourceTexts = paths.Select(p => new SourceText(File.ReadAllText(p), p)).ToArray();
            var compilation = Compilation.Compile(sourceTexts);
            var diagnostics = compilation.Emit(moduleName, outputPath, referencePaths.ToArray());
            diagnostics.WriteTo(Console.Out);

            if (!diagnostics.HasErrors)
                Console.Out.ColorWrite($"Sucessfully created {outputPath}\n", ConsoleColor.Green);

            return 0;
        }

        private static void WriteError(string err)
        {
            Console.Error.WriteLine();
            Console.Error.ColorWrite($"ERROR: {err}", ConsoleColor.Red);
            Console.Error.WriteLine();
        }

        private static string[] GetAllPaths(IEnumerable<string> paths)
        {
            var result = new List<string>(paths.Count());

            foreach (var path in paths)
            {
                if (File.Exists(path))
                    result.Add(path);
                else
                {
                    WriteError($"path {path} is not a file or directory.");
                    return null;
                }
            }
            return result.ToArray();
        }

    }
}
