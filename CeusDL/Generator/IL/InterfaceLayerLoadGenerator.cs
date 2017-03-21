using System;
using System.IO;
using Kdv.CeusDL.Parser.Model;

namespace Kdv.CeusDL.Generator.IL {    

    ///
    /// Generiert den Code zum Laden der CSV-Dateien in die IL-Tabellen
    ///
    /// Ablauf:
    /// -------
    /// * Daten des Mandanten aus IL löschen
    /// * CSV-Dateien zeilenweise in IL einlesen
    public class InterfaceLayerLoadGenerator : InterfaceLayerAbstractGenerator
    {
        public const string FOLDER = "GeneratedCode";

        public override string GenerateCode(ParserResult model)
        {
            CreateLoadCsvClasses(model);
            return GetDeleteStatements(model);
        }

        ///
        /// Delete-Statements bauen
        ///
        public string GetDeleteStatements(ParserResult model) {
            string code = GetHeader(model);
            foreach(var obj in model.Interfaces) {
                // Nur Tabellen, und keine Def-Tabellen die beginnen erst in BL
                if(InterfaceTypeResolver.IsTable(obj.Type) && obj.Type != InterfaceType.DEF_TABLE) {
                    if(obj.IsMandantInterface()) {
                        code += $"delete from {obj.Name} where Mandant_KNZ = @mandant;\n";
                    } else {
                        code += $"truncate table {obj.Name};\n";
                    }
                }
            }
            return code;
        }

        public void CreateLoadCsvClasses(ParserResult model) {
            if(!Directory.Exists(FOLDER)) {
                Directory.CreateDirectory(FOLDER);
            } else {
                Directory.Delete(FOLDER, true);
                Directory.CreateDirectory(FOLDER);
            }

            foreach(var ifa in model.Interfaces) {
                if(InterfaceTypeResolver.IsTable(ifa.Type) && ifa.Type != InterfaceType.DEF_TABLE) {
                    File.WriteAllText(Path.Combine(FOLDER, $"{ifa.Name}Loader.cs"), CreateLoadCsvClass(ifa, model));
                }
            }
        }

