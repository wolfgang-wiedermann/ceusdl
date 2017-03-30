/*

-- Beispiel für ein mögliches generiertes Load-Skript zu einer DimTable, DimView oder FactTable ...

use FH_AP_BaseLayer

truncate table [dbo].[AP_BT_D_Land]

insert into [dbo].[AP_BT_D_Land] (
       [Land_ID]
      ,[Land_KNZ]
      ,[Land_KURZBEZ]
      ,[Land_LANGBEZ]
      ,[Land_Laengengrad]
      ,[Land_Breitengrad]
-- Interessant:
	  ,Kontinent_ID 
      ,[Kontinent_KNZ]
-- /
      ,[T_Modifikation]
      ,[T_Bemerkung]
      ,[T_Benutzer]
      ,[T_System]
      ,[T_Erst_DAT]
      ,[T_Aend_DAT]
      ,[T_Ladelauf_NR]
)
SELECT [Land_ID]
      ,[Land_KNZ]
      ,[Land_KURZBEZ]
      ,[Land_LANGBEZ]
      ,[Land_Laengengrad]
      ,[Land_Breitengrad]
-- Interessant:
	  ,cKontinent.Kontinent_ID as Kontinent_ID
      ,b.[Kontinent_KNZ]
-- /
      ,b.[T_Modifikation]
      ,b.[T_Bemerkung]
      ,b.[T_Benutzer]
      ,b.[T_System]
      ,b.[T_Erst_DAT]
      ,b.[T_Aend_DAT]
      ,b.[T_Ladelauf_NR]
  FROM [FH_AP_BaseLayer].[dbo].[AP_BL_D_Land] as b
  -- Interessant:
  inner join FH_AP_BaseLayer.dbo.AP_BL_D_Kontinent as cKontinent 
  on b.Kontinent_KNZ = cKontinent.Kontinent_KNZ
  -- /

 */

using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Kdv.CeusDL.Generator.BT {
    public class BaseLayerTransLoadGenerator : BaseLayerTransAbstractGenerator
    {        
        public override string GenerateCode(ParserResult model) {
              string code = "-- BT Load Skript \n\n";
              code += GetUseStatement(model);
              foreach(var ifa in model.Interfaces.Where(i => i.Type != InterfaceType.DEF_TABLE )) {
                    code += GetTruncateStatement(ifa, model.Config);
                    code += GetLoadStatement(ifa, model);
              }
              return code;
        }

        private string GetLoadStatement(Interface ifa, ParserResult model)
        {
            string code = "\n";
            code += $"insert into [dbo].[{GetTableName(ifa, model.Config)}] (\n";
            code += GetInsertFieldList(ifa, model);
            code += "\n)\n";
            code += "select\n";
            code += GetSelectFieldList(ifa, model);
            code += $"\nfrom [dbo].[{blGenerator.GetTableName(ifa, model.Config)}] as b\n";
            code += GetJoinClause(ifa, model);
            code += "\n\n";

            return code;
        }

        ///
        /// Generiert die Feldliste für den Select-Clause
        ///
        private string GetInsertFieldList(Interface ifa, ParserResult model)
        {
            string code = "";
            code += $"    {ifa.Name}_ID";

            foreach(var attr in ifa.Attributes) {
                if(attr is InterfaceBasicAttribute) {
                    code += $",\n    {blGenerator.GetAttributeName(attr)}";
                } else if (attr is InterfaceRefAttribute) {
                    var r = (InterfaceRefAttribute)attr;
                    if(string.IsNullOrEmpty(r.Alias)) {
                        code += $",\n    {r.ReferencedAttribute.ParentInterface.Name}_ID";
                    } else {
                        code += $",\n    {r.Alias}_{r.ReferencedAttribute.ParentInterface.Name}_ID";
                    }
                    code += $",\n    {blGenerator.GetAttributeName(attr)}";
                } else {
                      throw new NotImplementedException();
                }
            }

            if(ifa.IsMandantInterface()) {
                code += ",\n    Mandant_ID";
            }

//            if(ifa.IsHistorizedInterface()) TODO: ggf. noch nötig ...

            code += ",\n    T_Modifikation";
            code += ",\n    T_Bemerkung";
            code += ",\n    T_Benutzer";
            code += ",\n    T_System";
            code += ",\n    T_Erst_DAT";
            code += ",\n    T_Aend_DAT";
            code += ",\n    T_Ladelauf_NR";
            return code;
        }

        ///
        /// Generiert die Feldliste für den Select-Clause
        ///
        private string GetSelectFieldList(Interface ifa, ParserResult model)
        {
            string code = "";
            code += $"    b.{ifa.Name}_ID";

            int i = 0;
            foreach(var attr in ifa.Attributes) {
                if(attr is InterfaceBasicAttribute) {
                    code += $",\n    b.{blGenerator.GetAttributeName(attr)}";
                } else if (attr is InterfaceRefAttribute) {                    
                    var r = (InterfaceRefAttribute)attr;
                    code += $",\n    isnull(a{i}.{r.ReferencedAttribute.ParentInterface.Name}_ID, -1) as {r.ReferencedAttribute.ParentInterface.Name}{i}_ID";                    
                    code += $",\n    b.{blGenerator.GetAttributeName(attr)}";
                    i++;
                } else {
                    throw new NotImplementedException();
                }
            }

            if(ifa.IsMandantInterface()) {
                  code += ",\n    cast(b.Mandant_KNZ as int) as Mandant_ID";
            }

            code += ",\n    b.T_Modifikation";
            code += ",\n    b.T_Bemerkung";
            code += ",\n    b.T_Benutzer";
            code += ",\n    b.T_System";
            code += ",\n    b.T_Erst_DAT";
            code += ",\n    GetDate()";
            code += ",\n    b.T_Ladelauf_NR";
            return code;
        }

        ///
        /// Baut den erforderlichen Join für das Select-Statement auf
        /// (!!! Funktioniert nur mit Tabellen mit Einelementigem Primärschlüssel !!!)
        ///
        private string GetJoinClause(Interface ifa, ParserResult model)
        {
            string code = "";
            int i = 0;
            foreach(var attr in ifa.Attributes) {
                if (attr is InterfaceRefAttribute) {                    
                    var r = (InterfaceRefAttribute)attr;
                    code += $"left outer join [dbo].[{blGenerator.GetTableName(r.ReferencedAttribute.ParentInterface, model.Config)}] as a{i}\n    on ";
                    if(r.ReferencedAttribute.ParentInterface.IsMandantInterface()) {
                        code += $"b.Mandant_KNZ = a{i}.Mandant_KNZ\n    and ";
                    }                    
                    code += $"b.{blGenerator.GetAttributeName(attr)} = a{i}.{blGenerator.GetAttributeName(r.ReferencedAttribute)}\n";                    
                    i++;
                }                
            }
            return code;
        }

        ///
        /// Truncate Table Statement zum Löschen aller Inhalte vor dem Laden
        ///
        public string GetTruncateStatement(Interface ifa, Config conf) {
              return $"truncate table [dbo].[{GetTableName(ifa, conf)}]\n\n";
        }
    }
}