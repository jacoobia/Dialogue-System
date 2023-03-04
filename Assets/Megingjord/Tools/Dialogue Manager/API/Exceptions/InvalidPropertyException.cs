using System;

namespace Megingjord.Tools.Dialogue_Manager.API.Exceptions {
    [Serializable]
    public class InvalidPropertyException : Exception {
        public InvalidPropertyException(string message) : base(message) { }
    }
}