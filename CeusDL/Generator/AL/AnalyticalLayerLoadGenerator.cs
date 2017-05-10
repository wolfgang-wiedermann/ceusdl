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
                code += GenerateFactTable(new AnalyticalFactHistoryTable(factTable, model), model);                
                if(factTable.IsHistorizedInterface()) {
                      code += GenerateFactTable(new AnalyticalFactNowTable(factTable, model), model);
                }
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
            var btGenerator = new BT.BaseLayerTransTableGenerator();
            string joincode = "";
            string selectcode = "select ";
            Dictionary<Interface, string> alias = new Dictionary<Interface, string>();

            string code = $"-- Laden der Dimensionstabelle {dim.Name}\n";            
            code += $"truncate table [dbo].[{dim.Name}]\n\n";
            code += $"insert into [dbo].[{dim.Name}] (";
            
            int i = 0;
            var tables = dim.Attributes.Select(a => a.ParentInterface).Distinct<Interface>();
            foreach(var table in tables) {
                  alias.Add(table, $"t{i}"); 
                  i++;
            }

            i = 0;
            foreach(var table in tables) {                                   

                  if(table == dim.MainInterface) {
                        joincode += $"from {GetBTDatabaseIfExists(model)}[dbo].[{btGenerator.GetTableName(table, model.Config)}] as t{i}\n";
                  } else {
                        joincode += $"left outer join {GetBTDatabaseIfExists(model)}[dbo].[{btGenerator.GetTableName(table, model.Config)}] as t{i}\n";                                                
                        joincode += $"   on {GetRefAlias(alias, table, dim)}.{table.Name}_ID = t{i}.{table.Name}_ID\n";
                  }

                  var attrs = dim.Attributes
                                 .Where(a => a.ParentInterface == table)
                                 .Select(a => (DerivedInterfaceBasicAttribute)a);

                  foreach(var attr in attrs) {
                        if(attr.Name == "Mandant_ID") {
                              selectcode += $"\n    t{i}.{attr.Name} as {attr.Name}";
                        } else {
                              selectcode += $"\n    t{i}.{blGenerator.GetAttributeName(attr.BaseAttribute)} as {attr.Name}";
                        }
                        code += $"\n    {attr.Name}";

                        if(!(attr == attrs.Last() && table == tables.Last())) {
                              selectcode += ",";
                              code += ",";
                        } 
                  }

                  i++;
            }

            code += "\n)\n";
            code += selectcode;
            code += "\n";
            code += joincode;
            code += "\n";

            return code;
        }

        private string GetRefAlias(Dictionary<Interface, string> alias, Interface table, AnalyticalDimTable dim) {
            var attr = dim.Attributes
                          .Where(a => a.ParentInterface == table)
                          .Select(a => (DerivedInterfaceBasicAttribute)a)
                          .First().ReferenceBase;
            return alias[attr];
        }

        private string GenerateFactTable(AnalyticalFactTable fact, ParserResult model)
        {           
            var btGenerator = new BT.BaseLayerTransTableGenerator();
            string joincode = "";            
            string code = $"-- Laden der Faktentabelle {fact.Name}\n";
            code += $"truncate table [dbo].[{fact.Name}]\n\n";
            code += $"insert into [dbo].[{fact.Name}] (";
            string selectcode = "select ";            

            int i = 0;
            var tables = fact.Attributes.Select(a => a.ParentInterface).Distinct<Interface>();

            foreach(var table in tables) {
                  i++;

                  if(table == fact.MainInterface) {
                        selectcode += $"\n    a.{fact.MainInterface.Name}_ID,";
                        code += $"\n    {fact.MainInterface.Name}_ID,";
                  }

                  var attrs = fact.Attributes
                            .Where(a => a.ParentInterface == table)
                            .Select(a => (InterfaceBasicAttribute)a);

                  foreach(var attr in attrs) {                                                
                        if(table == fact.MainInterface) {
                              if(attr?.ParentAttribute is InterfaceFact) {
                                    selectcode += $"\n    a.{blGenerator.GetAttributeName(attr.ParentAttribute)} as {attr.Name}";
                              } else {
                                    selectcode += $"\n    a.{attr.Name} as {attr.Name}";
                              }
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

            // Wenn es eine Ist-Stand-Tabelle zu einer Verlaufs-Tabelle ist, dann
            if(fact is AnalyticalFactNowTable) {
                  var timeAttr = fact.MainInterface.GetHistoryAttribute();
                  var timeAttrName = this.GetColumnName(timeAttr, timeAttr.ParentInterface, model);
                  joincode += $"where a.{timeAttrName} = (select max({timeAttrName}) from {GetBTDatabaseIfExists(model)}[dbo].[{btGenerator.GetTableName(fact.MainInterface, model.Config)}])\n";
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