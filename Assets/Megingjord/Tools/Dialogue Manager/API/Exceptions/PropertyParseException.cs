using System;

namespace Megingjord.Tools.Dialogue_Manager.API.Exceptions {
    [Serializable]
    public class PropertyParseException : Exception {
        public PropertyParseException(string message) : base(message) {}
    }
}