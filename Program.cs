using System;
using System.IO;
using Kdv.CeusDL.Parser;
using Kdv.CeusDL.Generator.IL;
using Kdv.CeusDL.Generator.BL;
using Kdv.CeusDL.Generator.BT;
using Kdv.CeusDL.Generator.AL;
using Kdv.CeusDL.Generator.Helpers;
using Kdv.CeusDL.Generator.Doc;
using Kdv.CeusDL.Utils;

namespace ConsoleApplication
{
    ///
    /// Release mit .exe-Datei erzeugen:  dotnet publish -c Release -r win10-x64
    ///
    public class Program
    {
        public static void Main(string[] args)
        {
            string path ="";

            if(args.Length > 0) {
                path = args[0];
            } else {
                path = Path.Combine(".", "dsl", "bewerber.ceusdl");
            }

            if(!File.Exists(path)) {
                Console.WriteLine($"FEHLER: Die Datei {path} existiert nicht!");
                return;
            }

            var parser = new CeusDLParser();
            var result = parser.ParseFile(path);

            if(result == null) {
                throw new Exception("Parsing-Vorgang mit Fehler abgebrochen ....");
            }

            var ddGenerator = new DemoDataGenerator();
            ddGenerator.GenerateCode(result);

            // Falls die Ausgabeverzeichnisse noch nicht existieren dann anlegen...

            if(!Directory.Exists("GeneratedSQL")) {
                Directory.CreateDirectory("GeneratedSQL");
            }
            if(!Directory.Exists("GeneratedCode")) {
                Directory.CreateDirectory("GeneratedCode");
            }
            if(!Directory.Exists("GeneratedData")) {
                Directory.CreateDirectory("GeneratedData");
            }
            if(!Directory.Exists("GeneratedDoc")) {
                Directory.CreateDirectory("GeneratedDoc");
            }

            // Interface Layer
            
            var ilDropGenerator = new InterfaceLayerDropGenerator();
            File.WriteAllText(Path.Combine("GeneratedSQL", "IL_Drop.sql"), ilDropGenerator.GenerateCode(result));

            var ilGenerator = new InterfaceLayerGenerator();
            File.WriteAllText(Path.Combine("GeneratedSQL", "IL_Create.sql"), ilGenerator.GenerateCode(result));

            var ilLoadGenerator = new InterfaceLayerLoadGenerator();        
            File.WriteAllText(Path.Combine("GeneratedSQL", "IL_DeleteContent.sql"), ilLoadGenerator.GenerateCode(result));

            // Base Layer

            var blDropGenerator = new BaseLayerDropGenerator();
            File.WriteAllText(Path.Combine("GeneratedSQL", "BL_Drop.sql"), blDropGenerator.GenerateCode(result));

            var blGenerator = new BaseLayerGenerator();
            File.WriteAllText(Path.Combine("GeneratedSQL", "BL_Create.sql"), blGenerator.GenerateCode(result));

            var blLGenerator = new BaseLayerLoadGenerator();
            File.WriteAllText(Path.Combine("GeneratedSQL", "BL_Load.sql"), blLGenerator.GenerateCode(result));

            // Base Layer Transformation

            var btDropGenerator = new BaseLayerTransDropGenerator();
            File.WriteAllText(Path.Combine("GeneratedSQL", "BT_Drop.sql"), btDropGenerator.GenerateCode(result));

            var btGenerator = new BaseLayerTransTableGenerator();
            File.WriteAllText(Path.Combine("GeneratedSQL", "BT_Create.sql"), btGenerator.GenerateCode(result));

            var btLoadGenerator = new BaseLayerTransLoadGenerator();
            File.WriteAllText(Path.Combine("GeneratedSQL", "BT_Load.sql"), btLoadGenerator.GenerateCode(result));

            // Analytical Layer

            var alDropGenerator = new AnalyticalLayerDropGenerator();
            File.WriteAllText(Path.Combine("GeneratedSQL", "AL_Drop.sql"), alDropGenerator.GenerateCode(result));    

            var alGenerator = new AnalyticalLayerTableGenerator();
            File.WriteAllText(Path.Combine("GeneratedSQL", "AL_Create.sql"), alGenerator.GenerateCode(result));            

            var alLoadGenerator = new AnalyticalLayerLoadGenerator();
            File.WriteAllText(Path.Combine("GeneratedSQL", "AL_Load.sql"), alLoadGenerator.GenerateCode(result));

            // Dokumentation

            var docGenerator = new DocGenerator();
            File.WriteAllText(Path.Combine("GeneratedDoc", "doku.html"), docGenerator.GenerateCode(result));

            var dotGenerator = new GraphGenerator();
            File.WriteAllText(Path.Combine("GeneratedDoc", "doku.dot"), dotGenerator.GenerateCode(result));

            // Zusammenfassen für schnelle Nutzung

            ResultFileFinisher.AggregateDrops("GeneratedSQL");          
            ResultFileFinisher.AggregateCreates("GeneratedSQL");

            Console.WriteLine();
        }
    }
}
