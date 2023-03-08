using System;

namespace Megingjord.Shared.Reflection.Attributes {
    /// <summary>
    /// Used to automatically add node classes to the graph list
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class NodeAttribute : Attribute {
        
        public string Name { get; }
        
        public NodeAttribute(string name) {
            Name = name;
        }
    }
}