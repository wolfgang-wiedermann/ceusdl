using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System;

namespace Kdv.CeusDL.Generator.BL {
    public class BaseLayerTableGenerator : BaseLayerAbstractGenerator
    {
        public override string GenerateCode(ParserResult model)
        {
            string code = "--\n-- BaseLayer\n--\n\n";
            foreach(var obj in model.Interfaces) {
                if(obj.Type == InterfaceType.DIM_TABLE || obj.Type == InterfaceType.DIM_TABLE_HISTORY) {                        
                    code += GenerateCreateDimTableCode(obj);
                } else if(obj.Type == InterfaceType.DIM_VIEW || obj.Type == InterfaceType.DIM_VIEW_HISTORY) {
                    code += "/*\n* Create a View that conforms to the following Table\n*\n* ";
                    code += GenerateCreateDimTableCode(obj).Replace("\n", "\n* ");
                    code += "\n*/\n";
                } else if(obj.Type == InterfaceType.FACT_TABLE) {                    
                    code += GenerateCreateFactTableCode(obj);
                }
            }
            return code;
        }

        ///
        /// Generierung des gesamten Codes für eine Dimensionstabelle/Interface
        ///
        public string GenerateCreateDimTableCode(Interface ifa) {
            string code = $"create table {GetTableName(ifa)} (\n";
            code += $"    {ifa.Name}_ID int primary key auto_increment"; // TODO: Das mit dem auto_increment muss ich noch auf identity anpassen

            foreach(var attribute in ifa.Attributes) {
                code += $",\n    {GetAttributeName(attribute)} {GetAttributeType(attribute)}";
            }

            if(ifa.Type == InterfaceType.DIM_TABLE_HISTORY) {
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

        ///
        /// Generierung des gesamten Codes für eine Tabelle/Interface
        ///
        public string GenerateCreateFactTableCode(Interface ifa) {
            string code = $"create table {GetTableName(ifa)} (\n";
            code += $"    {ifa.Name}_ID int primary key auto_increment"; // TODO: Das mit dem auto_increment muss ich noch auf identity anpassen

            foreach(var attribute in ifa.Attributes) {
                code += $",\n    {GetAttributeName(attribute)} {GetAttributeType(attribute)}";
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