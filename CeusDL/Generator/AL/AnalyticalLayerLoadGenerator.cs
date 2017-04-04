/*

SQL zum Laden einer Faktentabelle:

select a.[Antrag_ID]
      ,a.[Mandant_ID]
      ,a.[Antrag_Antragsnummer]
      ,a.[Antrag_Liefertag]
      ,a.[Antrag_Anzahl_F]
      ,t1.[Bewerber_ID]
      ,t1.[Bewerber_Bewerbernummer]
      ,t1.[Bewerber_Liefertag]
      ,t1.[Geschlecht_ID]
      ,t1.[Semester_ID]
      ,t1.[HochschulSemester_ID]
      ,t1.[FachSemester_ID]
      ,t1.[HZBArt_ID]
      ,t1.[HZBNote_ID]
      ,t1.[HZB_Kreis_ID]
      ,t1.[Heimatort_Kreis_ID]
      ,t1.[Staatsangehoerigkeit_Land_ID]
      ,a.[StudiengangHisInOne_ID]
from FH_AP_BaseLayer.dbo.AP_BT_F_Antrag a
inner join FH_AP_BaseLayer.dbo.AP_BT_F_Bewerber t1
on a.Bewerber_ID = t1.Bewerber_ID

 */

using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Kdv.CeusDL.Generator.AL {
    public class AnalyticalLayerLoadGenerator : AnalyticalLayerAbstractGenerator
    {        
        public override string GenerateCode(ParserResult model) {  
              return "";
        }
    }
}