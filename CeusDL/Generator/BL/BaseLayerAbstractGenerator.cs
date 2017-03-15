using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System;

namespace Kdv.CeusDL.Generator.BL {
    public abstract class BaseLayerAbstractGenerator : IGenerator
    {
        public abstract string GenerateCode(ParserResult model);

        ///
        /// Dem Interface Namen ein BL_ voranstellen
        ///
        protected string GetTableName(Interface ifa) {
            return $"BL_{ifa.Name}";
        }

        ///
        /// Dem Interface Namen ein BL_ voranstellen und ein _VW anhängen
        ///
        protected string GetViewName(Interface ifa) {
            return $"BL_{ifa.Name}_VW";
        }

        ///
        /// Falls erforderlich Mandant-Spalte hinzufügen
        ///
        protected string GetMandantSpalte(Interface ifa) {
            if(ifa.IsMandantInterface()) {
                return ",\n    Mandant_KNZ varchar(10) not null";
            } else {
                return "";
            }
        }

        ///
        /// Get InterfaceLayer Name for Primary Key Field
        ///
        protected string GetILPKField(Interface ifa) {
            var pk = ifa.Attributes.Where(i => i is InterfaceBasicAttribute)
                                   .Select(i => (InterfaceBasicAttribute)i)
                                   .Where(i => i.PrimaryKey);

            return $"{ifa.Name}_{pk.First().Name}";
        }

        ///
        /// Get BaseLayer Name for Primary Key Field
        ///
        protected string GetBLPKField(Interface ifa) {
            var pk = ifa.Attributes.Where(i => i is InterfaceBasicAttribute)
                                   .Select(i => (InterfaceBasicAttribute)i)
                                   .Where(i => i.PrimaryKey);

            return GetAttributeName(pk.First());
        }

        ///
        /// Ermittelt den BL-Kompatiblen Datentyp eines Attributs
        ///
        protected string GetAttributeType(InterfaceAttribute attr) {
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
        protected string GetAttributeName(InterfaceAttribute attr) {
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