using System;
using System.Linq;
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
                    var lexer = new Lexer(inp);
                    var tokens = lexer.Tokenize().ToArray();
                    if (lexer.diagnostics.Count > 0) foreach(var err in lexer.diagnostics) Console.WriteLine('\n' + err + '\n');
                    else foreach (var t in tokens) Console.WriteLine(t);
                }
            }
        }
    }
}
