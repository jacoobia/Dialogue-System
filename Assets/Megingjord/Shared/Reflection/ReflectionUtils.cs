using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Megingjord.Shared.Reflection.Attributes;
using UnityEngine;
using DialogueEventConsumer = Megingjord.Tools.Dialogue_Manager.API.Exposed.DialogueEventConsumer;

namespace Megingjord.Shared.Reflection {
    public static class ReflectionUtils {
        
        /// <summary>
        /// Gets the class type for each class in the runtime assembly marked with
        /// the Node attribute
        /// </summary>
        /// <param name="assembly">The assembly to search</param>
        /// <returns></returns>
        private static IEnumerable<Type> GetNodeTypes(Assembly assembly) {
            return assembly.GetTypes().Where(type => type.GetCustomAttributes(typeof(NodeAttribute), true).Length > 0);
        }
 
        /// <summary>
        /// Builds a dictionary of types : name for each of the nodes
        /// marked with the Node attribute
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the methods within a class extending the event consumer interface
        /// that are marked with the event dialogue attribute
        /// </summary>
        /// <param name="consumer">The consumer to search</param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetEventMethods(this DialogueEventConsumer consumer) {
            return consumer.GetType().GetMethods().Where(method => method.GetCustomAttributes(typeof(DialogueEvent), false).Length > 0).ToList();
        }

        /// <summary>
        /// Checks if a method is a subscriber of a particular event or not
        /// </summary>
        /// <param name="methodInfo">The method to use</param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static bool IsMethodForEvent(this MethodInfo methodInfo, string method) {
            var attribute = methodInfo.GetCustomAttributes(typeof(DialogueEvent), false)[0] as DialogueEvent;
            return attribute != null && method.Equals(attribute.GetEventName());
        }
        
    }
}