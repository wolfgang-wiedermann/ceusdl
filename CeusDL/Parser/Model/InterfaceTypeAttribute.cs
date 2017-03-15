using System;
using System.Collections.Generic;
using System.IO;
using Kdv.CeusDL.Parser.TmpModel;

namespace Kdv.CeusDL.Parser.Model {

    public enum InterfaceTypeAttributeEnum {
        MANDANT, HISTORY
    }

    public class InterfaceTypeAttribute {
        public InterfaceTypeAttributeEnum Name {get;set;}
        public string Value {get;set;}

        public static InterfaceTypeAttribute Convert(TmpInterfaceTypeAttribute attr) {
            return new InterfaceTypeAttribute() {
                Name = GetNameEnum(attr),
                Value = attr.Value
            };
        }

        private static InterfaceTypeAttributeEnum GetNameEnum(TmpInterfaceTypeAttribute attr) {
            switch(attr.Name.ToLower()) {
                case "mandant":
                    return InterfaceTypeAttributeEnum.MANDANT;
                case "history":
                    return InterfaceTypeAttributeEnum.HISTORY;
                default:
                    throw new InvalidDataException($"Ungültiges InterfaceTypeAttribut {attr.Name}");
            }
        }
    }
}