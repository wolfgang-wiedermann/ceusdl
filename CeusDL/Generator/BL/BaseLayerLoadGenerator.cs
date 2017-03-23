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
            return "";
        }
    }
}