using System;
using System.Linq;
using System.Collections.Generic;
using Kdv.CeusDL.Parser.Model;

namespace Kdv.CeusDL.Generator.AL {
    public class DerivedInterfaceBasicAttribute : InterfaceBasicAttribute {
        // Basis-Attribut aus dem das Abgeleitete AL-Attribut generiert wurde...
        // Kann ein beliebiges InterfaceAttribute sein !!!
        public InterfaceAttribute BaseAttribute {get; set;}

        // Basistabelle von der aus die Referenz aufgelöst werden soll, wenn
        // eine Dimension eingeschachtelt wird.
        public Interface ReferenceBase {get; set;}
    }
}