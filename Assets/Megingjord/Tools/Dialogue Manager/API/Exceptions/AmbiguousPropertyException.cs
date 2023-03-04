using System;

namespace Megingjord.Tools.Dialogue_Manager.API.Exceptions {
    [Serializable]
    public class AmbiguousPropertyException : Exception {
        public AmbiguousPropertyException(string message) : base(message) {}
    }
}