using Kdv.CeusDL.Parser.Model;

namespace Kdv.CeusDL.Generator.IL {

    ///
    /// Achtung: Nicht Threadsave !!! (wegen Prefix-Attribut)
    ///
    public class InterfaceLayerGenerator : InterfaceLayerAbstractGenerator {

        public override string GenerateCode(ParserResult model) {
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

        public string GenerateILTable(Interface ifa) {
            int i = 0;
            string code = $"create table {prefix}IL_{ifa.Name} (\n";            

            if(ifa.IsMandantInterface()) {
                code += "    Mandant_KNZ varchar(10) not null";
                if(ifa.Attributes.Count > 0) {
                    code += ",\n";
                } else {
                    code += "\n";
                }
            }
            
            foreach(var field in ifa.Attributes) {
                code += GenerateILTableField(field);
                if(i+1 < ifa.Attributes.Count) {
                    code += ",\n";
                } else if(!ifa.IsMandantInterface()) {
                    code += "\n";
                }
                i++;
            }            
            code += ");\n\n";
            return code;
        }

        public string GenerateILTableField(InterfaceAttribute attr) {
            string code = $"    {GetILFieldName(attr)} ";
            if(attr is InterfaceBasicAttribute) {
                code += GetTypeFromBasic((InterfaceBasicAttribute)attr);
            } else if (attr is InterfaceRefAttribute) {
                code += GetTypeFromBasic(((InterfaceRefAttribute)attr).ReferencedAttribute);
            }        
            return code;
        }
    }
}