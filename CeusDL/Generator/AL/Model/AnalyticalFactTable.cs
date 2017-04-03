/*
 * Idee: Faktentabellen erstmal Datenseitig aufbauen und dann so im Generierungsprozess verwenden
 */

using System;
using System.Linq;
using System.Collections.Generic;
using Kdv.CeusDL.Parser.Model;

namespace Kdv.CeusDL.Generator.AL {
    internal class AnalyticalFactTable : AnalyticalAbstractTable {        

        internal AnalyticalFactTable(Interface baseTable, ParserResult model) {
            var blGenerator = new BL.BaseLayerTableGenerator();            
            Name = $"{blGenerator.GetPrefix(model.Config)}F_{baseTable.Name}";

            // Die ID-Spalte wird hier nicht berücksichtigt, sondern statisch
            // in der Generierung eingebracht, (wegen bigint)

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
                    Name=$"{baseTable.Name}_{basic.Name}",
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
                // ID-Spalte einbringen...
                var refTable = refAttr.ReferencedAttribute.ParentInterface;
                Add(new InterfaceBasicAttribute() {
                    Name=(string.IsNullOrEmpty(refAttr.Alias)?"":refAttr.Alias+"_")+$"{refTable.Name}_ID",
                    DataType=InterfaceAttributeDataType.INT,
                    PrimaryKey=false,
                    ParentInterface=baseTable
                });
                
                if(refAttr.ReferencedAttribute.ParentInterface.Type == InterfaceType.FACT_TABLE) {
                    // Rekursiv integrieren
                    GetRefRecursive(refAttr);
                } 
            }
        }

        private void GetRefRecursive(InterfaceRefAttribute attrIn) {
            var baseTable = attrIn.ReferencedAttribute.ParentInterface;

            // Basis-Attribute und Fakten einfügen
            foreach(var attr in baseTable.Attributes.Where(a => a is InterfaceBasicAttribute)) {
                var basic = (InterfaceBasicAttribute)attr;
                // Bei untergeordneten keine Fakten mit übernehmen (würden evtl. multipliziert!)
                if(!(attr is InterfaceFact)) {
                    Add(new InterfaceBasicAttribute() {
                        Name=$"{baseTable.Name}_{basic.Name}",
                        DataType=basic.DataType,
                        PrimaryKey=basic.PrimaryKey,
                        ParentInterface=basic.ParentInterface,
                        Length=basic.Length,
                        Decimals=basic.Decimals, 
                        Unit=basic.Unit
                    });                
                }
            }

            // Attribute aus untergeordneten Tabellen ermitteln...
            foreach(var attr in baseTable.Attributes.Where(a => a is InterfaceRefAttribute)) {
                // Ref-Attribute auflösen
                var refAttr = (InterfaceRefAttribute)attr;
                // ID-Spalte einbringen...
                var refTable = refAttr.ReferencedAttribute.ParentInterface;
                Add(new InterfaceBasicAttribute() {
                    Name=(string.IsNullOrEmpty(refAttr.Alias)?"":refAttr.Alias+"_")+$"{refTable.Name}_ID",
                    DataType=InterfaceAttributeDataType.INT,
                    PrimaryKey=false,
                    ParentInterface=baseTable
                });

                if(refAttr.ReferencedAttribute.ParentInterface.Type == InterfaceType.FACT_TABLE) {
                    // Rekursiv integrieren
                    GetRefRecursive(refAttr);
                } 
            }
        }
    }
}