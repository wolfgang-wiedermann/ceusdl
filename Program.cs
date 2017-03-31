﻿using System;
using System.IO;
using Kdv.CeusDL.Parser;
using Kdv.CeusDL.Generator.IL;
using Kdv.CeusDL.Generator.BL;
using Kdv.CeusDL.Generator.BT;
using Kdv.CeusDL.Generator.AL;
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

            var alGenerator = new AnalyticalLayerTableGenerator();
            File.WriteAllText(Path.Combine("GeneratedSQL", "AL_Create.sql"), alGenerator.GenerateCode(result));

            Console.WriteLine();
        }
    }
}
