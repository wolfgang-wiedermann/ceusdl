using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System;

namespace Kdv.CeusDL.Generator.BT {
    public abstract class BaseLayerTransAbstractGenerator : IGenerator
    {
        public abstract string GenerateCode(ParserResult model);
    }
}