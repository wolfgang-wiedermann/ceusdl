using System.Collections.Generic;

namespace Kdv.CeusDL.Parser.TmpModel {
    public class TmpConfigItem {
        public string Name {get; set;}
        public string Value {get; set;}

        public TmpConfigItem() {
            Name = "";
            Value = "";
        }
    }
}