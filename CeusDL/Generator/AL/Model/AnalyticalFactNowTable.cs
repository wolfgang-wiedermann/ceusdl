/*
 * Idee: Faktentabellen erstmal Datenseitig aufbauen und dann so im Generierungsprozess verwenden
 */

using System;
using System.Linq;
using System.Collections.Generic;
using Kdv.CeusDL.Parser.Model;

namespace Kdv.CeusDL.Generator.AL {
    internal class AnalyticalFactNowTable : AnalyticalFactTable {                

        internal AnalyticalFactNowTable(Interface baseTable, ParserResult model) {

            if(!baseTable.IsHistorizedInterface()) {
                throw new InvalidOperationException("AnalyticalFactNowTables können nur für Historisierte Faktentabellen erzeugt werden");
            }

            var blGenerator = new BL.BaseLayerTableGenerator();            
            Name = $"{blGenerator.GetPrefix(model.Config)}F_Now_{baseTable.Name}";
            MainInterface = baseTable;

            // Die ID-Spalte wird hier nicht berücksichtigt, sondern statisch
            // in der Generierung eingebracht, (wegen bigint)

            if(baseTable.IsMandantInterface()) {
                // Mandant-Spalte einfügen
                Add(new InterfaceBasicAttribute() {
                    Name="Mandant_ID",
                    DataType=InterfaceAttributeDataType.INT,
                    PrimaryKey=false,
                    ParentInterface=baseTable,
                    ParentAttribute=null
                });                
            }

            var historyAttr = baseTable.GetHistoryAttribute();
            // Basis-Attribute und Fakten einfügen        
            foreach(var attr in baseTable.Attributes.Where(a => a is InterfaceBasicAttribute)) {
                if(attr != historyAttr) {
                    var basic = (InterfaceBasicAttribute)attr;
                    var tmpName = $"{baseTable.Name}_{basic.Name}";
                    if(attr is InterfaceFact) {
                        tmpName = $"{baseTable.Name}_Now_{basic.Name}";
                    }
                    Add(new InterfaceBasicAttribute() {
                        Name=tmpName,
                        DataType=basic.DataType,
                        PrimaryKey=basic.PrimaryKey,
                        ParentInterface=basic.ParentInterface,
                        ParentAttribute=attr,
                        Length=basic.Length,
                        Decimals=basic.Decimals, 
                        Unit=basic.Unit
                    });                
                }
            }

            // Attribute aus untergeordneten Tabellen ermitteln...
            foreach(var attr in baseTable.Attributes.Where(a => a is InterfaceRefAttribute)) {
                if(attr != historyAttr) {
                    // Ref-Attribute auflösen
                    var refAttr = (InterfaceRefAttribute)attr;
                    // ID-Spalte einbringen...
                    var refTable = refAttr.ReferencedAttribute.ParentInterface;
                    Add(new InterfaceBasicAttribute() {
                        Name=(string.IsNullOrEmpty(refAttr.Alias)?"":refAttr.Alias+"_")+$"{refTable.Name}_ID",
                        DataType=InterfaceAttributeDataType.INT,
                        PrimaryKey=false,
                        ParentInterface=baseTable,
                        ParentAttribute=attr
                    });
                
                    if(refAttr.ReferencedAttribute.ParentInterface.Type == InterfaceType.FACT_TABLE) {
                        // Rekursiv integrieren
                        GetRefRecursive(refAttr, false);
                    } 
                }
            }
        }
    }
}