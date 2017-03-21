using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System;

namespace Kdv.CeusDL.Generator.BL {
    ///
    /// Zweck: Generieren der Load-Statements (Insert und Update)
    ///
    public class BaseLayerLoadGenerator : BaseLayerAbstractGenerator
    {
        public override string GenerateCode(ParserResult model)
        {
            return "";
        }
    }
}