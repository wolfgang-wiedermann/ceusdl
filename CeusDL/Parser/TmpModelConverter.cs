using System;
using System.Collections.Generic;
using Kdv.CeusDL.Parser.TmpModel;
using Kdv.CeusDL.Parser.Model;
using System.Linq;

namespace Kdv.CeusDL.Parser {
    internal class TmpModelConverter {

        public TmpParserResult Input {get;}
        public Dictionary<string, TmpInterface> TmpInterfaceDict {get;}
        public Dictionary<string, Dictionary<string, TmpInterfaceAttribute>> TmpInterfaceAttributeDict {get;}

        public TmpModelConverter(TmpParserResult input) {
            this.Input = input;
            this.TmpInterfaceDict = new Dictionary<string, TmpInterface>();
            this.TmpInterfaceAttributeDict = new Dictionary<string, Dictionary<string, TmpInterfaceAttribute>>();

            foreach(var ifa in input.Interfaces) {
                TmpInterfaceDict.Add(ifa.Name, ifa);
                TmpInterfaceAttributeDict.Add(ifa.Name, new Dictionary<string, TmpInterfaceAttribute>());
                var dict = TmpInterfaceAttributeDict[ifa.Name];
                foreach(var attr in ifa.Attributes) {
                    if(attr.Name != null) {
                        dict.Add(attr.Name, attr);
                    }
                }
            }
        }

        internal ParserResult ToParserResult() {
            var result = new ParserResult();
            // Konfiguration (falls Vorhanden) einlesen
            result.Config = new Config();
            foreach(var tmpConfItem in Input.Config.Items) {
                result.Config.Items.Add(ConfigItem.Convert(tmpConfItem));
            }
            // Basis übersetzen
            foreach(var tmpIfa in Input.Interfaces) {
                var ifa = ToInterface(tmpIfa);
                result.Interfaces.Add(ifa);
            }
            // Referenzen auflösen
            foreach(var ifa in result.Interfaces) {
                var list = ifa.Attributes.Where(a => a is InterfaceRefAttribute)
                                         .Select(a => (InterfaceRefAttribute)a)
                                         .ToList<InterfaceRefAttribute>();
                foreach(var r in list) {
                    r.ReferencedAttribute = result.Interfaces
                          .Where(a => a.Name.Equals(r.ReferencedTypeName))
                          .Single().Attributes
                          .Where(a => a is InterfaceBasicAttribute)
                          .Select(a => (InterfaceBasicAttribute)a)
                          .Where(a => a.Name.Equals(r.ReferencedFieldName))
                          .Single();
                }
            }
            return result;
        }

        private Interface ToInterface(TmpInterface input) {
            var ifa = new Interface();
            ifa.Name = input.Name;
            ifa.Type = InterfaceTypeResolver.Get(input.Type);
            foreach(var tmpAttr in input.TypeAttributes) {
                var attr = ToInterfaceTypeAttribute(tmpAttr);
                ifa.TypeAttributes.Add(attr);
            }
            foreach(var tmpAttr in input.Attributes) {
                var attr = ToInterfaceAttribute(tmpAttr);
                attr.ParentInterface = ifa;
                ifa.Attributes.Add(attr);
            }
            return ifa;
        }

        private InterfaceAttribute ToInterfaceAttribute(TmpInterfaceAttribute input) {
            if(input.AttributeType == TmpInterfaceAttributeType.BASE) {
                return ToInterfaceBasicAttribute(input);
            } else if(input.AttributeType == TmpInterfaceAttributeType.REF) {
                return ToInterfaceRefAttribute(input);
            } else if(input.AttributeType == TmpInterfaceAttributeType.FACT) {
                return ToInterfaceFact(input);                
            } else {
                throw new NotSupportedException();
            }
        }

        private InterfaceRefAttribute ToInterfaceRefAttribute(TmpInterfaceAttribute input) {
            var attr = new InterfaceRefAttribute(input.ForeignInterface, input.ReferencedField);
            attr.Alias = input.As;
            
            attr.Calculated = "true".Equals(input.Calculated);
            
            if(input.PrimaryKey == "true") {
                attr.PrimaryKey = true;
            } else {
                attr.PrimaryKey = false;
            }

            return attr;
        }

        private InterfaceFact ToInterfaceFact(TmpInterfaceAttribute input) {
            var attr = new InterfaceFact();
            return (InterfaceFact)ToInterfaceBasicAttribute_common(input, attr);
        }

        private InterfaceBasicAttribute ToInterfaceBasicAttribute(TmpInterfaceAttribute input) {
            var attr = new InterfaceBasicAttribute();
            return ToInterfaceBasicAttribute_common(input, attr);
        }

        private InterfaceBasicAttribute ToInterfaceBasicAttribute_common(TmpInterfaceAttribute input, InterfaceBasicAttribute attr) {
            attr.Name = input.Name;            
            // DataType
            if(input.DataType.Equals("decimal")) {
                var tokens = input.Length.Split(',');
                attr.Length = Int32.Parse(tokens[0]);
                attr.Decimals = Int32.Parse(tokens[1]);
                attr.DataType = InterfaceAttributeDataType.DECIMAL;
            } else if(input.DataType.Equals("varchar")) {
                attr.Length = Int32.Parse(input.Length);
                attr.Decimals = null;
                attr.DataType = InterfaceAttributeDataType.VARCHAR;
            } else if(input.DataType.Equals("int")) {
                attr.Length = 0;
                attr.Decimals = null;
                attr.DataType = InterfaceAttributeDataType.INT;
            }
            // PrimaryKey
            if(input.PrimaryKey == "true") {
                attr.PrimaryKey = true;
            } else {
                attr.PrimaryKey = false;
            }
            // Unit
            attr.Unit = input.Unit;
            return attr;
        }

        private InterfaceTypeAttribute ToInterfaceTypeAttribute(TmpInterfaceTypeAttribute input) {
            return InterfaceTypeAttribute.Convert(input);
        }
    }
}