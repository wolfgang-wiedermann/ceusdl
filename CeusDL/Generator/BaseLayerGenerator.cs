using Kdv.CeusDL.Parser.Model;
using System.Linq;

namespace Kdv.CeusDL.Generator {
    public class BaseLayerGenerator : IGenerator {
        
        public string GenerateCode(ParserResult model) {
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

            code += "--\n-- BaseLayer Views\n--\n\n";
            foreach(var obj in model.Interfaces) {
                if(obj.Type == InterfaceType.DIM_TABLE) {
                    code += GenerateCreateViewCode(obj);
                } else if(obj.Type == InterfaceType.FACT_TABLE) {
                    // TODO: Prüfen, ob der code auf bei Faktentabellen passt !!!
                    code += GenerateCreateViewCode(obj);
                } 
                // TODO: hier weiter für die anderen Inteface-Typen
            }
            return code;
        }

        ///
        /// Generierung des gesamten Codes für eine Tabelle/Interface
        ///
        public string GenerateCreateTableCode(Interface ifa) {
            string code = $"create table {GetTableName(ifa)} (\n";
            code += $"    {ifa.Name}_ID int primary key auto_increment";

            foreach(var attribute in ifa.Attributes) {
                code += $",\n    {GetAttributeName(attribute)} {GetAttributeType(attribute)}";
            }

            code += "\n);\n\n";
            return code;
        }

        ///
        /// Generierung des gesamten Codes für eine Tabelle/Interface
        ///
        public string GenerateCreateViewCode(Interface ifa) {
            var il = new InterfaceLayerGenerator();
            string code = $"create view {GetTableName(ifa)+"_VW"} as \n";
            code += $"select bl.{ifa.Name}_ID";
            foreach(var attr in ifa.Attributes) {
                if(attr is InterfaceBasicAttribute) {
                    var basic = (InterfaceBasicAttribute)attr;
                    code += $",\n    il.{attr.ParentInterface.Name}_{basic.Name}";
                } else {
                    var refer = (InterfaceRefAttribute)attr;
                    if(string.IsNullOrEmpty(refer.Alias)) {
                        code += $",\n    il.{refer.ReferencedAttribute.ParentInterface.Name}_{refer.ReferencedAttribute.Name} ";
                    } else {
                        code += $",\n    il.{refer.Alias}_{refer.ReferencedAttribute.ParentInterface.Name}_{refer.ReferencedAttribute.Name} ";
                    }
                }
                code += $" as {GetAttributeName(attr)}";
            }
            code += $"\nfrom IL_{ifa.Name} as il\n";
            code += $"    left outer join {GetTableName(ifa)} as bl\n";
            code += $"    on il.{GetILPKField(ifa)} = bl.{GetBLPKField(ifa)};\n\n";
            return code;
        }

        ///
        /// Dem Interface Namen ein BL_ voranstellen
        ///
        private string GetTableName(Interface ifa) {
            return $"BL_{ifa.Name}";
        }

        ///
        /// Get InterfaceLayer Name for Primary Key Field
        ///
        private string GetILPKField(Interface ifa) {
            var pk = ifa.Attributes.Where(i => i is InterfaceBasicAttribute)
                                   .Select(i => (InterfaceBasicAttribute)i)
                                   .Where(i => i.PrimaryKey);

            return $"{ifa.Name}_{pk.First().Name}";
        }

        ///
        /// Get BaseLayer Name for Primary Key Field
        ///
        private string GetBLPKField(Interface ifa) {
            var pk = ifa.Attributes.Where(i => i is InterfaceBasicAttribute)
                                   .Select(i => (InterfaceBasicAttribute)i)
                                   .Where(i => i.PrimaryKey);

            return GetAttributeName(pk.First());
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
                    return temp.Alias
                        +"_"
                        +temp.ReferencedAttribute.ParentInterface.Name
                        +"_"
                        +temp.ReferencedFieldName;
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