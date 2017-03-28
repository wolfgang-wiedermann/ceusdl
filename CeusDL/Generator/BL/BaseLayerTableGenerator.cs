using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System;

namespace Kdv.CeusDL.Generator.BL {

    ///
    /// Neben der einfachen Tabelle sollte auch noch ein Unique-Key für 
    /// die PK-Attribute generiert werden (PK-Attribute sind in BL ja nicht mehr der
    /// datebankseitige Primärschlüssel)
    ///
    public class BaseLayerTableGenerator : BaseLayerAbstractGenerator
    {
        public override string GenerateCode(ParserResult model)
        {
            string code = "--\n-- BaseLayer\n--\n\n";
            code += GetHeader(model);
            foreach(var obj in model.Interfaces) {
                if(obj.Type == InterfaceType.DIM_TABLE) {                        
                    code += GenerateCreateDimTableCode(obj, model.Config);
                    code += GenerateUniqueConstraint(obj, model.Config);
                } else if(obj.Type == InterfaceType.DIM_VIEW) {
                    code += "/*\n* Create a View that conforms to the following Table\n*\n* ";
                    code += GenerateCreateDimTableCode(obj, model.Config).Replace("\n", "\n* ");
                    code += "*/\n";
                } else if(obj.Type == InterfaceType.FACT_TABLE) {                    
                    code += GenerateCreateFactTableCode(obj, model);
                } else if(obj.Type == InterfaceType.DEF_TABLE) {                    
                    code += GenerateCreateDefTableCode(obj, model.Config);
                    code += GenerateUniqueConstraint(obj, model.Config);
                }
            }
            return code;
        }

        public string GetHeader(ParserResult model) {
            string code = "";
            if(model.Config.HasValueFor(ConfigItemEnum.BL_DATABASE)) {
                code += $"use {model.Config.GetValue(ConfigItemEnum.BL_DATABASE)};\n\n";
            }
            return code;
        }

        ///
        /// Generierung des gesamten Codes für eine Dimensionstabelle/Interface
        ///
        public string GenerateCreateDimTableCode(Interface ifa, Config conf) {
            string code = $"create table {GetTableName(ifa, conf)} (\n";
            code += $"    {ifa.Name}_ID int primary key identity not null";            

            foreach(var attribute in ifa.Attributes) {
                code += $",\n    {GetAttributeName(attribute)} {GetAttributeType(attribute)}";
            }

            code += GetMandantSpalte(ifa);

            if(ifa.IsHistorizedInterface()) {
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
        public string GenerateCreateFactTableCode(Interface ifa, ParserResult model) {
            string code = $"create table {GetTableName(ifa, model.Config)} (\n";
            code += $"    {ifa.Name}_ID int primary key identity not null";            

            foreach(var attribute in ifa.Attributes) {
                code += $",\n    {GetAttributeName(attribute)} {GetAttributeType(attribute)}";
            }

            code += GetMandantSpalte(ifa);

            if(ifa.IsHistorizedInterface()) {
                var attr = ifa.TypeAttributes.Where(a => a.Name.Equals(InterfaceTypeAttributeEnum.HISTORY)).First();
                var attribute = model.GetBasicAttributeByName(attr.Value); // TODO: Evtl. wie zusätzliches ref-Attribut behandeln ???
                code += $",\n    -- Historisiert pro Ausprägung dieses Attributs\n    HIST_{GetAttributeName(attribute)} {GetAttributeType(attribute)}";
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
        /// Generierung des gesamten Codes für eine Definitionstabelle/Interface
        ///
        public string GenerateCreateDefTableCode(Interface ifa, Config conf) {
            string code = $"create table {GetTableName(ifa, conf)} (\n";
            code += $"    {ifa.Name}_ID int primary key identity not null"; 
            code += GetMandantSpalte(ifa);

            foreach(var attribute in ifa.Attributes) {
                code += $",\n    {GetAttributeName(attribute)} {GetAttributeType(attribute)}";
            }
            
            code += ",\n    T_Benutzer varchar(100) not null";
            code += ",\n    T_System varchar(10) not null";
            code += ",\n    T_Erst_DAT datetime not null";
            code += ",\n    T_Aend_DAT datetime not null";

            code += "\n);\n\n";
            return code;
        }

        /// 
        /// Erstellt ein Unique-Constraint für die Felder, die gemeinsam als
        /// Primary Key markiert wurden
        /// ggf. inclusive Mandant
        ///
        public string GenerateUniqueConstraint(Interface ifa, Config conf) {
            string code = $"ALTER TABLE {this.GetBLDatabaseAndSchema(conf)}[{this.GetTableName(ifa, conf)}]\n";
            code += $"ADD CONSTRAINT {this.GetTableName(ifa, conf)}_UK UNIQUE NONCLUSTERED (\n";

            int i = 0;
            if(ifa.IsMandantInterface()) {
                code += "    Mandant_KNZ ASC";
                i++;
            }

            // Offizielle Primärschlüssel-Attribute ermitteln
            var list = ifa.Attributes.Where(a => a is InterfaceBasicAttribute)
                                     .Select(a => (InterfaceBasicAttribute)a)
                                     .Where(a => a.PrimaryKey);            

            foreach(var attr in list) {                
                if(i > 0) {
                    code += ",\n";
                }
                code += $"    {this.GetAttributeName(attr)} ASC";
                i++;
            }

            if(ifa.IsHistorizedInterface()) {
                code += ",\n    [T_Gueltig_Von] ASC";   
            }

            code += "\n);\n\n";
            
            return code;
        }

    }

}