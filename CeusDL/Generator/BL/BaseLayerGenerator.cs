using Kdv.CeusDL.Parser.Model;
using System.Linq;

namespace Kdv.CeusDL.Generator.BL {
    public class BaseLayerGenerator : IGenerator {
        
        public string GenerateCode(ParserResult model) {
            var tab = new BaseLayerTableGenerator();
            var vie = new BaseLayerViewGenerator();

            return tab.GenerateCode(model) + vie.GenerateCode(model);
        }
    }
}