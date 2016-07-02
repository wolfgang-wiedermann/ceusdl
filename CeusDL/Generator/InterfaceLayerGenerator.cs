using Kdv.CeusDL.Parser.Model;

namespace Kdv.CeusDL.Generator {
    public class InterfaceLayerGenerator : IGenerator {
        public string GenerateCode(ParserResult model) {
            string code = "-- Interface Layer\n\n";
            foreach(var obj in model.Interfaces) {
                code += GenerateILTable(obj);
            }
            return code;
        }

        public string GenerateILTable(Interface ifa) {
            int i = 0;
            string code = $"create table IL_{ifa.Name} (\n";
            foreach(var field in ifa.Attributes) {
                code += GenerateILTableField(field);
                if(i+1 < ifa.Attributes.Count) {
                    code += ", \n";
                } else {
                    code += "\n";
                }
                i++;
            }
            code += ");\n\n";
            return code;
        }

        public string GenerateILTableField(InterfaceAttribute attr) {
            string code = "    ";
            if(attr is InterfaceBasicAttribute) {
                var basic = (InterfaceBasicAttribute)attr;
                code += $"{attr.ParentInterface.Name}_{basic.Name} ";
                code += GetTypeFromBasic(basic);
            } else if (attr is InterfaceRefAttribute) {
                var refer = (InterfaceRefAttribute)attr;
                if(string.IsNullOrEmpty(refer.Alias)) {
                    code += $"{refer.ReferencedAttribute.ParentInterface.Name}_{refer.ReferencedAttribute.Name} ";
                } else {
                    code += $"{refer.Alias}_{refer.ReferencedAttribute.ParentInterface.Name}_{refer.ReferencedAttribute.Name} ";
                }
                code += GetTypeFromBasic(refer.ReferencedAttribute);
            }
            return code;
        }

        public string GetTypeFromBasic(InterfaceBasicAttribute basic) {
            string code = "";
            if(basic.DataType == InterfaceAttributeDataType.VARCHAR) {
                code += $"varchar({basic.Length})";
            } else if(basic.DataType == InterfaceAttributeDataType.DECIMAL) {
                code += $"decimal({basic.Length},{basic.Decimals})";
            } else if(basic.DataType == InterfaceAttributeDataType.INT) {
                code += $"int";
            }
            return code;
        }
    }
}