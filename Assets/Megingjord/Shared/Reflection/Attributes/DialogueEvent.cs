using System;
using JetBrains.Annotations;

namespace Megingjord.Shared.Reflection.Attributes {
    /// <summary>
    /// Defines what event function will consume 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    [MeansImplicitUse]
    public class DialogueEvent : Attribute {
        
        private readonly string _event;

        public DialogueEvent(string eventName) {
            _event = eventName;
        }

        public string GetEventName() {
            return _event;
        }
        
    }
}