
using static Kdv.CeusDL.Parser.TmpModel.TmpInterfaceAttributeType;

namespace Kdv.CeusDL.Parser.TmpModel
{
    internal enum TmpInterfaceAttributeType {
        BASE, FACT, REF
    }
    
    internal class TmpInterfaceAttribute {
        
        public TmpInterfaceAttributeType AttributeType {get;set;}
        // For BASE-Attributes only
        public string Name {get;set;}
        public string DataType {get;set;}
        public string Length {get;set;}
        public string PrimaryKey {get;set;}
        public string Unit {get;set;}
        // For REF-Attributes only
        public string ForeignInterface {get;set;}
        public string ReferencedField {get;set;}
        public string As {get;set;}
        // TODO: hier gehts weiter !!!

        public bool IsValid() {
            if(AttributeType == TmpInterfaceAttributeType.REF) {
                return !string.IsNullOrEmpty(ForeignInterface)
                    && !string.IsNullOrEmpty(ReferencedField);
            } else if(AttributeType == TmpInterfaceAttributeType.BASE || AttributeType == TmpInterfaceAttributeType.FACT) {
                return !string.IsNullOrEmpty(Name)
                    && !string.IsNullOrEmpty(DataType);
            }
            return false;
        }
    }
}