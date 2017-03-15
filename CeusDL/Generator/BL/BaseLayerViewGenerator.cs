using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System;

namespace Kdv.CeusDL.Generator {
    public class BaseLayerViewGenerator : BaseLayerAbstractGenerator
    {
        public override string GenerateCode(ParserResult model)
        {
            string code = "--\n-- BaseLayer Views\n--\n\n";
            foreach(var obj in model.Interfaces) {
                if(obj.Type == InterfaceType.DIM_TABLE) {
                    code += GenerateCreateViewCode(obj);
                } else if(obj.Type == InterfaceType.FACT_TABLE) {
                    // TODO: Prüfen, ob der code auf bei Faktentabellen passt !!!
                    code += GenerateCreateViewCode(obj);
                } 
                // TODO: hier weiter für die anderen Inteface-Typen
            }
            return code;
        }

        ///
        /// Generierung des gesamten Codes für eine Tabelle/Interface
        ///
        public string GenerateCreateViewCode(Interface ifa) {
            var il = new InterfaceLayerGenerator();
            string code = $"create view {GetViewName(ifa)} as \n";
            code += $"select bl.{ifa.Name}_ID";
            foreach(var attr in ifa.Attributes) {
                if(attr is InterfaceBasicAttribute) {
                    var basic = (InterfaceBasicAttribute)attr;
                    code += $",\n    il.{attr.ParentInterface.Name}_{basic.Name}";
                } else {
                    var refer = (InterfaceRefAttribute)attr;
                    if(string.IsNullOrEmpty(refer.Alias)) {
                        code += $",\n    il.{refer.ReferencedAttribute.ParentInterface.Name}_{refer.ReferencedAttribute.Name} ";
                    } else {
                        code += $",\n    il.{refer.Alias}_{refer.ReferencedAttribute.ParentInterface.Name}_{refer.ReferencedAttribute.Name} ";
                    }
                }
                code += $" as {GetAttributeName(attr)}";
            }
            code += $"\nfrom IL_{ifa.Name} as il\n";
            code += $"    left outer join {GetTableName(ifa)} as bl\n";
            code += $"    on il.{GetILPKField(ifa)} = bl.{GetBLPKField(ifa)};\n\n";
            return code;
        }
    }

}