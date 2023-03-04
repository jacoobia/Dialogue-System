using System.Collections.Generic;
using System.Linq;
using Megingjord.Shared.Helpers;
using Megingjord.Tools.Dialogue_Manager.API.Core;
using Megingjord.Tools.Dialogue_Manager.API.Core.Data;
using Megingjord.Tools.Dialogue_Manager.API.Exceptions;
using Megingjord.Tools.Dialogue_Manager.API.Exposed.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static Megingjord.Tools.Dialogue_Manager.PrimitiveUtils;

namespace Megingjord.Tools.Dialogue_Manager.API.Exposed {
    [RequireComponent(typeof(AudioSource))]
    public class DialogueManager : MonoBehaviour {

        public static event UnityAction DialogueExitEvent = delegate { }; 
        public static event UnityAction DialogueEnterEvent = delegate { };

        public static DialogueManager instance;
        
        private void Awake() {
            if (instance != null) {
                Debug.LogError($"A dialogue manager already exists in this scene, destroying instance on '{gameObject.name}'.");
                Destroy(this);
            }

            instance = this;
        }

        [Header("Object References")] 
        [Tooltip("If your player isn't persistent or is loaded via script, make use of DialogueManager.SetPlayer()")]
        [SerializeField] private GameObject player;
        [Tooltip("This will default to Camera.main if none is set")]
        [SerializeField] private Camera dialogueCamera;
        [Tooltip("These are the cameras that will NOT have their state changed by the dialogue manager. An example could be a UI camera.")]
        [SerializeField] private Camera[] cameraExemptions;

        [Header("Settings")] 
        [SerializeField] private bool focusCameraOnActor;
        [SerializeField] private bool resetCameraOnExit = true;
        [SerializeField] private bool displayActorNames;

        [Header("Dialogue UI References")] 
        [SerializeField] private GameObject dialogueUiContainer;
        [SerializeField] private Transform choiceButtonParent;
        [SerializeField] private TMP_Text nameDisplay;
        [SerializeField] private TMP_Text textDisplay;

        [Header("Prefabs")] 
        [SerializeField] private GameObject choiceButtonPrefab;

        private DialogueParser _dialogueParser;

        private DialogueServer _currentDialogueServer;
        private GameObject _actor;

        private Vector3 _originalCameraPosition;
        private Quaternion _originalCameraRotation;
        
        private readonly List<GameObject> _buttonCache = new();
        private readonly Dictionary<Camera, bool> _cameraStateCache = new();
        private readonly List<DialogueData> _dialogueDataCache = new();

        private void Start() {
            dialogueCamera ??= Camera.main;
            dialogueUiContainer.SetActive(false);
        }
        
        public void AddData(DialogueData dialogueData) {
            if (_dialogueDataCache.Contains(dialogueData)) return;
            _dialogueDataCache.Add(dialogueData);
        }

        public void RemoveData(DialogueData dialogueData) {
            if (!_dialogueDataCache.Contains(dialogueData)) return;
            _dialogueDataCache.Remove(dialogueData);
        }

        public void StartDialogue(DialogueServer server) {
            if (_dialogueParser != null) {
                Debug.LogWarning($"An attempt was made to start a new dialogue session when one is already running! {server.dialogueData.name}");
                return;
            }

            if (server.actor == null) {
                MWarn.ActorNotSpecified.Print();
            } 
            else _actor = server.actor;

            var cameraTransform = dialogueCamera.transform;
            _originalCameraPosition = cameraTransform.position;
            _originalCameraRotation = cameraTransform.rotation;

            _currentDialogueServer = server;
            _dialogueParser = new DialogueParser(server.dialogueData);
            DialogueEnterEvent.Invoke();

            if (dialogueUiContainer != null) {
                dialogueUiContainer.SetActive(true);
            }

            // Store none-exempt cameras states
            _cameraStateCache.Clear();
            var cameras = Camera.allCameras;
            foreach (var cam in cameras) {
                if(cameraExemptions.Contains(cam)) continue;
                _cameraStateCache.Add(cam, cam.enabled);
                if (cam != dialogueCamera)
                    cam.enabled = false;
            }

            Next();
        }

        private void StopDialogue() {
            ClearButtons();
            _dialogueParser = null;
            dialogueUiContainer.SetActive(false);
            
            // Reset all cameras to their original state
            var cameras = Camera.allCameras;
            foreach (var cam in cameras) {
                if(cameraExemptions.Contains(cam)) continue;
                cam.enabled = _cameraStateCache[cam];
            }

            if (resetCameraOnExit) {
                dialogueCamera.transform.position = _originalCameraPosition;
                // ReSharper disable once Unity.InefficientPropertyAccess
                dialogueCamera.transform.rotation = _originalCameraRotation;
            }

            _currentDialogueServer = null;
            _actor = null;
            _cameraStateCache.Clear();
        }

        private void Next(int option = 0) {
            while (true) {
                ClearButtons();
                var node = _dialogueParser.GetNext(option);
                if (node == null) {
                    StopDialogue();
                    break;
                }
                
                UpdateUI(node);
                
                switch (node) {
                    case ExitNodeData:
                        StopDialogue();
                        break;
                    case EventNodeData eventNodeData:
                        TriggerEvent(eventNodeData.eventName);
                        option = 0;
                        continue;
                    case FocusNodeData focusNodeData:
                        ProcessFocusNode(focusNodeData);
                        option = 0;
                        continue;
                    case BasicNodeData basicNodeData:
                        ProcessBasicNode(basicNodeData);
                        break;
                    case ConditionNodeData conditionNodeData:
                        ProcessConditionNode(conditionNodeData);
                        break;
                }
                break;
            }
        }

