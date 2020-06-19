using System;
using System.Linq;
using Compiler.Diagnostics;
using Compiler.Syntax;

namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("$ ");
                var inp = Console.ReadLine();
                if (inp == "exit") break;
                else if (inp == "cls") Console.Clear();
                else
                {
                    var bag = new DiagnosticBag();
                    var lexer = new Lexer(inp, bag);
                    var tokens = lexer.Tokenize().ToArray();

                    if (bag.Errors > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        foreach (var err in bag.GetErrors())
                            Console.WriteLine(err);
                        Console.ResetColor();
                    }
                    else foreach (var t in tokens) Console.WriteLine(t);

                }
            }
        }
    }
}
