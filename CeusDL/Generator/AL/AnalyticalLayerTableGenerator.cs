using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Kdv.CeusDL.Generator.AL {
    public class AnalyticalLayerTableGenerator : AnalyticalLayerAbstractGenerator
    {        
        public override string GenerateCode(ParserResult model) {  
            var factTables = GetFactTables(model);
            var dimRepo = new DirectAttachedDimRepository();

            string code = GetUseStatement(model);

            foreach(var factTable in factTables) {
                code += GenerateFactTable(factTable, model);                
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
            string code = $"-- Dimensionstabelle für Schlüssel {dim.Name}\n";
            code += $"create table [dbo].[{dim.Name}] (\n";
            foreach(var field in dim.Attributes) {
                if(field is InterfaceBasicAttribute) {
                    var basic = (InterfaceBasicAttribute)field;
                    if(field == dim.Attributes.First()) {
                        code += $"    {basic.Name} {GetColumnType(field)}";
                    } else {
                        code += $",\n    {basic.Name} {GetColumnType(field)}";
                    }

                    if(field.PrimaryKey) {
                        code += " not null primary key";
                    }
                }   
            }
            code += "\n)\n\n";
            return code;
        }

        private string GenerateFactTable(Interface factTable, ParserResult model)
        {
            var m = new AnalyticalFactTable(factTable, model);

            string code = $"-- Faktentabelle für {factTable.Name}\n";
            code += $"create table {m.Name} (\n";
            code += $"    {factTable.Name}_ID bigint not null primary key,\n";            

            foreach(var attr in m.Attributes.Select(a => (InterfaceBasicAttribute)a)) {
                code += $"    {attr.Name} {GetColumnType(attr)}";
                if(m.Attributes.Last() != attr) {
                    code += ",";
                }
                code += "\n";                
            }            

            code += ")\n\n";
            return code;
        }

        private object GetColumnType(InterfaceAttribute attr)
        {
            if(attr is InterfaceBasicAttribute) {
                var a = (InterfaceBasicAttribute)attr;
                switch(a.DataType) {
                    case InterfaceAttributeDataType.DECIMAL:
                        return $"decimal({a.Length},{a.Decimals})";
                    case InterfaceAttributeDataType.INT:
                        return "int";                        
                    case InterfaceAttributeDataType.VARCHAR:
                        return $"varchar({a.Length})";
                    default:
                        throw new NotImplementedException();
                }                
            } else if(attr is InterfaceRefAttribute) {
                return "int not null";
            } else {
                throw new NotImplementedException();
            }            
        }
    }
}