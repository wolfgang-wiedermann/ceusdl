namespace Kdv.CeusDL.Parser.Model {
    public class InterfaceAttribute {
        public Interface ParentInterface { get; set; }
        public InterfaceAttribute ParentAttribute {get; set;}
        public bool PrimaryKey {get; set;}
        public bool Calculated {get; set;}     
    }
}