using System;

namespace Megingjord.Shared.Reflection {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class NodeAttribute : Attribute {
        
        public string Name { get; }
        
        public NodeAttribute(string name) {
            Name = name;
        }
    }
}