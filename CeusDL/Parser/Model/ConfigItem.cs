using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kdv.CeusDL.Parser.TmpModel;

namespace Kdv.CeusDL.Parser.Model {

    public enum ConfigItemEnum {
        PREFIX, IL_DATABASE, BL_DATABASE, BT_DATABASE, AL_DATABASE
    }

    public class ConfigItem {
        public ConfigItemEnum Name {get;set;}
        public string Value {get;set;}

        public static ConfigItem Convert(TmpConfigItem item) {
            return new ConfigItem() {
                Name = GetNameEnum(item),
                Value = item.Value
            };
        }

        private static ConfigItemEnum GetNameEnum(TmpConfigItem item) {
            switch(item.Name.ToLower()) {
                case "prefix":
                    return ConfigItemEnum.PREFIX;
                case "il_database":                
                    return ConfigItemEnum.IL_DATABASE;
                case "bl_database":
                    return ConfigItemEnum.BL_DATABASE;
                case "bt_database":
                    return ConfigItemEnum.BT_DATABASE;
                case "al_database":
                    return ConfigItemEnum.AL_DATABASE;
                default:
                    throw new InvalidDataException($"Ungültiges ConfigItem {item.Name}");
            }
        }
    }
}