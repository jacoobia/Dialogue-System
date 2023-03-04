using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Megingjord.Shared.Reflection {
    public static class ReflectionUtils {
        private static IEnumerable<Type> GetNodeTypes(Assembly assembly) {
            return assembly.GetTypes().Where(type => type.GetCustomAttributes(typeof(NodeAttribute), true).Length > 0);
        }
 
        public static Dictionary<Type, string> GetNodeTypeDictionary() {
            Dictionary<Type, string> output = new();
            var types = GetNodeTypes(Assembly.GetCallingAssembly());
            foreach(var type in types) {
                var nodeAttribute = (NodeAttribute)type.GetCustomAttribute(typeof(NodeAttribute));
                if (nodeAttribute != null) {
                    output.Add(type, nodeAttribute.Name);
                }
                else Debug.Log("no have attribute, big sad :(");
            }
            return output;
        }
        
    }
}