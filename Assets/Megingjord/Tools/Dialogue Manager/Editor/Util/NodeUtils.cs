using System;
using UnityEditor.Experimental.GraphView;

namespace Megingjord.Tools.Dialogue_Manager.Editor.Util {
    public static class NodeUtils {
        
        public static Port GenerateTypelessPort(Node node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single) {
            return GeneratePort(node, portDirection, typeof(float), capacity);
        }
        
        public static Port GeneratePort(Node node, Direction portDirection, Type type, Port.Capacity capacity = Port.Capacity.Single) {
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, type);
        }
        
    }
}