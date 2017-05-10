/*
 * Idee: Faktentabellen erstmal Datenseitig aufbauen und dann so im Generierungsprozess verwenden
 */

using System;
using System.Linq;
using System.Collections.Generic;
using Kdv.CeusDL.Parser.Model;

namespace Kdv.CeusDL.Generator.AL {
    internal class AnalyticalFactTable : AnalyticalAbstractTable {        

        internal AnalyticalFactTable() {            
        }

        protected void GetRefRecursive(InterfaceRefAttribute attrIn, bool withHistory) {
            var baseTable = attrIn.ReferencedAttribute.ParentInterface;

            // Basis-Attribute und Fakten einfügen
            foreach(var attr in baseTable.Attributes.Where(a => a is InterfaceBasicAttribute)) {
                var basic = (InterfaceBasicAttribute)attr;
                // Bei untergeordneten keine Fakten mit übernehmen (würden evtl. multipliziert!)
                // Außerdem die 
                if(!((attr is InterfaceFact) 
                   || (!withHistory && attr == baseTable.GetHistoryAttribute()) )
                    ) {
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
                if(!(!withHistory && attr == baseTable.GetHistoryAttribute())) {
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
                        GetRefRecursive(refAttr, withHistory);
                    } 
                }
            }
        }
    }
}