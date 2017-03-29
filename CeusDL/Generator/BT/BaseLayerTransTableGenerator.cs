using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Kdv.CeusDL.Generator.BT {
    public class BaseLayerTransTableGenerator : BaseLayerTransAbstractGenerator
    {        
        public override string GenerateCode(ParserResult model) {                        
            string result = "-- Base Layer Transformation - Create Table Skript\n\n";
            result += GetUseStatement(model);

            foreach(var ifa in model.Interfaces) {
                if(ifa.Type == InterfaceType.DIM_TABLE || ifa.Type == InterfaceType.DIM_VIEW) {
                    result += $"-- Dimmensionstabelle : {ifa.Name} \n";
                    result += GenerateCreateWithChildrenDimTableCode(ifa, model.Config);
                    // TODO: Foreign Keys generieren
                } else if(ifa.Type == InterfaceType.FACT_TABLE) {                    
                    result += $"-- Faktentabelle : {ifa.Name} \n";
                    result += GenerateCreateWithChildrenDimTableCode(ifa, model.Config);
                    // TODO: Foreign Keys generieren
                }
            }

            return result;
        }

        ///
        /// Generierung des gesamten Codes für eine Dimensionstabelle/Interface 
        ///
        public string GenerateCreateWithChildrenDimTableCode(Interface ifa, Config conf) {
            string code = $"create table {GetTableName(ifa, conf)} (\n";
            code += $"    {ifa.Name}_ID int primary key not null";            

            foreach(var attribute in ifa.Attributes) {
                if(attribute is InterfaceBasicAttribute) {
                    code += $",\n    {blGenerator.GetAttributeName(attribute)} {blGenerator.GetAttributeType(attribute)}";
                } else {
                    var r = (InterfaceRefAttribute)attribute;
                    if(string.IsNullOrEmpty(r.Alias)) {
                        code += $",\n    {r.ReferencedAttribute.ParentInterface.Name}_ID int not null";
                    } else {
                        code += $",\n    {r.Alias}_{r.ReferencedAttribute.ParentInterface.Name}_ID int not null";
                    }
                    code += $",\n    {blGenerator.GetAttributeName(attribute)} {blGenerator.GetAttributeType(attribute)}";
                }
            }

            code += GetMandantSpalte(ifa);

            if(ifa.IsHistorizedInterface()) {
                code += ",\n    T_Gueltig_Von int not null";
                code += ",\n    T_Gueltig_Bis int not null";
            }

            code += ",\n    T_Modifikation varchar(10) not null";
            code += ",\n    T_Bemerkung varchar(100)";
            code += ",\n    T_Benutzer varchar(100) not null";
            code += ",\n    T_System varchar(10) not null";
            code += ",\n    T_Erst_DAT datetime not null";
            code += ",\n    T_Aend_DAT datetime not null";
            code += ",\n    T_Ladelauf_NR int not null";

            code += "\n);\n\n";
            return code;
        }        
    }
}