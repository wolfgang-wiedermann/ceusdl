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
        protected string GetTableName(Interface ifa, Config conf) {        
            return $"{GetPrefix(conf)}BL{GetTypeSuffix(ifa)}_{ifa.Name}";
        }

        ///
        /// Dem Interface Namen ein BL_ voranstellen und ein _VW anhängen
        ///
        protected string GetViewName(Interface ifa, Config conf) {
            return $"{GetPrefix(conf)}BL{GetTypeSuffix(ifa)}_{ifa.Name}_VW";
        }

        ///
        /// Falls in config-Sektion enthalten, Prefix liefern, ansonsten leerstring
        ///
        protected string GetPrefix(Config conf) {
            if(conf.HasValueFor(ConfigItemEnum.PREFIX)) {
                return $"{conf.GetValue(ConfigItemEnum.PREFIX)}_";                
            } else {
                return "";
            }
        }

        ///
        /// Ermittelt den InterfaceLayer-Datenbankprefix (DB + Schema)
        ///
        protected string GetILDatabaseAndSchema(Config conf) {
            if(conf.HasValueFor(ConfigItemEnum.IL_DATABASE)) {
                return $"{conf.GetValue(ConfigItemEnum.IL_DATABASE)}.dbo.";                
            } else {
                return "";
            }
        }

        ///
        /// Ermittelt den InterfaceLayer-Datenbankprefix (DB + Schema)
        ///
        protected string GetBLDatabaseAndSchema(Config conf) {
            if(conf.HasValueFor(ConfigItemEnum.BL_DATABASE)) {
                return $"{conf.GetValue(ConfigItemEnum.BL_DATABASE)}.dbo.";                
            } else {
                return "";
            }
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
                    return temp.ReferencedAttribute.ParentInterface.Name
                        +"_"
                        +temp.ReferencedFieldName;
                }
            } else {
                // TODO: Exception werfen
                return "ERROR";
            }
        }

        ///
        /// Ermittelt einen InterfaceLayer-Konformen Attribut-Namen
        ///
        protected string GetILAttributeName(InterfaceAttribute attr) {
            string code = "";
            if(attr is InterfaceBasicAttribute) {
                var basic = (InterfaceBasicAttribute)attr;
                code += $"{attr.ParentInterface.Name}_{basic.Name}";
            } else {
                var refer = (InterfaceRefAttribute)attr;
                if(string.IsNullOrEmpty(refer.Alias)) {
                    code += $"{refer.ReferencedAttribute.ParentInterface.Name}_{refer.ReferencedAttribute.Name} ";
                } else {
                    code += $"{refer.Alias}_{refer.ReferencedAttribute.ParentInterface.Name}_{refer.ReferencedAttribute.Name} ";
                }
            }
            return code;
        }

        private string GetTypeSuffix(Interface ifa) {
            switch(ifa.Type) {
                case InterfaceType.DEF_TABLE:
                    return "_DEF";                    
                case InterfaceType.DIM_TABLE:
                case InterfaceType.DIM_VIEW:
                    return "_D";                    
                case InterfaceType.FACT_TABLE:
                    return "_F";                    
                default:
                    throw new InvalidOperationException("Ungültiger Zustand");
            }
        }
    }

}