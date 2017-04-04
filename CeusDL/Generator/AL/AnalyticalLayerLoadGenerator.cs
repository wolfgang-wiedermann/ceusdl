using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Kdv.CeusDL.Generator.AL {
    public class AnalyticalLayerLoadGenerator : AnalyticalLayerAbstractGenerator
    { 
        public override string GenerateCode(ParserResult model) {  
            var factTables = GetFactTables(model);
            var dimRepo = new DirectAttachedDimRepository();

            string code = GetUseStatement(model);

            foreach(var factTable in factTables) {
                code += GenerateFactTable(new AnalyticalFactTable(factTable, model), model);                
                dimRepo.AddRange(GetDirectAttachedDimensions(factTable, model));                
            }

            foreach(var dimTable in dimRepo.Dimensions.Values) {                
                var dim = new AnalyticalDimTable(dimTable.Attribute, model);                           
                code += GenerateDimTable(dim, model);
            }

            return code;            
        }

        private string GenerateDimTable(AnalyticalDimTable dim, ParserResult model)
        {                
            string code = $"-- Laden der Dimensionstabelle {dim.Name}\n";            
            
            return code;
        }

/*

SQL zum Laden einer Faktentabelle:

select a.[Antrag_ID]
      ,a.[Mandant_ID]
      ,a.[Antrag_Antragsnummer]
      ,a.[Antrag_Liefertag]
      ,a.[Antrag_Anzahl_F]
      ,t1.[Bewerber_ID]
      ,t1.[Bewerber_Bewerbernummer]
      ,t1.[Bewerber_Liefertag]
      ,t1.[Geschlecht_ID]
      ,t1.[Semester_ID]
      ,t1.[HochschulSemester_ID]
      ,t1.[FachSemester_ID]
      ,t1.[HZBArt_ID]
      ,t1.[HZBNote_ID]
      ,t1.[HZB_Kreis_ID]
      ,t1.[Heimatort_Kreis_ID]
      ,t1.[Staatsangehoerigkeit_Land_ID]
      ,a.[StudiengangHisInOne_ID]
from FH_AP_BaseLayer.dbo.AP_BT_F_Antrag a
inner join FH_AP_BaseLayer.dbo.AP_BT_F_Bewerber t1
on a.Bewerber_ID = t1.Bewerber_ID

 */
        private string GenerateFactTable(AnalyticalFactTable fact, ParserResult model)
        {           
            var btGenerator = new BT.BaseLayerTransTableGenerator();
            string joincode = "";
            string code = $"-- Laden der Faktentabelle {fact.Name}\n";
            code += $"insert into [dbo].[{fact.Name}] (";
            string selectcode = "select ";            

            int i = 0;
            var tables = fact.Attributes.Select(a => a.ParentInterface).Distinct<Interface>();

            foreach(var table in tables) {
                  i++;

                  if(table == fact.MainInterface) {
                        selectcode += $"\n    a.{table.Name}_ID,";
                        code += $"\n    {table.Name}_ID,";
                  }

                  var attrs = fact.Attributes
                            .Where(a => a.ParentInterface == table)
                            .Select(a => (InterfaceBasicAttribute)a);

                  foreach(var attr in attrs) {                                                
                        if(table == fact.MainInterface) {
                              selectcode += $"\n    a.{attr.Name} as {attr.Name}";                              
                        } else {
                              selectcode += $"\n    t{i}.{attr.Name} as {attr.Name}";
                        }
                        code += $"\n    {attr.Name}";
                        if(!(attr == attrs.Last() && table == tables.Last())) {
                              selectcode += ",";
                              code += ",";
                        }   
                  }

                  if(table == fact.MainInterface) {
                        joincode += $"\nfrom {GetBTDatabaseIfExists(model)}[dbo].[{btGenerator.GetTableName(table, model.Config)}] as a\n";
                  } else {
                        joincode += $"inner join {GetBTDatabaseIfExists(model)}[dbo].[{btGenerator.GetTableName(table, model.Config)}] as t{i}\n";                        
                        // Das mit {table.Name}_ID ist noch murks !!! evtl. brauchts hier auf der a. Seite
                        // zusätzlich noch den Alias...
                        joincode += $"   on a.{table.Name}_ID = t{i}.{table.Name}_ID\n";
                  }
            }
            code += ")\n";
            code += selectcode;
            code += joincode;
            code += "\n";
            return code;
        }

        private string GetBTDatabaseIfExists(ParserResult model) {
            if(model.Config.HasValueFor(ConfigItemEnum.AL_DATABASE)) {
                return $"[{model.Config.GetValue(ConfigItemEnum.BT_DATABASE)}].";
            } 
            return "";        
        }
    }
}