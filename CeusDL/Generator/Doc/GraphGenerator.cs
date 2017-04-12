using System;
using System.Linq;
using Kdv.CeusDL.Parser.Model;

namespace Kdv.CeusDL.Generator.Doc {
    public class GraphGenerator : IGenerator
    {
        public string GenerateCode(ParserResult model)
        {
            string code = "digraph modell\n{\n";
            foreach(var table in model.Interfaces) {
                code += GenerateInterfaceItems(table);
                code += GenerateInterfaceDependencies(table, model);
            }            
            code += "}";
            return code;
        }

        public string GenerateInterfaceItems(Interface table) {
            string code = $"    {table.Name} [shape=record{GetItemLabel(table)}];\n";            
            return code;
        }

        public string GenerateInterfaceDependencies(Interface table, ParserResult model) {
           string code = "";
           foreach(var attr in table.Attributes.Where(a => a is InterfaceRefAttribute).Select(a => (InterfaceRefAttribute)a)) {
               code += $"    {table.Name} -> {attr.ReferencedAttribute.ParentInterface.Name}{GetRefLabel(attr)};\n";
           }
           return code;
        }

        private string GetRefLabel(InterfaceRefAttribute attr)
        {
            if(!string.IsNullOrEmpty(attr.Alias)) {
                return $" [label=\"{attr.Alias}\"]";
            }
            return "";
        }

        private string GetItemLabel(Interface table)
        {
            if(table.Type == InterfaceType.FACT_TABLE) {
                return ", label=\"{&lt;&lt; Fakt &gt;&gt;|"+table.Name+"}\"";
            } else if(table.Type == InterfaceType.DEF_TABLE) {
                return ", label=\"{&lt;&lt; Konstante &gt;&gt;|"+table.Name+"}\"";
            } else if(table.Type == InterfaceType.DIM_VIEW) {
                return ", label=\"{&lt;&lt; View &gt;&gt;|"+table.Name+"}\"";
            }
            return "";
        }
    }
}