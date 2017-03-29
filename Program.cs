using System;
using System.IO;
using Kdv.CeusDL.Parser;
using Kdv.CeusDL.Generator.IL;
using Kdv.CeusDL.Generator.BL;
using Kdv.CeusDL.Generator.BT;
using Kdv.CeusDL.Generator.Helpers;

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

            var ddGenerator = new DemoDataGenerator();
            ddGenerator.GenerateCode(result);

            // Interface Layer
            
            var ilDropGenerator = new InterfaceLayerDropGenerator();
            File.WriteAllText("GeneratedSQL\\IL_Drop.sql", ilDropGenerator.GenerateCode(result));

            var ilGenerator = new InterfaceLayerGenerator();
            File.WriteAllText("GeneratedSQL\\IL_Create.sql", ilGenerator.GenerateCode(result));

            var ilLoadGenerator = new InterfaceLayerLoadGenerator();        
            File.WriteAllText("GeneratedSQL\\IL_DeleteContent.sql", ilLoadGenerator.GenerateCode(result));

            // Base Layer

            var blDropGenerator = new BaseLayerDropGenerator();
            File.WriteAllText("GeneratedSQL\\BL_Drop.sql", blDropGenerator.GenerateCode(result));

            var blGenerator = new BaseLayerGenerator();
            File.WriteAllText("GeneratedSQL\\BL_Create.sql", blGenerator.GenerateCode(result));

            var blLGenerator = new BaseLayerLoadGenerator();
            File.WriteAllText("GeneratedSQL\\BL_Load.sql", blLGenerator.GenerateCode(result));

            // Base Layer Transformation

            var btGenerator = new BaseLayerTransTableGenerator();
            File.WriteAllText("GeneratedSQL\\BT_Create.sql", btGenerator.GenerateCode(result));

            Console.WriteLine();
        }
    }
}
