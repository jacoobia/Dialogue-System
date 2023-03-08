using System;

namespace Megingjord.Tools.Dialogue_Manager.API.Exceptions {
    /// <summary>
    /// Thrown when a property is unable to be parsed
    /// </summary>
    [Serializable]
    public class PropertyParseException : Exception {
        public PropertyParseException(string message) : base(message) {}
    }
}