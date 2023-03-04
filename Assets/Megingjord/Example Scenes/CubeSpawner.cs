using Megingjord.Tools.Dialogue_Manager.API.Exposed;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Megingjord.Example_Scenes {
    public class CubeSpawner : MonoBehaviour, IDialogueEventConsumer {
        
        private const string Property = "CUBE_COUNT";
        
        public string targetDialogueEvent;
        public GameObject prefab;

        private Camera _camera;
    
        private void Awake() {
            _camera = Camera.main;
        }

        public void ConsumeDialogueEvent(string @event) {
            if (!@event.Equals(targetDialogueEvent) || _camera == null) return;
            
            var value = DialogueManager.GetIntProperty(Property);
            DialogueManager.SetProperty(Property, value + 1);
            
            var x = Random.Range(-10, 10);
            var y = Random.Range(-5, 5);
            var spawnPosition = new Vector3(x, y, 0);
            Instantiate(prefab, spawnPosition, quaternion.identity);
        }
    
    }
}
