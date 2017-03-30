using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Kdv.CeusDL.Generator.AL {
    public abstract class AnalyticalLayerAbstractGenerator : IGenerator
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

        protected object GetTableName(Interface ifa, Config conf)
        {
            if(ifa.Type == InterfaceType.FACT_TABLE) {
                return $"{blGenerator.GetPrefix(conf)}F_{ifa.Name}";                
            } else {
                // TODO: hier muss ich noch drübber!!!
                return $"{blGenerator.GetPrefix(conf)}D_TODO_{ifa.Name}";
            }
        }

        public string GetUseStatement(ParserResult model) {
            if(model.Config.HasValueFor(ConfigItemEnum.AL_DATABASE)) {
                return $"use {model.Config.GetValue(ConfigItemEnum.AL_DATABASE)}\n\n";
            } 
            return "";
        }

        public List<Interface> GetFactTables(ParserResult model) {
            return model.Interfaces.Where(i => i.Type == InterfaceType.FACT_TABLE).ToList<Interface>();
        }

        public List<Interface> GetDirectAttachedDimensions(ParserResult model) {
            HashSet<Interface> directAttached = new HashSet<Interface>();
            var factTables = GetFactTables(model);

            foreach(var factTable in factTables) {

                var refAttributes = factTable.Attributes
                    .Where(a => a is InterfaceRefAttribute)
                    .Select(a => (InterfaceRefAttribute)a)
                    .ToList<InterfaceRefAttribute>();

                foreach(var attr in refAttributes) {
                    directAttached.Add(attr.ReferencedAttribute.ParentInterface);
                }
            }

            return directAttached.ToList<Interface>();
        }

    }
}