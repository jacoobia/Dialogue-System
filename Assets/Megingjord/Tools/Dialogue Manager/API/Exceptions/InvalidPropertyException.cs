using System;

namespace Megingjord.Tools.Dialogue_Manager.API.Exceptions {
    /// <summary>
    /// Thrown when no property is found for a given name or a property has
    /// an invalid name/value
    /// </summary>
    [Serializable]
    public class InvalidPropertyException : Exception {
        public InvalidPropertyException(string message) : base(message) { }
    }
}