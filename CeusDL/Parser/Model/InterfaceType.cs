using System.Collections.Generic;
using System.IO;

namespace Kdv.CeusDL.Parser.Model 
{
    public enum InterfaceType {
        DEF_TABLE, DIM_TABLE, DIM_VIEW, FACT_TABLE
    }

    public class InterfaceTypeResolver {
        public static InterfaceType Get(string name) {
            if(string.IsNullOrEmpty(name)) {
                return InterfaceType.DIM_TABLE;
            }

            if(name == null) {
                name = "";
            }

            switch(name) {
                case "": return InterfaceType.DIM_TABLE;
                case "DefTable": return InterfaceType.DEF_TABLE;
                case "DimTable": return InterfaceType.DIM_TABLE;            
                case "DimView": return InterfaceType.DIM_VIEW;                
                case "FactTable": return InterfaceType.FACT_TABLE;                
                default:
                    throw new InvalidDataException("InterfaceType ungültig");
            }
        }

        ///
        /// Ermittelt, ob ein interface eine Tabelle oder eine View sein soll...
        ///
        public static bool IsTable(InterfaceType type) {
            switch(type) {
                case InterfaceType.DEF_TABLE:
                case InterfaceType.DIM_TABLE:                
                case InterfaceType.FACT_TABLE:                
                    return true;
                default:
                    return false;
            }
        }
    }
}