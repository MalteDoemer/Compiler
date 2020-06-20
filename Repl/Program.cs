using System;
using System.Collections.Generic;
using System.Linq;
using Compiler.Binding;
using Compiler.Diagnostics;
using Compiler.Syntax;

namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            var env = new Dictionary<string, (TypeSymbol type, dynamic value)>();
            
            while (true)
            {
                Console.Write("$ ");
                var inp = Console.ReadLine();
                if (inp == "exit") break;
                else if (inp == "cls") Console.Clear();
                else Evaluator.Evaluate(inp,env, out DiagnosticBag diagnostics);
            }
        }
    }
}
