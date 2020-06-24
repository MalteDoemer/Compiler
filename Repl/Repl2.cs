using System;
using System.Collections.Generic;
using Compiler;
using Compiler.Binding;

namespace Repl
{
    public class Repl
    {
        private readonly Dictionary<string, VariableSymbol> variables;
        //private Compilation compilation;
        private List<string> history;

        public Repl()
        {
            variables = new Dictionary<string, VariableSymbol>();
            history = new List<string>();
        }


        private void Start()
        {
            while (true)
            {
                string inp = ReadInput();
            }
        }

        private string ReadInput()
        {
            throw new NotImplementedException();
        }

        public static void NotMain(string[] args)
        {
            var repl = new Repl();
            repl.Start();
        }
    }
}