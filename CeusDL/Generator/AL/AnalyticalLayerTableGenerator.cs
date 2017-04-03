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
                // Test
                var dim = new AnalyticalDimTable(dimTable.Attribute, model);
                // End of Test                
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
                }   
            }
            code += "\n)\n\n";
            return code;
        }

        private string GenerateFactTable(Interface factTable, ParserResult model)
        {
            string code = $"-- Faktentabelle für {factTable.Name}\n";
            code += $"create table {GetTableName(factTable, model.Config)} (\n";
            code += $"    {factTable.Name}_ID bigint not null primary key,\n";

            if(factTable.IsMandantInterface()) {
                code += "    Mandant_ID int not null,\n";
            }

            foreach(var attr in factTable.Attributes) {
                code += $"    {GetColumnName(attr, factTable, model)} {GetColumnType(attr)}";
                if(factTable.Attributes.Last() != attr) {
                    code += ",";
                }
                code += "\n";
                // TODO: das muss mittelfristig auch rekursiv aufgelöst werden
                if(attr is InterfaceRefAttribute 
                    && ((InterfaceRefAttribute)attr)?.ReferencedAttribute?.ParentInterface.Type == InterfaceType.FACT_TABLE) {
                        var subTable = ((InterfaceRefAttribute)attr)?.ReferencedAttribute?.ParentInterface;
                        foreach(var attr2 in subTable.Attributes) {
                            if(!(attr2 is InterfaceFact)) {
                                // Problem: die Fakten sollen natürlich nicht mit übernommen werden!!!
                                code += $"    {GetColumnName(attr2, subTable, model)} {GetColumnType(attr2)}";
                                if(subTable.Attributes.Last() != attr) {
                                    code += ",";
                                }
                                code += "\n";
                            }
                        }
                }                
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