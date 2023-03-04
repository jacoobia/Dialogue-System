using Megingjord.Tools.Dialogue_Manager.API.Exposed;
using UnityEngine;

namespace Megingjord.Example_Scenes {
    public class CubeDestroy : MonoBehaviour, IDialogueEventConsumer {

        private const string Property = "CUBE_COUNT";
        
        public string targetDialogueEvent;
        
        public void ConsumeDialogueEvent(string @event) {
            if (!@event.Equals(targetDialogueEvent)) return;
            var value = DialogueManager.GetIntProperty(Property);
            DialogueManager.SetProperty(Property, value - 1);
            Destroy(gameObject);
        }
    }
}