        private string CreateLoadCsvClass(Interface ifa, ParserResult model)
        {
            string code ="using System.Collections.Generic;\nusing System.IO;\n\nnamespace Kdv.Loader";
            if(model.Config.HasValueFor(ConfigItemEnum.PREFIX)) {
                code += $".{model.Config.GetValue(ConfigItemEnum.PREFIX)}";
            }
            code += " {\n";
            // Parser-State-Enum generieren
            code += $"    public enum {ifa.Name}ParserState "+"{\n";
            code += "        ";
            int i = 0;
            foreach(var attr in ifa.Attributes) {
                i++;
                code += "IN_"+GetCSAttributeName(attr, ifa).ToUpper();
                if(i < ifa.Attributes.Count) {
                    code += ", ";
                } else {
                    code += ", FINAL\n";
                }
            }
            code += "    }\n\n";

            // Parser-Substate-Enum generieren
            code += $"    public enum {ifa.Name}ParserSubstate ";
            code += "{\n        INITIAL, INSTRING\n    }\n\n";

            // Parser- und Datenklasse generieren
            code += $"    public class {ifa.Name}Loader "+"{\n";
            
            // Attribute
            foreach(var attr in ifa.Attributes) {
                code += CreateCSAttribute(attr, ifa);
            }

            // Laden der Datei
            code += $"\n        public static IEnumerable<{ifa.Name}Loader> Load(string filename)"+" {\n";
            code += "            if(!File.Exists(filename)) {\n";
            code += "                throw new FileNotFoundException(filename);\n";
            code += "            }\n\n";
            code += "            using(var fs = new StreamReader(new FileStream(filename, FileMode.Open))) {\n";
            code += "                string line = \"\";\n\n";
            code += "                while((line = fs.ReadLine()) != null) {\n";
            code += "                    yield return ParseLine(line);\n";
            code += "                }\n";
            code += "            }\n";
            code += "        }\n\n";

            // Parser-Funktion
            code += $"        internal static {ifa.Name}Loader ParseLine(string line) "+"{\n";
            code += $"            {ifa.Name}ParserState state = {ifa.Name}ParserState.IN_{GetCSAttributeName(ifa.Attributes[0], ifa).ToUpper()};\n";
            code += $"            {ifa.Name}ParserSubstate substate = {ifa.Name}ParserSubstate.INITIAL;\n";
            code += $"            {ifa.Name}Loader content = new {ifa.Name}Loader();\n";           
            code += "            char c = ' ';\n";
            code += "            string buf = \"\";\n\n";
            code += "            for(int i = 0; i < line.Length; i++) {\n";
            code += "                c = line[i];\n";
            code += "                switch(state) {\n";

            for(i = 0; i < ifa.Attributes.Count; i++) {
                InterfaceAttribute nextAttr = null;
                var attr = ifa.Attributes[i];
                if(i+1 < ifa.Attributes.Count) {
                    nextAttr = ifa.Attributes[i+1];
                }

                code += $"                    case {ifa.Name}ParserState.IN_{GetCSAttributeName(attr, ifa).ToUpper()}:\n";
                code += $"                        if(substate == {ifa.Name}ParserSubstate.INITIAL && c == '\"') "+"{\n";
                code += $"                            substate = {ifa.Name}ParserSubstate.INSTRING;\n";
                code += "                        } else if(substate == "+ifa.Name+"ParserSubstate.INITIAL && c == '\"') {\n";
                code += "                            throw new InvalidDataException($\"Ungültiges Zeichen an Position {i}\");\n";
                code += "                        } else if(substate == "+ifa.Name+"ParserSubstate.INSTRING && c == '\"') {\n";
                code += $"                            content.{GetCSAttributeName(attr, ifa)} = buf;\n";
                code += "                            buf = \"\";\n";
                code += $"                            substate = {ifa.Name}ParserSubstate.INITIAL;\n";
                code += "                        } else if(substate == "+ifa.Name+"ParserSubstate.INITIAL && c == ';') {\n";
                if(nextAttr == null) {
                    code += $"                            state = {ifa.Name}ParserState.FINAL;\n";
                } else {
                    code += $"                            state = {ifa.Name}ParserState.IN_{GetCSAttributeName(nextAttr, ifa).ToUpper()};\n";
                }
                code += "                        } else {\n";
                code += "                            buf += c;\n";
                code += "                        }\n";
                code += "                        break;\n";
            }

            code += "                    default:\n";
            code += "                        throw new InvalidDataException(\"Invalid State : \"+state);\n";
            code += "                }\n";
            code += "            }\n";
            code += "            return content;\n";
            code += "        }\n\n";

            // Klasse und Namespace abschließen
            code += "    }\n";
            code += "}\n";

            return code;            
        }

        private string CreateCSAttribute(InterfaceAttribute attr, Interface ifa) {
            string code = $"        public string {GetCSAttributeName(attr, ifa)} ";
            code += "{ get; set; }\n";
            return code;
        }

        public string GetCSAttributeName(InterfaceAttribute attr, Interface ifa) {
            if(attr is InterfaceBasicAttribute) {
                var ba = (InterfaceBasicAttribute)attr;
                return $"{ifa.Name}_{ba.Name}";
            } else {
                var ra = (InterfaceRefAttribute)attr;
                if(string.IsNullOrEmpty(ra.Alias)) {
                    return $"{ra.ReferencedAttribute.ParentInterface.Name}_{ra.ReferencedAttribute.Name}";
                } else {
                    return $"{ra.Alias}_{ra.ReferencedAttribute.ParentInterface.Name}_{ra.ReferencedAttribute.Name}";
                }
            }
        }
    }
}
