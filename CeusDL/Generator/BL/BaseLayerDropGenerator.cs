using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System;

namespace Kdv.CeusDL.Generator.BL {

    ///
    /// Neben der einfachen Tabelle sollte auch noch ein Unique-Key für 
    /// die PK-Attribute generiert werden (PK-Attribute sind in BL ja nicht mehr der
    /// datebankseitige Primärschlüssel)
    ///
    public class BaseLayerDropGenerator : BaseLayerAbstractGenerator
    {
        public override string GenerateCode(ParserResult model)
        {
            return "--TODO: Programmieren...";
        }
    }
}