using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Kdv.CeusDL.Generator.AL {
    public class AnalyticalLayerDropGenerator : AnalyticalLayerAbstractGenerator
    {        
        public override string GenerateCode(ParserResult model) {  
            var factTables = GetFactTables(model);
            var dimRepo = new DirectAttachedDimRepository();

            string code = GetUseStatement(model);

            foreach(var factTable in factTables) {
                code += GenerateDropTable(new AnalyticalFactHistoryTable(factTable, model), model);
                if(factTable.IsHistorizedInterface()) {
                    code += GenerateDropTable(new AnalyticalFactNowTable(factTable, model), model);
                }
                dimRepo.AddRange(GetDirectAttachedDimensions(factTable, model));
            }

            foreach(var dimTable in dimRepo.Dimensions.Values) {
                code += GenerateDropTable(new AnalyticalDimTable(dimTable.Attribute, model), model);
            }

            return code;
        }

        private string GenerateDropTable(AnalyticalAbstractTable table, ParserResult model) {            
            string code = $"IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{table.Name}]') AND type in (N'U'))\n";
            code += $"drop table [dbo].[{table.Name}]\n";                
            code += "go\n\n";
            return code;            
        }


    }
}