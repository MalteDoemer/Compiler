using System;

namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            int i = 3;
            System.Console.WriteLine((i+=2) - 2);
            Console.ReadLine();
        }
    }
}
