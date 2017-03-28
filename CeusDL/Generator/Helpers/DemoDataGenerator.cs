using System;
using System.IO;
using System.Linq;
using Kdv.CeusDL.Generator.IL;
using Kdv.CeusDL.Parser.Model;

namespace Kdv.CeusDL.Generator.Helpers {

    public class DemoDataGenerator : InterfaceLayerAbstractGenerator
    {
        public override string GenerateCode(ParserResult model)
        {
            foreach(var ifa in model.Interfaces) {
                GenerateDataFor(ifa, model);
            }
            return "";
        }

        private void GenerateDataFor(Interface ifa, ParserResult model)
        {
            int cols = ifa.Attributes.Count;            
            string content = "";

            // Header rausschreiben
            if(ifa.IsMandantInterface()) {
                content += $"\"Mandant_KNZ\"";
            }
            foreach(var col in ifa.Attributes) {
                if(content.Length > 0) {
                    content += ";";
                }

                content += $"\"{GetILFieldName(col)}\"";
            }
            content += "\n";

            // Inhalt rausschreiben
            for(int i = 0; i < 100; i++) {
                if(ifa.IsMandantInterface()) {
                    content += $"\"0000\"";
                }
                foreach(var col in ifa.Attributes) {
                    if(col != ifa.Attributes.First()) {
                        content += ";";
                    }

                    if(col is InterfaceBasicAttribute && ((InterfaceBasicAttribute)col).Name == "KNZ") {
                        content += $"\"KNZ{i}\"";    
                    } else if(col is InterfaceRefAttribute) {
                        content += $"\"KNZ{i}\"";    
                    } else {
                        content += $"\"Inhalt von {GetILFieldName(col)}\"";
                    }
                }
                content += "\n";
            }

            File.WriteAllText(Path.Combine("GeneratedData", this.GetFilename(ifa)), content);
        }

        
    }

}