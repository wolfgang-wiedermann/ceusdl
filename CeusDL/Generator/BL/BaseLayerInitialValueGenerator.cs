using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System;
using System.Text;

namespace Kdv.CeusDL.Generator.BL {
    ///
    /// Zweck: Generieren der Initialwerte als Insert-Statements (-1 Unbekannt etc.)    
    ///
    public class BaseLayerInitialValueGenerator : BaseLayerAbstractGenerator
    {
        public override string GenerateCode(ParserResult model) 
        {
            StringBuilder sql = new StringBuilder("");            
            foreach(Interface ifa in model.Interfaces.Where(i => i.Type != InterfaceType.DIM_VIEW && i.Type != InterfaceType.FACT_TABLE)) {
                sql.Append(GenerateDefaultInsert(ifa, model));
                sql.Append("\n\n");
            }
            return sql.ToString();
        }

        public string GenerateDefaultInsert(Interface ifa, ParserResult model) 
        {
            string sql = $"set identity_insert {GetBLDatabaseAndSchema(model.Config)}{GetTableName(ifa, model.Config)} on;\n";
            sql += $"insert into {GetBLDatabaseAndSchema(model.Config)}{GetTableName(ifa, model.Config)} (\n";
            // Attributliste aufbauen
            sql += $"    {ifa.Name}_ID";
            if(ifa.IsMandantInterface()) {
                sql += ", Mandant_KNZ";
            }
            foreach(var attr in ifa.Attributes) {
                sql += $", {this.GetAttributeName(attr)}";
            }
            if(ifa.Type == InterfaceType.DIM_TABLE) {
                sql += "\n    , T_Modifikation, T_Bemerkung, T_Benutzer, T_System, T_Erst_DAT, T_Aend_DAT, T_Ladelauf_NR\n";
            } else if(ifa.Type == InterfaceType.DEF_TABLE) {
                sql += "\n    , T_Benutzer, T_System, T_Erst_DAT, T_Aend_DAT\n";
            }
            sql += "\n) values (\n";
            // Werteliste aufbauen
            sql += "    -1";
            if(ifa.IsMandantInterface()) {
                sql += ", '-1'";
            }
            foreach(var attr in ifa.Attributes) {
                if(attr is InterfaceBasicAttribute) {
                    var basic = (InterfaceBasicAttribute)attr;
                    switch(basic.DataType) {
                        case InterfaceAttributeDataType.VARCHAR:
                            if((new string[2]{"KURZBEZ", "LANGBEZ"}).Contains(basic.Name)) {
                                sql += ", 'UNBEKANNT'";
                            } else {
                                sql += ", '-1'";
                            }
                            break;
                        case InterfaceAttributeDataType.DECIMAL:
                            sql += ", -1.0";
                            break;
                        case InterfaceAttributeDataType.INT:
                            sql += ", -1";
                            break;
                        default:
                            throw new NotImplementedException("BaseLayerInitialValueGenerator: Ungültiger Datentyp des Attributs");
                    }
                } else if(attr is InterfaceRefAttribute) {
                    var reference = (InterfaceRefAttribute)attr;
                    switch(reference.ReferencedAttribute.DataType) {
                        case InterfaceAttributeDataType.VARCHAR:
                            sql += ", '-1'";
                            break;
                        case InterfaceAttributeDataType.DECIMAL:
                            sql += ", -1.0";
                            break;
                        case InterfaceAttributeDataType.INT:
                            sql += ", -1";
                            break;
                        default:
                            throw new NotImplementedException("BaseLayerInitialValueGenerator: Ungültiger Datentyp des referenzierten Attributs");                            
                    }
                }
            }
            if(ifa.Type == InterfaceType.DIM_TABLE) {
                sql += "\n    , 'I', 'Default Wert', 'INITIAL', 'D', GetDate(), GetDate(), 0\n";
            } else if(ifa.Type == InterfaceType.DEF_TABLE) {
                sql += "\n    , 'INITIAL', 'D', GetDate(), GetDate()\n";
            }
            sql += "\n)\n";
            sql += $"set identity_insert {GetBLDatabaseAndSchema(model.Config)}{GetTableName(ifa, model.Config)} off;\n";
            return sql;
        }
    }
}