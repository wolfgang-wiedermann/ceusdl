using Kdv.CeusDL.Parser.Model;
using Kdv.CeusDL.Generator.IL;
using System.Linq;
using System;

namespace Kdv.CeusDL.Generator.BL {
    public class BaseLayerViewGenerator : BaseLayerAbstractGenerator
    {
        public override string GenerateCode(ParserResult model)
        {
            string code = "--\n-- BaseLayer Views\n--\n\n";
            foreach(var obj in model.Interfaces) {
                if(obj.Type == InterfaceType.DIM_TABLE) {
                    code += GenerateCreateViewCode(obj, model.Config);
                } else if(obj.Type == InterfaceType.FACT_TABLE) {                    
                    code += GenerateCreateFactViewCode(obj, model.Config);
                }                 
            }
            return code;
        }

        ///
        /// Generierung des gesamten Codes für eine Tabelle/Interface
        ///
        /// Dimmensionstabllen
        ///
        public string GenerateCreateViewCode(Interface ifa, Config conf) {
            var il = new InterfaceLayerGenerator();
            string code = $"go\ncreate view {GetViewName(ifa, conf)} as \n";
            code += $"select bl.{ifa.Name}_ID";

            if(ifa.IsMandantInterface()) {                
                code += ",\n    il.Mandant_KNZ as Mandant_KNZ";
            }
            foreach(var attr in ifa.Attributes) {
                code += $",\n    il.{GetILAttributeName(attr)} as {GetAttributeName(attr)}";
            }

            code += GetTModifikation(ifa);

            code += $"\nfrom {GetILDatabaseAndSchema(conf)}{GetPrefix(conf)}IL_{ifa.Name} as il\n";
            code += $"    left outer join {GetBLDatabaseAndSchema(conf)}{GetTableName(ifa, conf)} as bl\n";
            code += $"    on il.{GetILPKField(ifa)} = bl.{GetBLPKField(ifa)}";

            if(ifa.IsMandantInterface()) {                
                code += "\n        and il.Mandant_KNZ = bl.Mandant_KNZ";
            }

/* 
            if(ifa.IsHistorizedInterface()) {
                // TODO: Wie reagiere ich bei unterschiedlicher Granularität 
                //       zwischen T_Gueltig_Von/T_Gueltig_Bis und dem HistoryAttribute 
                var timeAttribute = ifa.GetHistoryAttribute();
                if(timeAttribute == null) {
                    code += "\n        -- ERROR: INVALID TIME AND HISTORY ATTRIBUTE";
                } else {
                    code += $"\n        and (il.{GetILAttributeName(timeAttribute)} >= bl.T_Gueltig_Von";
                    code += $"\n        and il.{GetILAttributeName(timeAttribute)} < bl.T_Gueltig_Bis)";
                }
            }
*/ // TODO: Das mit der Historisierung hier ist noch unfertiger sch...            

            code += ";\n\n";
            return code;
        }

        ///
        /// Generierung des gesamten Codes für eine Tabelle/Interface
        ///
        /// Faktentabelle
        ///
        public string GenerateCreateFactViewCode(Interface ifa, Config conf) {
            var il = new InterfaceLayerGenerator();
            string code = $"go\ncreate view {GetViewName(ifa, conf)} as \n";
            code += $"select null as {ifa.Name}_ID";

            if(ifa.IsMandantInterface()) {                
                code += ",\n    il.Mandant_KNZ as Mandant_KNZ";
            }
            foreach(var attr in ifa.Attributes) {
                code += $",\n    il.{GetILAttributeName(attr)} as {GetAttributeName(attr)}";
            }

            code += ",\n    'I' as T_Modifikation";

            code += $"\nfrom {GetILDatabaseAndSchema(conf)}{GetPrefix(conf)}IL_{ifa.Name} as il;\n\n";
            return code;
        }
                

        ///
        /// case 
        ///     when bl.Bewerberstatus_ID is null then 'I'
        ///     when bl.Bewerberstatus_KURZBEZ <> il.Bewerberstatus_KURZBEZ 
        ///       or bl.Bewerberstatus_LANGBEZ <> il.Bewerberstatus_LANGBEZ then 'U'
        ///     else 'X' 
        /// end as T_Modifikation	
        ///
        private string GetTModifikation(Interface ifa) {
            // wird bei Faktentabellen nicht verwendet!
            if(ifa.Type == InterfaceType.FACT_TABLE) {
                return "";
            }

            string code = ",\n    case";
            code += $"\n        when bl.{ifa.Name}_ID is null then 'I'";

            // alle Felder, die nicht PK sind hier berücksichtigen
            var fields = ifa.Attributes.Where(a => a is InterfaceBasicAttribute)
                                       .Select(a => (InterfaceBasicAttribute)a)
                                       .Where(a => a.PrimaryKey == false)
                                       .ToList<InterfaceBasicAttribute>();
            // Evtl. fehlen hier noch die InterfaceRefAttribute => die könnten auch relevant sein!

            if(fields.Count > 0) {
                code += "\n        when ";
            }
            int i = 0; 
            foreach(var field in fields) {
                i++;
                code += $"bl.{GetAttributeName(field)} <> il.{GetILAttributeName(field)}";
                if(i < fields.Count) {
                    code += "\n          or ";
                }
            }
            if(fields.Count > 0) {
                code += " then 'U'";
            }

            code += "\n        else 'X'";
            code += "\n    end as T_Modifikation";

            return code;
        }
    }
}