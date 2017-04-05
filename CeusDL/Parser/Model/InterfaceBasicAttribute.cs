namespace Kdv.CeusDL.Parser.Model {
    public enum InterfaceAttributeDataType {
        VARCHAR, INT, DECIMAL
    }
    public class InterfaceBasicAttribute : InterfaceAttribute {
        public string Name {get;set;}
        public InterfaceAttributeDataType DataType {get;set;}
        public int Length {get;set;}
        public int? Decimals {get;set;}
        public string Unit {get;set;}

        public override string ToString() => $"Name: {Name}, DataType: {DataType}, " 
            +$"Length: {Length}, Decimals: {Decimals}, "
            +$"PrimaryKey: {PrimaryKey}, Unit: {Unit}\n";
    }
}