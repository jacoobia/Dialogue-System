using Megingjord.Shared.Reflection.Attributes;
using Megingjord.Tools.Dialogue_Manager.API.Exposed;
using DialogueEventConsumer = Megingjord.Tools.Dialogue_Manager.API.Exposed.DialogueEventConsumer;

namespace Megingjord.Example_Scenes {
    public class CubeDestroy : DialogueEventConsumer {

        private const string Property = "CUBE_COUNT";

        [DialogueEvent("destroy")]
        public void ConsumeDialogueEvent() {
            var value = DialogueManager.GetIntProperty(Property);
            DialogueManager.SetProperty(Property, value - 1);
            Destroy(gameObject);
        }
    }
}