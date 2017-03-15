using System.Collections.Generic;
using System.Linq;

namespace Kdv.CeusDL.Parser.Model {
    public class Interface {
        public string Name {get;set;}
        public InterfaceType Type {get;set;}
        public List<InterfaceTypeAttribute> TypeAttributes {get;set;}
        public List<InterfaceAttribute> Attributes {get;set;}

        public Interface() {
            this.Attributes = new List<InterfaceAttribute>();
            this.TypeAttributes = new List<InterfaceTypeAttribute>();
        }  

        public bool IsMandantInterface() {
            var result = TypeAttributes.Where(a => a.Name.Equals(InterfaceTypeAttributeEnum.MANDANT));
            return result.Count() > 0 && result.First().Value.ToLower() == "true";
        }

        public bool IsHistorizedInterface() {
            var result = TypeAttributes.Where(a => a.Name.Equals(InterfaceTypeAttributeEnum.HISTORY));
            return result.Count() > 0;
        }

        public override string ToString() {
            string str = $"Interface\nName: {Name}\nTyp: {Type}\n";
            str += "Typ-Attribute: ";
            foreach(var ta in TypeAttributes) {
                str += $"{ta.Name} -> {ta.Value}, ";
            }
            str += "\nAttribute:";
            foreach(var attr in Attributes) {
                str += attr.ToString();
            }
            str += "\n";
            return str;
        } 
    }
}