using System;
using Kdv.CeusDL.Parser;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Console.WriteLine("------------");

            var parser = new CeusDLParser();
            parser.Parse("");
        }
    }
}
