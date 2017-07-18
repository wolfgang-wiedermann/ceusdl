using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System;

namespace Kdv.CeusDL.Generator.BL {
    ///
    /// Zweck: Generieren der Load-Statements (Insert und Update bzw. Delete und Insert)
    ///
    /// Achtung: Noch ohne Historisierung von Attributwerten!!!
    ///
    public class BaseLayerLoadGenerator : BaseLayerAbstractGenerator
    {
        public override string GenerateCode(ParserResult model)
        {
            string code = "";
            if(model.Config.HasValueFor(ConfigItemEnum.BL_DATABASE))  {
                code += $"use {model.Config.GetValue(ConfigItemEnum.BL_DATABASE)}\n\n";
            }
            
            code += GenerateMandantParamCode();

            foreach(var ifa in model.Interfaces) {
                if(ifa.Type == InterfaceType.DIM_TABLE) {
                    code += $"-- BaseLayer Laden für DimTable {ifa.Name}\n";
                    code += GenerateDimUpdateCode(ifa, model);
                    code += GenerateDimInsertCode(ifa, model);  
                } else if(ifa.Type == InterfaceType.FACT_TABLE) {                    
                    code += $"-- BaseLayer Laden für FactTable {ifa.Name}\n";
                    code += GenerateFactDeleteCode(ifa, model);
                    // Achtung: Hier wird auch die Version von Dim verwendet, da das bist dato gleich ist!
                    code += GenerateDimInsertCode(ifa, model);                     
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

            code += ",\n    T_Modifikation, T_Ladelauf_NR, T_Benutzer, T_System, T_Erst_DAT, T_Aend_DAT, T_Bemerkung";
            code += "\n)\nselect distinct\n";            

            if(ifa.IsMandantInterface()) {
                code += "    Mandant_KNZ";
            }
            foreach(var attr in ifa.Attributes) {                
                if(!(attr == ifa.Attributes.First() && !ifa.IsMandantInterface())) {
                    code += ",\n";
                }
                code += $"    {GetAttributeName(attr)}";
            }   
            
            code += ",\n    T_Modifikation, @ladelauf_nr, SYSTEM_USER, 'H', GetDate(), GetDate(), 'Insert bei Load BL'";

            code += $"\nfrom {GetViewName(ifa, model.Config)}\n";
            code += "where T_Modifikation = 'I'";

            if(ifa.IsMandantInterface()) {
                code += " and Mandant_KNZ = @mandant\n";
            }

            code += "\n\n";
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
            code += ",\n    t.T_Modifikation = 'U', t.T_Bemerkung = 'Update bei Load BL', t.T_Benutzer = SYSTEM_USER, t.T_Aend_Dat = GetDate()";
            code += ",\n    t.T_Ladelauf_NR = @ladelauf_nr";
            code += $"\nfrom {GetTableName(ifa, model.Config)} as t inner join {GetViewName(ifa, model.Config)} as v ";            
            code += $"\non t.{ifa.Name}_ID = v.{ifa.Name}_ID";
            code += $"\nwhere v.T_Modifikation = 'U'";
            if(ifa.IsMandantInterface()) {
                code += " and t.Mandant_KNZ = @mandant\n";
            }
            code += "\n\n";
            return code;
        }

        ///
        /// Löschen evtl. zu ersetzender Fakten 
        ///
        private string GenerateFactDeleteCode(Interface ifa, ParserResult model)
        {
            string code = $"delete from [dbo].[{GetTableName(ifa, model.Config)}]\n";

            if(ifa.IsMandantInterface()) {
                code += "where Mandant_KNZ = @mandant\n";
            } 

            if(ifa.IsHistorizedInterface()) {
                if(ifa.IsMandantInterface()) {
                    code += "and ";
                } else {
                    code += "where ";
                }
                
                code += $"{GetAttributeName(ifa.GetHistoryAttribute())} in (\n";
                code += $"  select distinct {GetAttributeName(ifa.GetHistoryAttribute())}\n";
                code += $"  from [dbo].[{GetViewName(ifa, model.Config)}]\n";
                if(ifa.IsMandantInterface()) {
                    code += "  where Mandant_KNZ = @mandant\n";
                } 
                code += ")\n\n";
            }
            return code;
        }

        private string GenerateMandantParamCode()
        {
            string code = "-- DECLARE @mandant varchar(10);\n";
            code += "-- DECLARE @ladelauf_nr int;\n";
            code += "-- SET @mandant = '0000';\n";
            code += "-- SET @ladelauf_nr = 1;\n\n";
            return code;
        }
    }
}