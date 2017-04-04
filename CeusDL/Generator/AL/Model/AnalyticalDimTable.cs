/*
 * Idee: Dimensionstabellen erstmal Datenseitig aufbauen und dann so im Generierungsprozess verwenden
 */

using System;
using System.Linq;
using System.Collections.Generic;
using Kdv.CeusDL.Parser.Model;

namespace Kdv.CeusDL.Generator.AL {
    internal class AnalyticalDimTable : AnalyticalAbstractTable {        
        internal string Alias { get; set; }        
        private List<InterfaceRefAttribute> Children = new List<InterfaceRefAttribute>();
        internal AnalyticalDimTable(InterfaceRefAttribute reference, ParserResult model)  {
            Alias = reference.Alias;                        
            var baseTable = reference.ReferencedAttribute.ParentInterface;
            MainInterface = baseTable;
            var blGenerator = new BL.BaseLayerTableGenerator();            
            Name = $"{blGenerator.GetPrefix(model.Config)}D_{GetAlias()}{baseTable.Name}_1_{baseTable.Name}";

            // ID-Spalte einfügen            
            Add(new InterfaceBasicAttribute() {
                Name=$"{GetAlias()}{baseTable.Name}_ID",
                DataType=InterfaceAttributeDataType.INT,
                PrimaryKey=false,
                ParentInterface=baseTable
            });

            if(baseTable.IsMandantInterface()) {
                // Mandant-Spalte einfügen
                Add(new InterfaceBasicAttribute() {
                    Name="Mandant_ID",
                    DataType=InterfaceAttributeDataType.INT,
                    PrimaryKey=false,
                    ParentInterface=baseTable
                });                
            }

            // Basis-Attribute und Fakten einfügen
            foreach(var attr in baseTable.Attributes.Where(a => a is InterfaceBasicAttribute)) {
                var basic = (InterfaceBasicAttribute)attr;
                Add(new InterfaceBasicAttribute() {
                    Name=$"{GetAlias()}{baseTable.Name}_{basic.Name}",
                    DataType=basic.DataType,
                    PrimaryKey=basic.PrimaryKey,
                    ParentInterface=basic.ParentInterface,
                    Length=basic.Length,
                    Decimals=basic.Decimals, 
                    Unit=basic.Unit
                });                
            }

            // Attribute aus untergeordneten Tabellen ermitteln...
            foreach(var attr in baseTable.Attributes.Where(a => a is InterfaceRefAttribute)) {
                // Ref-Attribute auflösen
                var refAttr = (InterfaceRefAttribute)attr;
                GetRefRecursive(refAttr, refAttr.Alias);
            }
        }

        private void GetRefRecursive(InterfaceRefAttribute attr, string alias) {
            if(this.Children.Where(a => a.Alias == attr.Alias && a.ReferencedAttribute == attr.ReferencedAttribute).Count() == 0) {
                Children.Add(attr);
                var baseTable = attr.ReferencedAttribute.ParentInterface;

                // ID-Spalte einfügen            
                Add(new InterfaceBasicAttribute() {
                    Name=$"{GetAlias()}{baseTable.Name}_ID",
                    DataType=InterfaceAttributeDataType.INT,
                    PrimaryKey=false,
                    ParentInterface=baseTable
                });

                // Basis-Attribute und Fakten einfügen
                foreach(var tmpAttr in baseTable.Attributes.Where(a => a is InterfaceBasicAttribute)) {
                    var basic = (InterfaceBasicAttribute)tmpAttr;
                    Add(new InterfaceBasicAttribute() {
                        Name=$"{GetAlias()}{baseTable.Name}_{basic.Name}",
                        DataType=basic.DataType,
                        PrimaryKey=basic.PrimaryKey,
                        ParentInterface=basic.ParentInterface,
                        Length=basic.Length,
                        Decimals=basic.Decimals, 
                        Unit=basic.Unit
                    });                
                }

                // Attribute aus untergeordneten Tabellen ermitteln...
                foreach(var tmpAttr in baseTable.Attributes.Where(a => a is InterfaceRefAttribute)) {
                    // Ref-Attribute auflösen
                    var refAttr = (InterfaceRefAttribute)tmpAttr;
                    GetRefRecursive(refAttr, refAttr.Alias);
                }
            } else {
                Console.WriteLine($"ERROR: {attr.Alias}_{attr.ReferencedAttribute.ParentInterface} bereits enthalten...");
            }
        }

        internal string GetAlias() {
            if(string.IsNullOrEmpty(Alias)) {
                return "";
            } else {
                return $"{Alias}_";
            }
        }

    }
}