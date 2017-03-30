using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Kdv.CeusDL.Generator.AL {
    public class AnalyticalLayerTableGenerator : AnalyticalLayerAbstractGenerator
    {        
        public override string GenerateCode(ParserResult model) {  
            var factTables = GetFactTables(model);
            var directAttachedDims = GetDirectAttachedDimensions(model);

            string code = GetUseStatement(model);

            foreach(var factTable in factTables) {
                code += GenerateFactTable(factTable, model);
            }

            foreach(var ddimTable in directAttachedDims) {
                // TODO: ...
            }

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

        private object GetColumnName(InterfaceAttribute attr, Interface factTable, ParserResult model)
        {
            if(attr is InterfaceBasicAttribute) {
                var a = (InterfaceBasicAttribute)attr;              
                return blGenerator.GetAttributeName(a);                
            } else if(attr is InterfaceRefAttribute) {
                var a = (InterfaceRefAttribute)attr;  
                if(string.IsNullOrEmpty(a.Alias)) {
                    return $"{a.ReferencedAttribute.ParentInterface.Name}_ID";
                } else {
                    return $"{a.Alias}_{a.ReferencedAttribute.ParentInterface.Name}_ID";
                }            
            } else {
                throw new NotImplementedException();
            }    
        }
    }
}