using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Megingjord.Tools.Dialogue_Manager.API.Exposed.UI {
    [RequireComponent(typeof(Button))]
    public class DialogueButton : MonoBehaviour {
        
        public TMP_Text buttonText;

        private Button _button;
        private Action<int> _action;
        private int _portIndex;

        public void SetAction(string text, int portIndex, Action<int> action) {
            _action = action;
            _portIndex = portIndex;
            buttonText.text = text;
        }
        
        public void Click() {
            _action.Invoke(_portIndex);
        }
    }
}