using Megingjord.Tools.Dialogue_Manager.API.Core.Data;
using UnityEngine;

namespace Megingjord.Tools.Dialogue_Manager.API.Exposed {
    public class DialogueServer : MonoBehaviour {
        
        [Header("References")]
        [Tooltip("The dialogue data to serve to the dialogue manager when the serve method is called")]
        public DialogueData dialogueData;
        [Tooltip("The main actor for this dialogue, ie the NPC the player is talking to")]
        public GameObject actor;

        private void OnEnable() {
            DialogueManager.instance.AddData(dialogueData);
        }

        private void OnDestroy() {
            DialogueManager.instance.RemoveData(dialogueData);
        }

        /// <summary>
        /// Serves the dialogue data to the dialogue manager and
        /// begins a dialogue session
        /// </summary>
        public void Serve() {
            if (dialogueData == null) return;
            DialogueManager.instance.StartDialogue(this);
        }
    }
}