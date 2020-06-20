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
                else Evaluator.Evaluate(inp, out DiagnosticBag diagnostics);
            }
        }
    }
}
