/*
 * Idee: Faktentabellen erstmal Datenseitig aufbauen und dann so im Generierungsprozess verwenden
 */

using System;
using System.Linq;
using System.Collections.Generic;
using Kdv.CeusDL.Parser.Model;

namespace Kdv.CeusDL.Generator.AL {
    internal class AnalyticalAbstractTable {        
        internal string Name { get; set; }
        internal Interface MainInterface {get;set;}
        internal List<InterfaceAttribute> Attributes = new List<InterfaceAttribute>();
        private Dictionary<string, InterfaceAttribute> AttributeDict = new Dictionary<string, InterfaceAttribute>();

        protected void Add(InterfaceBasicAttribute attr) {
            if(!AttributeDict.ContainsKey(attr.Name)) {
                AttributeDict.Add(attr.Name, attr);
                Attributes.Add(attr);
            }
        }
    }
}