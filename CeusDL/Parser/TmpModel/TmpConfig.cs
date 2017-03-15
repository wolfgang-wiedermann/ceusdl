using System.Collections.Generic;

namespace Kdv.CeusDL.Parser.TmpModel {
    public class TmpConfig {
        public List<TmpConfigItem> Items {get; set;}

        public TmpConfig() {
            Items = new List<TmpConfigItem>();
        }
    }
}