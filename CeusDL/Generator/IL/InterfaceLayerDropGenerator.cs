using System;
using System.IO;
using Kdv.CeusDL.Parser.Model;

namespace Kdv.CeusDL.Generator.IL {    
    public class InterfaceLayerDropGenerator : InterfaceLayerAbstractGenerator {        

        ///
        /// Schritte:        
        /// 1. Tabellen Droppen
        /// (Indizes oder Fremdschlüssel gibt es in der IL nicht!)
        ///
        public override string GenerateCode(ParserResult model) {
            string code = "--\n-- InterfaceLayer-Tabellen aus der Datenbank entfernen\n";
            code += "--\n";

            if(model.Config.HasValueFor(ConfigItemEnum.IL_DATABASE)) {
                code += $"use [{model.Config.GetValue(ConfigItemEnum.IL_DATABASE)}]\nGO\n\n";
            }

            foreach(var ifa in model.Interfaces) {
                // in der IL gibt es nur DIM_TABLE und FACT_TABLE 
                if(ifa.Type != InterfaceType.DEF_TABLE && ifa.Type != InterfaceType.DIM_VIEW) {
                    code += GenerateDropTable(ifa, model);
                }
            }
            return code;            
        }

        private string GenerateDropTable(Interface ifa, ParserResult model)
        {
            string code = $"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{GetPrefix(model.Config)}IL_{ifa.Name}]') AND type in (N'U'))\n";
            code += $"DROP TABLE [dbo].[{GetPrefix(model.Config)}IL_{ifa.Name}]\n";
            code += "GO\n\n";
            return code;
        }
    }
}