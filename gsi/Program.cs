using System;

namespace gsi
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var repl = new GSharpRepl();
                repl.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }
    }
}
