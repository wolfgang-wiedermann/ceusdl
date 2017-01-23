using System.Collections.Generic;

namespace Kdv.CeusDL.Parser.TmpModel
{
    internal class TmpParserResult {

        public TmpParserResult() {
            this.Interfaces = new List<TmpInterface>();
        }
        public List<TmpInterface> Interfaces {get;set;}
        // TODO: hier gehts weiter !!!
    }
}