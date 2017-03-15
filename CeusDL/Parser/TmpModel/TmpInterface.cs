using System.Collections.Generic;

namespace Kdv.CeusDL.Parser.TmpModel
{
    internal class TmpInterface {

        public TmpInterface() {
            this.Attributes = new List<TmpInterfaceAttribute>();
            this.TypeAttributes = new List<TmpInterfaceTypeAttribute>();
        }

        public string Name {get;set;}

        ///
        /// Gültige Werte: DefTable, DimTable, DimView, FactTable
        ///
        public string Type {get;set;}
        public List<TmpInterfaceTypeAttribute> TypeAttributes {get;set;}
        public List<TmpInterfaceAttribute> Attributes {get;set;}
        
    }
}