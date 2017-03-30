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
                code += GenerateDimTable(dimTable, model);
            }

            return code;            
        }

        private string GenerateDimTable(DirectAttachedDim dimTable, ParserResult model)
        {
            var refIfa = dimTable.Attribute.ReferencedAttribute.ParentInterface;
            var repo = new DirectAttachedDimRepository();

            string code = $"-- Dimensionstabelle für Schlüssel {dimTable.Key}\n";
            code += $"create table [dbo].[{blGenerator.GetPrefix(model.Config)}D_";
            code += !string.IsNullOrEmpty(dimTable.Attribute.Alias)?dimTable.Attribute.Alias+"_":"";
            code += $"{refIfa.Name}_1_{refIfa.Name}] (\n";
            code += $"    {refIfa.Name}_ID int not null primary key";

            if(refIfa.IsMandantInterface()) {
                code += $",\n    Mandant_ID int not null";
            }

            code += GetDimTableBaseFields(refIfa);            
            GetDimTableRefFields(refIfa, repo);

            foreach(var subRefIfa in repo.Dimensions.Values) {
                code += $",\n    {subRefIfa.Attribute.ParentInterface.Name}_ID int";
                code += GetDimTableBaseFields(subRefIfa.Attribute.ParentInterface);
            }

            code += ")\n\n";
            return code;
        }

        private void GetDimTableRefFields(Interface refIfa, DirectAttachedDimRepository repo)
        {                        
            foreach(var attr in refIfa.Attributes) {
                if(attr is InterfaceRefAttribute) {
                    var refer = (InterfaceRefAttribute)attr;
                    repo.Add(new DirectAttachedDim(refer));
                    GetDimTableRefFields(refer.ReferencedAttribute.ParentInterface, repo);
                }     
            }
        }

        private string GetDimTableBaseFields(Interface refIfa)
        {
            string code = "";            

            foreach(var attr in refIfa.Attributes) {
                if(attr is InterfaceBasicAttribute) {
                    var basic = (InterfaceBasicAttribute)attr;
                    code += $",\n    {GetColumnName(attr, null, null)} {GetColumnType(attr)}";
                }     
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
    }
}