        private void ProcessConditionNode(ConditionNodeData data) {
            var result = _dialogueParser.CheckCondition(data.variableName);
            if (result == -1) {
                MWarn.InvalidPropertyName.Throw<InvalidPropertyException>();
                StopDialogue();
                return;
            }
            ClearButtons();
            Next(result);
        }

        private void UpdateUI(DialogueNodeData data) {
            if (_dialogueParser is null) return;
            textDisplay.text = _dialogueParser.GetText(data);
            BuildButtons(data);
            //if (!displayActorNames) return;
            
        }
        
        private void ProcessBasicNode(BasicNodeData data) {
            if (!focusCameraOnActor || data.choice == IntegerZero) return;
            switch (data.choice) {
                case 1: FocusPlayer();
                    break;
                case 2: FocusActor();
                    break;
            }
        }
        
        private void ProcessFocusNode(FocusNodeData data) {
            if (!focusCameraOnActor) return;
            switch (data.choice) {
                case 0: FocusPlayer();
                    break;
                case 1: FocusActor();
                    break;
            }
        }

        private void BuildButtons(DialogueNodeData node) {
            if (node is ChoiceNodeData choiceNode) {
                var choices = choiceNode.choices;
                foreach (var choice in choices) {
                    var obj = Instantiate(choiceButtonPrefab, choiceButtonParent);
                    var buttonObj = obj.GetComponent<DialogueButton>();
                    buttonObj.SetAction(choice.Value, choice.Key, Next);
                    _buttonCache.Add(obj);
                }
            } else {
                var obj = Instantiate(choiceButtonPrefab, choiceButtonParent);
                var buttonObj = obj.GetComponent<DialogueButton>();
                buttonObj.SetAction("Continue", IntegerZero, Next);
                _buttonCache.Add(obj);
            }
        }

        private void ClearButtons() {
            if (_buttonCache.Count <= IntegerZero) return;
            foreach (var button in _buttonCache) {
                Destroy(button);
            }
        }

        private void FocusActor() {
            if (_actor == null) {
                Debug.LogWarning("Actor was null when trying to focus on them.");
                return;
            }
            dialogueCamera.transform.LookAt(_actor.transform);
        }

        private void FocusPlayer() {
            dialogueCamera.transform.LookAt(player.transform);
        }

        private static void TriggerEvent(string eventName) {
            var consumers = FindObjectsOfType<MonoBehaviour>().OfType<IDialogueEventConsumer>();
            foreach (var consumer in consumers) {
                consumer.ConsumeDialogueEvent(eventName);
            }
        }
        
        public static void SetPlayer(GameObject player) {
            if (player == null) {
                MWarn.PlayerNotFound.Print();
            }
            else instance.player = player;
        }

        public static void SetProperty(string key, object value) {
            var candidate = GetDialogueWithProperty(key);
            SetProperty(candidate, key, value);
            if (instance._currentDialogueServer == null) return;
            var active = candidate == instance._currentDialogueServer.dialogueData ? instance._currentDialogueServer.dialogueData : null;
            if (active == null) return;
            SetProperty(active, key, value);
            instance._dialogueParser.UpdateReplacer(key, value);
        }

        public static int GetIntProperty(string propertyName) {
            var candidate = GetDialogueWithProperty(propertyName);
            return candidate.intProperties.First(prop => propertyName.Equals(prop.propertyName)).propertyValue;
        }
        
        public static string GetStringProperty(string propertyName) {
            var candidate = GetDialogueWithProperty(propertyName);
            return candidate.stringProperties.First(prop => propertyName.Equals(prop.propertyName)).propertyValue;
        }
        
        public static bool GetBoolProperty(string propertyName) {
            var candidate = GetDialogueWithProperty(propertyName);
            return candidate.boolProperties.First(prop => propertyName.Equals(prop.propertyName)).propertyValue;
        }

        private static void SetProperty(DialogueData data, string key, object value) {
            switch (value) {
                case int intProp:
                    data.intProperties.ForEach(prop => {
                        if(key == prop.propertyName)
                            prop.propertyValue = intProp;
                    });
                    break;
                
                case bool boolProp:
                    data.boolProperties.ForEach(prop => {
                        if(key == prop.propertyName)
                            prop.propertyValue = boolProp;
                    });
                    break;
                
                case string stringProp:
                    data.stringProperties.ForEach(prop => {
                        if(key == prop.propertyName)
                            prop.propertyValue = stringProp;
                    });
                    break;
                
                default:
                    MWarn.UnsuccessfulBoolParse.Throw<PropertyParseException>();
                    return;
            }
        }

        private static DialogueData GetDialogueWithProperty(string propertyName) {
            var manager = instance;
            List<DialogueData> candidates = new();
            foreach (var data in manager._dialogueDataCache) {
                var properties = data.GetAllProperties();
                candidates.AddRange(properties.Where(property => property.propertyName == propertyName).Select(_ => data));
            }
            
            switch (candidates.Count) {
                case 0:
                    MWarn.InvalidPropertyName.Throw<InvalidPropertyException>();
                    break;
                case > 1:
                    MWarn.AmbiguousPropertyName.Throw<AmbiguousPropertyException>();
                    break;
            }

            return candidates[0];
        }

    }
}