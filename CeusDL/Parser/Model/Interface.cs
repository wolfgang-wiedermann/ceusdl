using System.Collections.Generic;
using System.Linq;

namespace Kdv.CeusDL.Parser.Model {
    public class Interface {
        public string Name {get;set;}
        public InterfaceType Type {get;set;}
        public List<InterfaceTypeAttribute> TypeAttributes {get;set;}
        public List<InterfaceAttribute> Attributes {get;set;}

        public InterfaceAttribute GetHistoryAttribute() {
            if(!this.IsHistorizedInterface()) {
                return null;
            } else {
                var fieldName = TypeAttributes.Where(a => a.Name == InterfaceTypeAttributeEnum.HISTORY)
                                              .First().Value;
                return GetAttributeByName(fieldName);
            }
        }

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

        public InterfaceAttribute GetAttributeByName(string fieldName) {
            if(fieldName.Contains(".")) {
                // Feld ist InterfaceRefAttribute
                var splitResult = fieldName.Split('.');
                var typeName = splitResult[0];
                var selectedFieldName = splitResult[1];

                var attr = Attributes.Where(a => a is InterfaceRefAttribute)
                                    .Select(a => (InterfaceRefAttribute)a)
                                    .Where(a => a.ReferencedTypeName == typeName 
                                                && a.ReferencedFieldName == selectedFieldName)
                                    .ToList<InterfaceAttribute>();

                if(attr.Count > 0)
                    return attr.First();
                else
                    return null;
            } else {
                // Feld ist InterfaceBaseAttribute ...
                var res1 = Attributes.Where(a => a is InterfaceRefAttribute)
                                     .Select(a => (InterfaceBasicAttribute)a)
                                     .Where(a => a.Name == fieldName);

                if(res1.Count() > 0) {
                    return res1.First();
                }

                // ... oder Alias bei InterfaceRefAttribute
                var res2 = Attributes.Where(a => a is InterfaceRefAttribute)
                                     .Select(a => (InterfaceRefAttribute)a)
                                     .Where(a => a.Alias == fieldName);
                
                if(res2.Count() > 0) {
                    return res2.First();
                } else {
                    return null;
                }
            }
        }
    }
}