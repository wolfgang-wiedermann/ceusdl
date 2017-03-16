using System.Collections.Generic;
using System.Linq;

namespace Kdv.CeusDL.Parser.Model {
    public class Config {
        public List<ConfigItem> Items = new List<ConfigItem>();

        public bool HasValueFor(ConfigItemEnum name) {
            return Items.Where(i => i.Name == name)
                        .Count() > 0;
        }

        public string GetValue(ConfigItemEnum name) {
            var result = Items.Where(i => i.Name == name);
            if(result.Count() > 0) {
                return result.First().Value;
            } else {
                return "";
            }
        }
    }
}