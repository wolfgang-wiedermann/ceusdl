using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System;

namespace Kdv.CeusDL.Generator {
    public class BaseLayerTableGenerator : BaseLayerAbstractGenerator
    {
        public override string GenerateCode(ParserResult model)
        {
            string code = "--\n-- BaseLayer\n--\n\n";
            foreach(var obj in model.Interfaces) {
                if(obj.Type == InterfaceType.DIM_TABLE) {
                    code += GenerateCreateTableCode(obj);
                } else if(obj.Type == InterfaceType.DIM_VIEW) {
                    code += "/*\n* Create a View that conforms to the following Table\n*\n* ";
                    code += GenerateCreateTableCode(obj).Replace("\n", "\n* ");
                    code += "\n*/\n";
                } else if(obj.Type == InterfaceType.FACT_TABLE) {
                    // TODO: Code-Generierung für Faktentabelle programmieren ...
                    code += GenerateCreateTableCode(obj);
                }
            }
            return code;
        }

        ///
        /// Generierung des gesamten Codes für eine Tabelle/Interface
        ///
        public string GenerateCreateTableCode(Interface ifa) {
            string code = $"create table {GetTableName(ifa)} (\n";
            code += $"    {ifa.Name}_ID int primary key auto_increment"; // TODO: Das mit dem auto_increment muss ich noch auf identity anpassen

            foreach(var attribute in ifa.Attributes) {
                code += $",\n    {GetAttributeName(attribute)} {GetAttributeType(attribute)}";
            }

            code += "\n);\n\n";
            return code;
        }


    }

}