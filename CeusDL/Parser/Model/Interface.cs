using System.Collections.Generic;

namespace Kdv.CeusDL.Parser.Model {
    public class Interface {
        public string Name {get;set;}
        public List<InterfaceAttribute> Attributes {get;set;}

        public Interface() {
            this.Attributes = new List<InterfaceAttribute>();
        }  

        public override string ToString() {
            string str = $"Interface\nName: {Name}\n";
            foreach(var attr in Attributes) {
                str += attr.ToString();
            }
            return str;
        } 
    }
}