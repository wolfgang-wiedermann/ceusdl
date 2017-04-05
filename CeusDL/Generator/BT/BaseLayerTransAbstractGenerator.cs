using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Kdv.CeusDL.Generator.BT {
    public abstract class BaseLayerTransAbstractGenerator : IGenerator
    {
        protected BL.BaseLayerTableGenerator blGenerator = new BL.BaseLayerTableGenerator();
        public abstract string GenerateCode(ParserResult model);

        protected string GetMandantSpalte(Interface ifa)
        {
            if(ifa.IsMandantInterface()) {
                return ",\n    Mandant_ID int not null";
            } else {
                return "";
            }
        }

        public object GetTableName(Interface ifa, Config conf)
        {
            if(ifa.Type == InterfaceType.DEF_TABLE) {
                return $"{blGenerator.GetPrefix(conf)}BT_D_{ifa.Name}";
            } else {
                return $"{blGenerator.GetPrefix(conf)}BT{blGenerator.GetTypeSuffix(ifa)}_{ifa.Name}";
            }
        }

        public string GetUseStatement(ParserResult model) {
            if(model.Config.HasValueFor(ConfigItemEnum.BT_DATABASE)) {
                return $"use {model.Config.GetValue(ConfigItemEnum.BT_DATABASE)}\n\n";
            } else if(model.Config.HasValueFor(ConfigItemEnum.BL_DATABASE)) {
                // Notfalls BL-Datenbanknamen als Fallback verwenden
                return $"use {model.Config.GetValue(ConfigItemEnum.BT_DATABASE)}\n\n";
            }
            return "";
        }     

    }
}