using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Kdv.CeusDL.Generator.AL {
    public class AnalyticalLayerCopyGenerator : AnalyticalLayerAbstractGenerator
    {        
        public override string GenerateCode(ParserResult model) {  
            var factTables = GetFactTables(model);
            var dimRepo = new DirectAttachedDimRepository();

            string code = GetUseStatement(model);

            foreach(var factTable in factTables) {
                code += GenerateCopyFactTable(factTable, model);                
                dimRepo.AddRange(GetDirectAttachedDimensions(factTable, model));                
            }

            foreach(var dimTable in dimRepo.Dimensions.Values) {                
                var dim = new AnalyticalDimTable(dimTable.Attribute, model);                           
                code += GenerateCopyDimTable(dim, model);
            }

            return code;            
        }

        private string GenerateCopyDimTable(AnalyticalDimTable dim, ParserResult model)
        {                
            string code = $"-- Dimensionstabelle für Schlüssel {dim.Name}\n";
            code += $"truncate table {this.GetProdDBServer(model)}{this.GetALDatabaseName(model)}[dbo].[{dim.Name}];\n\n";
            code += $"insert into {this.GetProdDBServer(model)}{this.GetALDatabaseName(model)}[dbo].[{dim.Name}] (\n";

            foreach(var field in dim.Attributes) {
                if(field is InterfaceBasicAttribute) {
                    var basic = (InterfaceBasicAttribute)field;
                    if(field == dim.Attributes.First()) {
                        code += $"    {basic.Name}";
                    } else {
                        code += $",\n    {basic.Name}";
                    }                    
                }   
            }

            code += "\n)\nselect\n";

            foreach(var field in dim.Attributes) {
                if(field is InterfaceBasicAttribute) {
                    var basic = (InterfaceBasicAttribute)field;
                    if(field == dim.Attributes.First()) {
                        code += $"    {basic.Name}";
                    } else {
                        code += $",\n    {basic.Name}";
                    }                    
                }   
            }

            code += $"\nfrom {this.GetEtlDBServer(model)}{this.GetALDatabaseName(model)}[dbo].[{dim.Name}]\n\n";
            return code;
        }

        private string GenerateCopyFactTable(Interface factTable, ParserResult model)
        {
            var m = new AnalyticalFactTable(factTable, model);

            string code = $"-- Faktentabelle für {factTable.Name}\n";
            code += $"truncate table {this.GetProdDBServer(model)}{this.GetALDatabaseName(model)}[dbo].[{m.Name}];\n\n";
            code += $"insert into {this.GetProdDBServer(model)}{this.GetALDatabaseName(model)}[dbo].[{m.Name}] (\n";
            code += $"    {factTable.Name}_ID";

            foreach(var field in m.Attributes.Select(a => (InterfaceBasicAttribute)a)) {
                if(field is InterfaceBasicAttribute) {
                    var basic = (InterfaceBasicAttribute)field;
                    code += $",\n    {basic.Name}";                    
                } 
            }

            code += "\n)\nselect\n";
            code += $"    {factTable.Name}_ID";

            foreach(var field in m.Attributes.Select(a => (InterfaceBasicAttribute)a)) {
                if(field is InterfaceBasicAttribute) {
                    var basic = (InterfaceBasicAttribute)field;                    
                    code += $",\n    {basic.Name}";                    
                }   
            }
            
            code += $"\nfrom {this.GetEtlDBServer(model)}{this.GetALDatabaseName(model)}[dbo].[{m.Name}]\n\n";
            return code;            
        }

        public string GetProdDBServer(ParserResult model) {
            if(model.Config.HasValueFor(ConfigItemEnum.PROD_DB_SERVER)) {
                return $"[{model.Config.GetValue(ConfigItemEnum.PROD_DB_SERVER)}].";
            } 
            return "";
        }

        public string GetEtlDBServer(ParserResult model) {
            if(model.Config.HasValueFor(ConfigItemEnum.ETL_DB_SERVER)) {
                return $"[{model.Config.GetValue(ConfigItemEnum.ETL_DB_SERVER)}].";
            } 
            return "";
        }
    }
}