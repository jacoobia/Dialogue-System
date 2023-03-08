using System;

namespace Megingjord.Tools.Dialogue_Manager.API.Core.Properties {
    /// <summary>
    /// The data container for a string property
    /// </summary>
    [Serializable]
    public class StringProperty : DialogueProperty {
        public string propertyValue;
    }
}