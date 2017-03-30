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
            var children = GetChildInterfaces(dimTable);
            var alias = !string.IsNullOrEmpty(dimTable.Attribute.Alias)?dimTable.Attribute.Alias+"_":"";            

            string code = $"-- Dimensionstabelle für Schlüssel {dimTable.Key}\n";
            code += $"create table [dbo].[{blGenerator.GetPrefix(model.Config)}D_{alias}";
            code += $"{refIfa.Name}_1_{refIfa.Name}] (\n";
            code += $"    {alias}{refIfa.Name}_ID int not null primary key";

            if(refIfa.IsMandantInterface()) {
                code += $",\n    Mandant_ID int not null";
            }

            code += GetDimTableBaseFields(refIfa, alias);

            foreach(var child in children.Dimensions) {
                code += $",\n    {alias}{child.Value.Attribute.ReferencedAttribute.ParentInterface.Name}_ID int not null";
                code += GetDimTableBaseFields(child.Value.Attribute.ReferencedAttribute.ParentInterface, alias);
            }            

            code += "\n)\n\n";
            return code;
        }

        private DirectAttachedDimRepository GetChildInterfaces(DirectAttachedDim dimTable) {
            DirectAttachedDimRepository repo = new DirectAttachedDimRepository();            
            var refIfa = dimTable.Attribute.ReferencedAttribute.ParentInterface;
            var refAttrs = refIfa.Attributes
                                 .Where(a => a is InterfaceRefAttribute)
                                 .Select(a => (InterfaceRefAttribute)a);

            foreach(var refAttr in refAttrs) {
                repo.Add(new DirectAttachedDim(refAttr));
                // TODO: statt dessen rekursiv auflösen!!!
            }

            return repo;
        }

        private string GetDimTableBaseFields(Interface refIfa, string alias)
        {
            string code = "";            

            foreach(var attr in refIfa.Attributes) {
                if(attr is InterfaceBasicAttribute) {
                    var basic = (InterfaceBasicAttribute)attr;
                    code += $",\n    {alias}{GetColumnName(attr, null, null)} {GetColumnType(attr)}";
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