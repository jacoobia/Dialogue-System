using UnityEngine;

namespace Megingjord.Tools.Dialogue_Manager.API.Exposed {
    /// <summary>
    /// A base mono behaviour class for any class that plans to consume
    /// dialogue events
    /// Self managing with registration
    /// </summary>
    public abstract class DialogueEventConsumer : MonoBehaviour {
        
        private void Start() {
            DialogueManager.RegisterEventConsumer(this);
        }

        private void OnDestroy() {
            DialogueManager.DeregisterEventConsumer(this);
        }

    }
}