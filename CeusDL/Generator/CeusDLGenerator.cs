using Kdv.CeusDL.Parser.Model;
using System;

namespace Kdv.CeusDL.Generator {

public class CeusDLGenerator : IGenerator {
    public string GenerateCode(ParserResult model) {
        string str = "";
        foreach(var ifa in model.Interfaces) {
            str += GenerateInterfaceCode(ifa, model);
        }
        return str;
    }

    public string GenerateInterfaceCode(Interface ifa, ParserResult model) {
        string str = $"interface {ifa.Name} ";
        str += "{\n";
        foreach(var attr in ifa.Attributes) {
            str += GenerateAttrCode(attr);
        }
        str += "}\n\n";
        return str;
    }

    public string GenerateAttrCode(InterfaceAttribute attr) {
        string str = "";
        if(attr is InterfaceBasicAttribute) {
            // BaseAttribut
            var a = (InterfaceBasicAttribute) attr;
            str = $"    base {a.Name}:";
            switch(a.DataType) {
                case InterfaceAttributeDataType.VARCHAR:
                    str += $"varchar(len=\"{a.Length}\")";
                    break;
                case InterfaceAttributeDataType.INT:
                    str += $"int";
                    break;
                case InterfaceAttributeDataType.DECIMAL:
                    str += $"decimal(len=\"{a.Length},{a.Decimals}\"";
                    if(!string.IsNullOrEmpty(a.Unit)) {
                        str += $", unit=\"{a.Unit}\"";
                    }
                    str += ")";
                    break;
                default:
                    throw new InvalidOperationException($"Ungültiger Datentyp {a.DataType.GetType()}");
            }
            str += ";\n";
        } else {
            // RefAttribut
            var a = (InterfaceRefAttribute) attr;
            str = $"    ref {a.ReferencedTypeName}.{a.ReferencedFieldName}";
            if(!string.IsNullOrEmpty(a.Alias)) {
                str += $" as {a.Alias}";
            }
            str += ";\n";
        }
        return str;
    }
}

}