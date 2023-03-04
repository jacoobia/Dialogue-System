using System;

namespace Megingjord.Tools.Dialogue_Manager.API.Core.Attributes {
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class DialogueEventConsumer : Attribute {

        private string _event;

        public DialogueEventConsumer(string eventName) {
            _event = eventName;
        }

    }
}