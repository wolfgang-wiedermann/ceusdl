using System;
using System.IO;
using Kdv.CeusDL.Parser;
using Kdv.CeusDL.Generator;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Console.WriteLine("------------");

            var parser = new CeusDLParser();
            var result = parser.ParseFile(Path.Combine(".", "dsl", "interfacesample.ceusdl"));
            Console.WriteLine(result?.ToString());
            Console.WriteLine();
            var ilGenerator = new InterfaceLayerGenerator();
            Console.WriteLine(ilGenerator.GenerateCode(result));
        }
    }
}
