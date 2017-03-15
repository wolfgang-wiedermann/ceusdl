using System.Collections.Generic;
using System.IO;

namespace Kdv.CeusDL.Parser.Model 
{
    public enum InterfaceType {
        DIM_TABLE, DIM_TABLE_HISTORY, DIM_VIEW, DIM_VIEW_HISTORY, FACT_TABLE, FACT_TABLE_HISTORY
    }

    public class InterfaceTypeResolver {
        public static InterfaceType Get(string name) {
            if(string.IsNullOrEmpty(name)) {
                return InterfaceType.DIM_TABLE;
            }

            switch(name) {
                case "DimTable": return InterfaceType.DIM_TABLE;
                case "DimTableWithHistory": return InterfaceType.DIM_TABLE_HISTORY;
                case "DimView": return InterfaceType.DIM_VIEW;
                case "DimViewWithHistory": return InterfaceType.DIM_VIEW_HISTORY;
                case "FactTable": return InterfaceType.FACT_TABLE;
                case "FactTableWithHistory": return InterfaceType.FACT_TABLE_HISTORY;
                default:
                    throw new InvalidDataException("InterfaceType ungültig");
            }
        }

        ///
        /// Ermittelt, ob ein interface eine Tabelle oder eine View sein soll...
        ///
        public static bool IsTable(InterfaceType type) {
            switch(type) {
                case InterfaceType.DIM_TABLE:
                case InterfaceType.DIM_TABLE_HISTORY:
                case InterfaceType.FACT_TABLE:
                case InterfaceType.FACT_TABLE_HISTORY:
                    return true;
                default:
                    return false;
            }
        }
    }
}