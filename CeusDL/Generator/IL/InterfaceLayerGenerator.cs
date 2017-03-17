using Kdv.CeusDL.Parser.Model;

namespace Kdv.CeusDL.Generator.IL {

    ///
    /// Achtung: Nicht Threadsave !!! (wegen Prefix-Attribut)
    ///
    public class InterfaceLayerGenerator : IGenerator {

        private string prefix = "";

        public string GenerateCode(ParserResult model) {
            // Prefix vorbereiten
            if(model.Config.HasValueFor(ConfigItemEnum.PREFIX)) {
                prefix = $"{model.Config.GetValue(ConfigItemEnum.PREFIX)}_";
            }

            string code = "--\n-- Interface Layer\n--\n\n";
            code += GetHeader(model);
            foreach(var obj in model.Interfaces) {
                // Nur Tabellen, und keine Def-Tabellen die beginnen erst in BL
                if(InterfaceTypeResolver.IsTable(obj.Type) && obj.Type != InterfaceType.DEF_TABLE) {
                    code += GenerateILTable(obj);
                }
            }
            return code;
        }

        public string GetHeader(ParserResult model) {
            string code = "";
            if(model.Config.HasValueFor(ConfigItemEnum.IL_DATABASE)) {
                code += $"use {model.Config.GetValue(ConfigItemEnum.IL_DATABASE)};\n\n";
            }
            return code;
        }

        public string GenerateILTable(Interface ifa) {
            int i = 0;
            string code = $"create table {prefix}IL_{ifa.Name} (\n";            
            foreach(var field in ifa.Attributes) {
                code += GenerateILTableField(field);
                if(i+1 < ifa.Attributes.Count) {
                    code += ",\n";
                } else if(!ifa.IsMandantInterface()) {
                    code += "\n";
                }
                i++;
            }

            if(ifa.IsMandantInterface()) {
                code += ",\n    Mandant_KNZ varchar(10) not null\n";
            }
            code += ");\n\n";
            return code;
        }

        public string GenerateILTableField(InterfaceAttribute attr) {
            string code = "    ";
            if(attr is InterfaceBasicAttribute) {
                var basic = (InterfaceBasicAttribute)attr;
                code += $"{attr.ParentInterface.Name}_{basic.Name} ";
                code += GetTypeFromBasic(basic);
            } else if (attr is InterfaceRefAttribute) {
                var refer = (InterfaceRefAttribute)attr;
                if(string.IsNullOrEmpty(refer.Alias)) {
                    code += $"{refer.ReferencedAttribute.ParentInterface.Name}_{refer.ReferencedAttribute.Name} ";
                } else {
                    code += $"{refer.Alias}_{refer.ReferencedAttribute.ParentInterface.Name}_{refer.ReferencedAttribute.Name} ";
                }
                code += GetTypeFromBasic(refer.ReferencedAttribute);
            }
            return code;
        }

        public string GetTypeFromBasic(InterfaceBasicAttribute basic) {
            string code = "";
            if(basic.DataType == InterfaceAttributeDataType.VARCHAR) {
                code += $"varchar({basic.Length})";
            } else if(basic.DataType == InterfaceAttributeDataType.DECIMAL) {
                code += $"decimal({basic.Length},{basic.Decimals})";
            } else if(basic.DataType == InterfaceAttributeDataType.INT) {
                code += $"int";
            }
            return code;
        }
    }
}