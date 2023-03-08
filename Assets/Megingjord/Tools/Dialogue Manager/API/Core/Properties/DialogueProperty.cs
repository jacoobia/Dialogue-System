using System;

namespace Megingjord.Tools.Dialogue_Manager.API.Core.Properties {
    /// <summary>
    /// The base data container for a dialogue property
    /// </summary>
    [Serializable]
    public class DialogueProperty {

        public string guid;
        public string propertyName;

        public DialogueProperty() {
            guid = Guid.NewGuid().ToString();
            propertyName = "New Property";
        }
        
    }
}