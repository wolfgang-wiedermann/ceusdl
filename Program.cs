using System;
using System.IO;
using Kdv.CeusDL.Parser;
using Kdv.CeusDL.Generator.IL;
using Kdv.CeusDL.Generator.BL;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var parser = new CeusDLParser();
            var result = parser.ParseFile(Path.Combine(".", "dsl", "bewerber.ceusdl"));

            if(result == null) {
                throw new Exception("Parsing-Vorgang mit Fehler abgebrochen ....");
            }

            //Console.WriteLine(result?.ToString());
            //Console.WriteLine();

            var ilGenerator = new InterfaceLayerGenerator();
            Console.WriteLine(ilGenerator.GenerateCode(result));

            //var cdlGenerator = new CeusDLGenerator();
            //Console.WriteLine(cdlGenerator.GenerateCode(result));

            var blGenerator = new BaseLayerGenerator();
            Console.WriteLine(blGenerator.GenerateCode(result));

            Console.WriteLine();
        }
    }
}
