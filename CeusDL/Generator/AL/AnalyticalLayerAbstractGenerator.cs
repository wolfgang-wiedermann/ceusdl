using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Kdv.CeusDL.Generator.AL {
    public abstract class AnalyticalLayerAbstractGenerator : IGenerator
    {
        protected BL.BaseLayerTableGenerator blGenerator = new BL.BaseLayerTableGenerator();
        public abstract string GenerateCode(ParserResult model);

        protected object GetTableName(Interface ifa, Config conf)
        {
            if(ifa.Type == InterfaceType.FACT_TABLE) {
                return $"{blGenerator.GetPrefix(conf)}F_{ifa.Name}";                
            } else {
                throw new NotImplementedException();
            }
        }

        public string GetColumnName(InterfaceAttribute attr, Interface factTable, ParserResult model)
        {
            if(attr is InterfaceBasicAttribute) {
                var a = (InterfaceBasicAttribute)attr;              
                return blGenerator.GetAttributeName(a);                
            } else if(attr is InterfaceRefAttribute) {
                var a = (InterfaceRefAttribute)attr;  
                if(string.IsNullOrEmpty(a.Alias)) {
                    return $"{a.ReferencedAttribute.ParentInterface.Name}_ID";
                } else {
                    return $"{a.Alias}_{a.ReferencedAttribute.ParentInterface.Name}_ID";
                }            
            } else {
                throw new NotImplementedException();
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

        public List<InterfaceRefAttribute> GetDirectAttachedDimensions(Interface factTable, ParserResult model) {
            HashSet<InterfaceRefAttribute> directAttached = new HashSet<InterfaceRefAttribute>();
            
            var refAttributes = factTable.Attributes
                .Where(a => a is InterfaceRefAttribute)
                .Select(a => (InterfaceRefAttribute)a)                
                .Where(a => a.ReferencedAttribute.ParentInterface.Type != InterfaceType.FACT_TABLE)
                .ToList<InterfaceRefAttribute>();

            foreach(var attr in refAttributes) {                
                directAttached.Add(attr);
            }
            
            return directAttached.ToList<InterfaceRefAttribute>();
        }

    }
}