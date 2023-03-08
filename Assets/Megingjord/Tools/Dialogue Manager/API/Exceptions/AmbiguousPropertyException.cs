using System;

namespace Megingjord.Tools.Dialogue_Manager.API.Exceptions {
    /// <summary>
    /// An exception to be thrown when multiple differing properties of the same name
    /// exist, for example if an integer and a string property shared the same name
    /// this could cause type issues and thus this exception is thrown
    /// </summary>
    [Serializable]
    public class AmbiguousPropertyException : Exception {
        public AmbiguousPropertyException(string message) : base(message) {}
    }
}