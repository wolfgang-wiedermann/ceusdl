using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System;

namespace Kdv.CeusDL.Generator.IL {
    public abstract class InterfaceLayerAbstractGenerator : IGenerator
    {
        protected string prefix = "";

        public abstract string GenerateCode(ParserResult model);

        public string GetFilename(Interface ifa) {
            return $"{ifa.Name}.csv";
        }

        public string GetHeader(ParserResult model) {
            string code = "";
            if(model.Config.HasValueFor(ConfigItemEnum.IL_DATABASE)) {
                code += $"use {model.Config.GetValue(ConfigItemEnum.IL_DATABASE)};\n\n";
            }
            return code;
        }

        ///
        /// Falls in config-Sektion enthalten, Prefix liefern, ansonsten leerstring
        ///
        protected string GetPrefix(Config conf) {
            if(conf.HasValueFor(ConfigItemEnum.PREFIX)) {
                return $"{conf.GetValue(ConfigItemEnum.PREFIX)}_";                
            } else {
                return "";
            }
        }

        public string GetILDatabase(ParserResult model) {
            if(model.Config.HasValueFor(ConfigItemEnum.IL_DATABASE)) {
                return model.Config.GetValue(ConfigItemEnum.IL_DATABASE);
            } else {
                return "FH_InterfaceLayer";
            }
        }

        public string GetILFieldName(InterfaceAttribute attr) {
            string code = "";
            if(attr is InterfaceBasicAttribute) {
                var basic = (InterfaceBasicAttribute)attr;
                code += $"{attr.ParentInterface.Name}_{basic.Name}";                    
            } else if (attr is InterfaceRefAttribute) {
                var refer = (InterfaceRefAttribute)attr;
                if(string.IsNullOrEmpty(refer.Alias)) {
                    code += $"{refer.ReferencedAttribute.ParentInterface.Name}_{refer.ReferencedAttribute.Name} ";                    } else {
                    code += $"{refer.Alias}_{refer.ReferencedAttribute.ParentInterface.Name}_{refer.ReferencedAttribute.Name} ";
                }                
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