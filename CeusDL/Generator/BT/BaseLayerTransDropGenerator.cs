using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Kdv.CeusDL.Generator.BT {
    public class BaseLayerTransDropGenerator : BaseLayerTransAbstractGenerator
    {        
        public override string GenerateCode(ParserResult model) {
            string code = "";
            code += GetUseStatement(model);

            // Alle außer Def-Tables, die werden schon in BL angelegt und gelöscht und haben
            // keine Entsprechung in BT
            foreach(var ifa in model.Interfaces.Where(i => i.Type != InterfaceType.DEF_TABLE)) {
                code += $"IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{GetTableName(ifa, model.Config)}]') AND type in (N'U'))\n";              
                code += $"drop table {GetTableName(ifa, model.Config)}\n";
                code += "go\n\n";
            }

            return code;
        }
    }
}