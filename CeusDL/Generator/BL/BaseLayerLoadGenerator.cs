using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System;

namespace Kdv.CeusDL.Generator.BL {
    ///
    /// Zweck: Generieren der Load-Statements (Insert und Update)
    /// Beispiel für Insert und Update bei nicht historiserter nicht mandantenorientierter DimTable:
    ///    
    /// --
    /// -- Insert-Statement
    /// --
    /// insert into AP_BL_D_Bewerberstatus (
    ///	    Bewerberstatus_KNZ, Bewerberstatus_KURZBEZ, Bewerberstatus_LANGBEZ, Bewerberstatus_HISKEY_ID, 
    /// 	T_Modifikation, T_Ladelauf_NR, T_Benutzer, T_System, T_Erst_DAT, T_Aend_DAT
    /// )
    /// select Bewerberstatus_KNZ, Bewerberstatus_KURZBEZ, Bewerberstatus_LANGBEZ, Bewerberstatus_HISKEY_ID, 
    ///	   T_Modifikation, 1, SYSTEM_USER, 'H', GetDate(), GetDate()
    /// from AP_BL_D_Bewerberstatus_VW
    /// where T_Modifikation = 'I'
    ///
    /// --
    /// -- Update-Statement (Aktualisiert genau die Felder, die im Case für U geprüft werden!)
    /// --
    /// update b
    /// set b.Bewerberstatus_KURZBEZ = v.Bewerberstatus_KURZBEZ,
    /// b.Bewerberstatus_LANGBEZ = v.Bewerberstatus_LANGBEZ,
    ///	b.Bewerberstatus_HISKEY_ID = v.Bewerberstatus_HISKEY_ID
    /// from AP_BL_D_Bewerberstatus as b inner join AP_BL_D_Bewerberstatus_VW as v
    /// on b.Bewerberstatus_ID = v.Bewerberstatus_ID
    /// where v.T_Modifikation = 'U'
    ///
    public class BaseLayerLoadGenerator : BaseLayerAbstractGenerator
    {
        public override string GenerateCode(ParserResult model)
        {
            string code = "";
            if(model.Config.HasValueFor(ConfigItemEnum.BL_DATABASE))  {
                code += $"use {model.Config.GetValue(ConfigItemEnum.BL_DATABASE)}\n\n";
            }
            foreach(var ifa in model.Interfaces) {
                if(ifa.Type == InterfaceType.DIM_TABLE) {
                    code += $"-- BaseLayer Laden für DimTable {ifa.Name}\n";
                    code += GenerateDimUpdateCode(ifa, model);
                    code += GenerateDimInsertCode(ifa, model);  
                } else if(ifa.Type == InterfaceType.FACT_TABLE) {
                    // TODO: noch zu implementieren
                }
            }
            return code;
        }

        public string GenerateDimInsertCode(Interface ifa, ParserResult model) {
            string code = $"insert into {GetTableName(ifa, model.Config)} (\n";

            if(ifa.IsMandantInterface()) {
                code += "    Mandant_KNZ";
            }
            foreach(var attr in ifa.Attributes) {                
                if(!(attr == ifa.Attributes.First() && !ifa.IsMandantInterface())) {
                    code += ",\n";
                }
                code += $"    {GetAttributeName(attr)}";
            }            

            code += ",\n    T_Modifikation, T_Ladelauf_NR, T_Benutzer, T_System, T_Erst_DAT, T_Aend_DAT";
            code += "\n)\nselect\n";            

            if(ifa.IsMandantInterface()) {
                code += "    Mandant_KNZ";
            }
            foreach(var attr in ifa.Attributes) {                
                if(!(attr == ifa.Attributes.First() && !ifa.IsMandantInterface())) {
                    code += ",\n";
                }
                code += $"    {GetAttributeName(attr)}";
            }   

            // TODO: 1 als Ladelaufnummer ist natürlich noch Sch...
            code += ",\n    T_Modifikation, 1, SYSTEM_USER, 'H', GetDate(), GetDate()";

            code += $"\nfrom {GetViewName(ifa, model.Config)}\n";
            code += "where T_Modifikation = 'I'\n\n";
            return code;
        }

        public string GenerateDimUpdateCode(Interface ifa, ParserResult model) {
            string code = "update t set ";
            int i = 0;
            foreach(var attr in ifa.Attributes) {
                if(!(attr is InterfaceBasicAttribute && ((InterfaceBasicAttribute)attr).PrimaryKey)) {
                    if(i > 0) {
                        code += ",";
                    }
                    code += $"\n    t.{GetAttributeName(attr)} = v.{GetAttributeName(attr)}";                    
                    i++;
                }
            }
            code += $"\nfrom {GetTableName(ifa, model.Config)} as t inner join {GetViewName(ifa, model.Config)} as v ";            
            code += $"\non t.{ifa.Name}_ID = v.{ifa.Name}_ID";
            code += $"\nwhere v.T_Modifikation = 'U'";
            code += "\ngo\n\n";
            return code;
        }
    }
}