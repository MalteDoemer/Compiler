using System;
using Compiler;

namespace gsi
{
    class Program
    {
        static void Main(string[] args)
        {
            StartRepl();
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
    }
}
