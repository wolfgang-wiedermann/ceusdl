using System;
using Kdv.CeusDL.Parser.Model;

namespace Kdv.CeusDL.Generator {
    public interface IGenerator {
        string GenerateCode(ParserResult model); 
    }
}