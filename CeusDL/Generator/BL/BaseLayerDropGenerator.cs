using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System;

namespace Kdv.CeusDL.Generator.BL {

    ///
    /// Neben der einfachen Tabelle sollte auch noch ein Unique-Key für 
    /// die PK-Attribute generiert werden (PK-Attribute sind in BL ja nicht mehr der
    /// datebankseitige Primärschlüssel)
    ///
    public class BaseLayerDropGenerator : BaseLayerAbstractGenerator
    {        
        ///
        /// Schritte:        
        /// 1. ggf. Indizies Droppen (gibts noch nicht)
        /// 2. BL Views Droppen
        /// 3. BL Tabellen Droppen        
        ///
        public override string GenerateCode(ParserResult model) {
            string code = "--\n-- BaseLayer-Tabellen aus der Datenbank entfernen\n";
            code += "--\n";

            if(model.Config.HasValueFor(ConfigItemEnum.BL_DATABASE)) {
                code += $"use [{model.Config.GetValue(ConfigItemEnum.BL_DATABASE)}]\nGO\n\n";
            }

            // TODO: foreach(var ifa in model.Interfaces) => GenerateDropConstraints(ifa, model)

            foreach(var ifa in model.Interfaces) {
                // in der IL gibt es nur DIM_TABLE und FACT_TABLE 
                if(ifa.Type != InterfaceType.DIM_VIEW) {
                    code += $"-- Tabelle und View zu {ifa.Name} entfernen\n";
                    code += GenerateDropView(ifa, model);
                    code += GenerateDropTable(ifa, model);
                }
            }
            return code;            
        }

        private string GenerateDropTable(Interface ifa, ParserResult model)
        {
            string code = $"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{this.GetTableName(ifa, model.Config)}]') AND type in (N'U'))\n";
            code += $"DROP TABLE [dbo].[{this.GetTableName(ifa, model.Config)}]\n";
            code += "GO\n\n";
            return code;
        }

        private string GenerateDropView(Interface ifa, ParserResult model)
        {
            string code = $"IF OBJECT_ID(N'{this.GetViewName(ifa, model.Config)}', N'V') IS NOT NULL\n";
            code += $"DROP VIEW [dbo].[{this.GetViewName(ifa, model.Config)}]\n";
            code += "GO\n\n";
            return code;
        }
    }
}