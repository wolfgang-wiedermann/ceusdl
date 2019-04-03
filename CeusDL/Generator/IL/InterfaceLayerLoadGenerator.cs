using System;
using System.IO;
using System.Linq;
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
            CreateLoadCsvInterface();
            return GetDeleteStatements(model);
        }

        ///
        /// Delete-Statements bauen
        ///
        public string GetDeleteStatements(ParserResult model) {
            string code = GetHeader(model);
            foreach(var obj in model.Interfaces) {
                // Nur Tabellen, und keine Def-Tabellen die beginnen erst in BL
                if(obj.Type != InterfaceType.DIM_VIEW && obj.Type != InterfaceType.DEF_TABLE) {
                    if(obj.IsMandantInterface()) {
                        code += $"delete from {GetILDatabase(model)}.dbo.{GetPrefix(model.Config)}IL_{obj.Name} where Mandant_KNZ = @mandant;\n";
                    } else {
                        code += $"truncate table {GetILDatabase(model)}.dbo.{GetPrefix(model.Config)}IL_{obj.Name};\n";
                    }
                }
            }
            return code;
        }

        private void CreateLoadCsvInterface() {
            string ifaFilename = Path.Combine(FOLDER, "IInterfacelayerLoader.cs");
            if(!Directory.Exists(FOLDER)) {
                Directory.CreateDirectory(FOLDER);
            } else if(File.Exists(ifaFilename)) {
                File.Delete(ifaFilename);                
            }

            string code = "using System.Data.Common;\n\n";
            
            code += "namespace Kdv.Loader {\n";
            code += "    public interface IInterfacelayerLoader {\n";
            code += "        void Execute(DbConnection con);\n";
            code += "    }\n";
            code += "}\n";

            File.WriteAllText(ifaFilename, code);
        }

        private static string GetDeleteStatement(Interface ifa, ParserResult model)
        {
            string code = "";
            if(ifa.Type != InterfaceType.DIM_VIEW && ifa.Type != InterfaceType.DEF_TABLE) {
                code += $"truncate table {GetILDatabase(model)}.dbo.{GetPrefix(model.Config)}IL_{ifa.Name}";
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
                if(ifa.Type != InterfaceType.DIM_VIEW && ifa.Type != InterfaceType.DEF_TABLE) {
                    File.WriteAllText(Path.Combine(FOLDER, $"{ifa.Name}Loader.cs"), CreateLoadCsvClass(ifa, model));
                }
            }
        }

        private string CreateLoadCsvClass(Interface ifa, ParserResult model)
        {
            string code ="using System;\nusing System.Collections.Generic;\nusing System.IO;\nusing System.Data.Common;\n";
            if(model.Config.HasValueFor(ConfigItemEnum.PREFIX)) {
                code += "using Kdv.Loader;\n";
            }            
            code += "\nnamespace Kdv.Loader";
            if(model.Config.HasValueFor(ConfigItemEnum.PREFIX)) {
                code += $".{model.Config.GetValue(ConfigItemEnum.PREFIX)}";
            }
            code += " {\n";
            // Parser-State-Enum generieren
            code += $"    public enum {ifa.Name}ParserState "+"{\n";
            code += "        ";

            if(ifa.IsMandantInterface()) {
                code += "IN_MANDANT_KNZ, ";
            }

            int i = 0;
            foreach(var attr in ifa.Attributes.Where(a => !a.Calculated)) {
                i++;
                code += "IN_"+GetCSAttributeName(attr, ifa).ToUpper();
                if(i < ifa.Attributes.Where(a => !a.Calculated).Count()) {
                    code += ", ";
                } else {
                    code += ", FINAL\n";
                }
            }
            code += "    }\n\n";

            // Parser-Substate-Enum generieren
            code += $"    public enum {ifa.Name}ParserSubstate ";
            code += "{\n        INITIAL, INSTRING\n    }\n\n";

            // Datenklasse generieren
            code += $"    public class {ifa.Name}Line "+"{\n";            
            // Attribute
            if(ifa.IsMandantInterface()) {
                code += "        public string Mandant_KNZ { get; set; }\n";
            }
            foreach(var attr in ifa.Attributes.Where(a => !a.Calculated)) {
                code += CreateCSAttribute(attr, ifa);
            }
            code += "    }\n\n";

            // Parserklasse generieren
            code += $"    public class {ifa.Name}Loader : IInterfacelayerLoader "+"{\n";
            code += "        public string Folder { get; set; }\n";
            code += "        public string Filename { get; set; }\n\n";

            code += $"        public {ifa.Name}Loader(string folder) {{\n";
            code += $"            this.Filename = \"{ifa.Name}.csv\";\n";
            code +=  "            this.Folder = folder;\n";
            code +=  "        }\n\n";

            // Ausführen der Übertragung in die Datenbank
            code += "\n";
            code += "        public void Execute(DbConnection con) {\n";
            code += "            string filename = Path.Combine(this.Folder, this.Filename);\n";
            code += "            if(con == null) {\n";
            code += "                // Für Debug-Zwecke Ausgabe ...\n";
            code += "                foreach(var line in Load(filename)) {\n";
            code += "                    Console.WriteLine(GetInsertSQL(line));\n";
            code += "                }\n";
            code += "            } else {\n";
            // TODO: Prüfen ob das if hier nicht überflüssig ist!
            if(ifa.Type != InterfaceType.DIM_VIEW && ifa.Type != InterfaceType.DEF_TABLE) {
                code += "                // Tabelleninhalt löschen\n";
                code += "                DeleteFromTable(con);\n";
            }
            code += "                int i = 0;\n";
            code += "                // Tatsächliche Verarbeitung: 1. Zeile wird als Kopfzeile ausgelassen.\n";
            code += "                using(var cmd = con.CreateCommand()) {\n";
            code += "                    foreach(var line in Load(filename)) {\n";
            code += "                        cmd.CommandText = GetInsertSQL(line);\n";
            code += "                        if(i > 0) {\n";
            code += "                            cmd.ExecuteNonQuery();\n";
            code += "                        }\n";
            code += "                        i++;\n";
            code += "                    }\n";
            code += "                }\n";
            code += "            }\n";
            code += "        }\n";            

            // Laden der Datei
            code += $"\n        public IEnumerable<{ifa.Name}Line> Load(string filename)"+" {\n";
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

            // Insert-SQL-Statement generieren
            code += $"        internal string GetInsertSQL({ifa.Name}Line line) {{\n";
            code += $"            string sql = \"insert into {GetILDatabase(model)}.dbo.{GetPrefix(model.Config)}IL_{ifa.Name} values (\";\n";
            
            if(ifa.IsMandantInterface()) {
                code += $"            sql += \"'\"+line.Mandant_KNZ.Substring(0, line.Mandant_KNZ.Length>10?10:line.Mandant_KNZ.Length)+\"', \";\n";
            }
            i = 0;
            foreach(var attr in ifa.Attributes.Where(a => !a.Calculated)) {
                i++;
                code += $"            if(!String.IsNullOrEmpty(line.{GetCSAttributeName(attr, ifa)})) {{\n";
                code += $"                sql += \"'\"+line.{GetCSAttributeName(attr, ifa)}{GetSubstringIfNeeded(attr, ifa)}.Replace(\"'\", \"''\")+\"'";                
                if(i < ifa.Attributes.Where(a => !a.Calculated).Count()) {
                    code += ", \";\n";;
                } else {
                    code += "\";\n";;
                }                
                code +=  "            } else {\n";
                code +=  "                sql += \"''";
                                if(i < ifa.Attributes.Where(a => !a.Calculated).Count()) {
                    code += ", \";\n";;
                } else {
                    code += "\";\n";;
                }                
                code +=  "            }\n";
            }

            code += $"            sql += \");\";\n";
            code += $"            return sql;\n";
            code += "        }\n\n";

            // Parser-Funktion
            code += $"        internal {ifa.Name}Line ParseLine(string line) "+"{\n";
            if(ifa.IsMandantInterface()) {
                code += $"            {ifa.Name}ParserState state = {ifa.Name}ParserState.IN_MANDANT_KNZ;\n";
            } else {
                code += $"            {ifa.Name}ParserState state = {ifa.Name}ParserState.IN_{GetCSAttributeName(ifa.Attributes[0], ifa).ToUpper()};\n";
            }
            code += $"            {ifa.Name}ParserSubstate substate = {ifa.Name}ParserSubstate.INITIAL;\n";
            code += $"            {ifa.Name}Line content = new {ifa.Name}Line();\n";                    
            code += "            char c = ' ';\n";
            code += "            string buf = \"\";\n\n";
            code += "            for(int i = 0; i < line.Length; i++) {\n";
            code += "                c = line[i];\n";
            code += "                switch(state) {\n";

            if(ifa.IsMandantInterface()) {
                code += $"                    case {ifa.Name}ParserState.IN_MANDANT_KNZ:\n";
                code += $"                        if(substate == {ifa.Name}ParserSubstate.INITIAL && c == '\"') "+"{\n";
                code += $"                            substate = {ifa.Name}ParserSubstate.INSTRING;\n";
                code += "                        } else if(substate == "+ifa.Name+"ParserSubstate.INITIAL && c == '\"') {\n";
                code += "                            throw new InvalidDataException($\"Ungültiges Zeichen an Position {i}\");\n";
                code += "                        } else if(substate == "+ifa.Name+"ParserSubstate.INSTRING && c == '\"') {\n";
                code += $"                            content.Mandant_KNZ = buf;\n";
                code += "                            buf = \"\";\n";
                code += $"                            substate = {ifa.Name}ParserSubstate.INITIAL;\n";
                code += "                        } else if(substate == "+ifa.Name+"ParserSubstate.INITIAL && c == ';') {\n";
                code += $"                            state = {ifa.Name}ParserState.IN_{GetCSAttributeName(ifa.Attributes[0], ifa).ToUpper()};\n";            
                code += "                        } else {\n";
                code += "                            buf += c;\n";
                code += "                        }\n";
                code += "                        break;\n";

            }

            for(i = 0; i < ifa.Attributes.Where(a => !a.Calculated).Count(); i++) {
                InterfaceAttribute nextAttr = null;
                var attr = ifa.Attributes.Where(a => !a.Calculated).ToArray()[i];
                if(i+1 < ifa.Attributes.Where(a => !a.Calculated).Count()) {
                    nextAttr = ifa.Attributes.Where(a => !a.Calculated).ToArray()[i+1];
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

            // Funktion DeleteFromTable aufbauen
            if(ifa.Type != InterfaceType.DIM_VIEW && ifa.Type != InterfaceType.DEF_TABLE) {
            code += "        public void DeleteFromTable(DbConnection con) {\n";
            code += "            using(var cmd = con.CreateCommand()) {\n";
            code += $"                cmd.CommandText = \"{GetDeleteStatement(ifa, model)}\";\n";
            code += "                cmd.ExecuteNonQuery();\n";
            code += "            }\n";                
            code += "        }\n";
            }

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

        private string GetSubstringIfNeeded(InterfaceAttribute attr, Interface ifa) {
            if(attr is InterfaceBasicAttribute) {
                var ba = (InterfaceBasicAttribute)attr;
                if(ba.DataType == InterfaceAttributeDataType.VARCHAR) {
                    return $".Substring(0, line.{GetCSAttributeName(attr, ifa)}.Length>{ba.Length}?{ba.Length}:line.{GetCSAttributeName(attr, ifa)}.Length)";
                }
            } else {
                var ba = ((InterfaceRefAttribute)attr).ReferencedAttribute;
                if(ba.DataType == InterfaceAttributeDataType.VARCHAR) {
                    return $".Substring(0, line.{GetCSAttributeName(attr, ifa)}.Length>{ba.Length}?{ba.Length}:line.{GetCSAttributeName(attr, ifa)}.Length)";
                }
            }
            return "";
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
