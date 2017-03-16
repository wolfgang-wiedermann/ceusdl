using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;

namespace Kdv.CeusDL.Parser.Model
{
    public class ParserResult {
        public List<Interface> Interfaces = new List<Interface>();
        public Config Config = new Config();

        public Interface GetInterfaceByName(string name) {
            var result = this.Interfaces.Where(i => i.Name.Equals(name));
            if(result.Count() == 1) {
                return result.First<Interface>();
            } else {
                return null;
            }
        }

        ///
        /// Ermittelt ein Attribut anhand seines vollständigen Namens
        /// z. B. Bewerber.KNZ
        ///
        public InterfaceAttribute GetBasicAttributeByName(string name) {
            var expr = new Regex("^[a-zA-Z\\-_]+\\.[a-zA-Z\\-_]+$");
            if(!expr.Match(name).Success) {
                throw new InvalidTokenException($"Der Attributname {name} ist syntaktisch falsch");
            }

            var s = name.Split('.');
            var ifaName = s[0];
            var attName = s[1];

            var ifa = GetInterfaceByName(ifaName);
            var result = ifa.Attributes
                            .Where(a => a is InterfaceBasicAttribute)
                            .Select(a => (InterfaceBasicAttribute)a)
                            .Where(a => a.Name.Equals(attName));

            if(result.Count() == 0) {
                throw new InvalidOperationException($"Attribut {name} nicht gefunden");
            }

            return result.First();
        }

        public override string ToString() {
            string str = "";
            foreach(var ifa in Interfaces) {
                str += ifa.ToString();
            }
            return str;
        }
    }
}