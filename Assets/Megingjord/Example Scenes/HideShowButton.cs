using Megingjord.Tools.Dialogue_Manager.API.Exposed;
using UnityEngine;

namespace Megingjord.Example_Scenes {
    public class HideShowButton : MonoBehaviour {

        public GameObject exampleButton;

        private void Awake() {
            DialogueManager.DialogueEnterEvent += DialogueEnter;
            DialogueManager.DialogueExitEvent += DialogueExit;
        }

        private void OnDisable() {
            DialogueManager.DialogueEnterEvent -= DialogueEnter;
            DialogueManager.DialogueExitEvent -= DialogueExit;
        }

        private void DialogueEnter() {
            exampleButton.SetActive(false);
        }

        private void DialogueExit() {
            exampleButton.SetActive(true);
        }

    }
}