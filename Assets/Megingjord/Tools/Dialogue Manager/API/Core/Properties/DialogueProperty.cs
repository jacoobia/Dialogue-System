using System;

namespace Megingjord.Tools.Dialogue_Manager.API.Core.Properties {
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