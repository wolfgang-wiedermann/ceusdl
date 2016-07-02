using System.Collections.Generic;

namespace Kdv.CeusDL.Parser.Model
{
    public class ParserResult {
        public List<Interface> Interfaces = new List<Interface>();

        public override string ToString() {
            string str = "";
            foreach(var ifa in Interfaces) {
                str += ifa.ToString();
            }
            return str;
        }
    }
}