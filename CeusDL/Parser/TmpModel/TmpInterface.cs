using System.Collections.Generic;

namespace Kdv.CeusDL.Parser.TmpModel
{
    internal class TmpInterface {

        public TmpInterface() {
            this.Attributes = new List<TmpInterfaceAttribute>();
        }
        public string Name {get;set;}

        ///
        /// Gültige Werte: AttributTable, AttributView, FactTable
        ///
        public string Type {get;set;}
        public List<TmpInterfaceAttribute> Attributes {get;set;}
        
    }
}