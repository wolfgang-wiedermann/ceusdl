using Kdv.CeusDL.Parser.Model;

namespace Kdv.CeusDL.Generator {
    public class BaseLayerGenerator : IGenerator {
        
        public string GenerateCode(ParserResult model) {
            string code = "--\n-- BaseLayer\n--\n\n";
            foreach(var obj in model.Interfaces) {
                code += GenerateCreateTableCode(obj);
            }
            return code;
        }

        public string GenerateCreateTableCode(Interface ifa) {
            string code = $"create table {GetTableName(ifa)} (\n";
            code += $"    {ifa.Name}_ID int primary key auto_increment";

            foreach(var attribute in ifa.Attributes) {
                code += $",\n    {GetAttributeName(attribute)} {GetAttributeType(attribute)}";
            }

            code += "\n);\n\n";
            return code;
        }

        private string GetTableName(Interface ifa) {
            return $"BL_{ifa.Name}";
        }

        ///
        /// Ermittelt den BL-Kompatiblen Datentyp eines Attributs
        ///
        private string GetAttributeType(InterfaceAttribute attr) {
            if(attr is InterfaceBasicAttribute) {
                var temp = (InterfaceBasicAttribute)attr;
                switch(temp.DataType) {
                    case InterfaceAttributeDataType.VARCHAR:
                        return $"varchar({temp.Length})";
                    case InterfaceAttributeDataType.DECIMAL:
                        return $"decimal({temp.Length}, {temp.Decimals})";
                    case InterfaceAttributeDataType.INT:
                        return "int";
                }            
            } else if(attr is InterfaceRefAttribute) {
                var temp = (InterfaceRefAttribute)attr;
                return GetAttributeType(temp.ReferencedAttribute);
            }
            return "ERROR";
        }

        ///
        /// Ermittelt einen BaseLayer-Konformen Attribut-Namen
        ///
        private string GetAttributeName(InterfaceAttribute attr) {
            if(attr is InterfaceBasicAttribute) {
                var temp = (InterfaceBasicAttribute)attr;
                return temp.ParentInterface.Name
                    +"_"
                    +temp.Name;
            } else if(attr is InterfaceRefAttribute) {
                var temp = (InterfaceRefAttribute)attr;
                if(!string.IsNullOrEmpty(temp.Alias)) {
                    return temp.Alias;
                } else {
                    return temp.ParentInterface.Name
                        +"_"
                        +temp.ReferencedAttribute.ParentInterface.Name
                        +"_"
                        +temp.ReferencedFieldName;
                }
            } else {
                // TODO: Exception werfen
                return "ERROR";
            }
        }
    }
}