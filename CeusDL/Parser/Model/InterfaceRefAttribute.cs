namespace Kdv.CeusDL.Parser.Model {
    public class InterfaceRefAttribute : InterfaceAttribute {

        internal string ReferencedTypeName { get; }
        internal string ReferencedFieldName { get; }
        public InterfaceBasicAttribute ReferencedAttribute { get;set; }

        public string Alias { get; set;}

        public InterfaceRefAttribute(string typeName, string fieldName) {
            ReferencedTypeName = typeName;
            ReferencedFieldName = fieldName;
        }
        
        public override string ToString() => $"Ref: {ReferencedTypeName}.{ReferencedFieldName} =>"
            + $" {ReferencedAttribute.ParentInterface.Name}.{ReferencedAttribute.Name}\n";
    }
}