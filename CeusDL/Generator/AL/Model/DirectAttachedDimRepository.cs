using Kdv.CeusDL.Parser.Model;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Kdv.CeusDL.Generator.AL {
    
    public class DirectAttachedDim {

        public DirectAttachedDim(InterfaceRefAttribute attribute) {
            this.Key = attribute.Alias+"_"+attribute.ReferencedAttribute.ParentInterface.Name;
            this.Attribute = attribute;
        }
        public string Key { get; private set; }

        public InterfaceRefAttribute Attribute { get; private set; }
    }

    public class DirectAttachedDimRepository {
        public Dictionary<string, DirectAttachedDim> Dimensions { get; private set; }

        public DirectAttachedDimRepository() {
            Dimensions = new Dictionary<string, DirectAttachedDim>();
        }

        public void Add(DirectAttachedDim dim) { 
            if(!Dimensions.ContainsKey(dim.Key)) {
                Dimensions.Add(dim.Key, dim);         
            }   
        }

        public void AddRange(ICollection<InterfaceRefAttribute> refs) {
            foreach(var r in refs) {
                Add(new DirectAttachedDim(r));
            }
        }
    }
}