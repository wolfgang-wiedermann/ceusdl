
using System;
using System.Linq;
using Kdv.CeusDL.Parser.Model;

namespace Kdv.CeusDL.Generator.Doc {
    public class DocGenerator : IGenerator
    {
        public string GenerateCode(ParserResult model)
        {
            string code = "<h2>Faktentabellen</h2><div style=\"float:none;\">";
            foreach(var table in model.Interfaces.Where(i => i.Type == InterfaceType.FACT_TABLE)) {
                code += GenerateInterfaceDoc(table, model);
            }

            code += "<div style=\"clear:both;\"></div></div><h2>Dimensionstabellen</h2><div style=\"float:none;\">";
            foreach(var table in model.Interfaces.Where(i => i.Type == InterfaceType.DIM_TABLE)) {
                code += GenerateInterfaceDoc(table, model);
            }

            code += "<div style=\"clear:both;\"></div></div><h2>Dimensionsviews</h2><div style=\"float:none;\">";
            foreach(var table in model.Interfaces.Where(i => i.Type == InterfaceType.DIM_VIEW)) {
                code += GenerateInterfaceDoc(table, model);
            }

            code += "<div style=\"clear:both;\"></div></div><h2>Defintionstabellen</h2><div style=\"float:none;\">";
            foreach(var table in model.Interfaces.Where(i => i.Type == InterfaceType.DEF_TABLE)) {
                code += GenerateInterfaceDoc(table, model);
            }
            code += "<div style=\"clear:both;\"></div></div>";

            return $"<html><head><title>Dokumentation</title><head><body>{code}</body></html>";
        }

        public string GenerateInterfaceDoc(Interface table, ParserResult model) {
            string code = $"<h3>{table.Name}</h3>";
            string placeholder = "<div style=\"width:10px;float:left;\">&nbsp;</div>";

            code += $"<b>Basis-Attribute</b><br/><ul>";
            foreach(var attr in table.Attributes.Where(a => a is InterfaceBasicAttribute)) {
                var basic = (InterfaceBasicAttribute)attr;
                code += $"<li>{basic.Name} : {basic.DataType}";
                if(attr is InterfaceFact) {
                    code += "<b> (Fakt)</b>";
                }
                code += "</li>";
            }
            code += "</ul>";

            code += $"<b>Referenz-Attribute</b><br/><ul>";
            foreach(var attr in table.Attributes.Where(a => a is InterfaceRefAttribute)) {
                var reference = (InterfaceRefAttribute)attr;
                code += $"<li>{reference.Alias} {reference.ReferencedAttribute.ParentInterface.Name}.{reference.ReferencedAttribute.Name}";
                code += $" : {reference.ReferencedAttribute.DataType}</li>";
            }
            code += "</ul>";

            return $"<div style=\"border-width:1px;border-style:solid;border-color:black;width:400px;float:left;\">{code}</div>{placeholder}";
        }
    }
}