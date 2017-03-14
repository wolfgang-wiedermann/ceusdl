using System.Collections.Generic;
using System.IO;

namespace Kdv.CeusDL.Parser.Model 
{
    public enum InterfaceType {
        DIM_TABLE, DIM_VIEW, FACT_TABLE
    }

    public class InterfaceTypeResolver {
        public static InterfaceType Get(string name) {
            if(string.IsNullOrEmpty(name)) {
                return InterfaceType.DIM_TABLE;
            }

            switch(name) {
                case "DimTable": return InterfaceType.DIM_TABLE;
                case "DimView": return InterfaceType.DIM_VIEW;
                case "FactTable": return InterfaceType.FACT_TABLE;
                default:
                    throw new InvalidDataException("InterfaceType ungültig");
            }
        }
    }
}