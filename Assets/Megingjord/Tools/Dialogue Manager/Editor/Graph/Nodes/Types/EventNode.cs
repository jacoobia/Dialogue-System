using Megingjord.Shared.Reflection;
using Megingjord.Shared.Reflection.Attributes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Megingjord.Tools.Dialogue_Manager.Editor.Graph.Nodes.Types {
    [Node("Event Node")]
    public sealed class EventNode : DialogueNode {

        private string _eventName = "event";
        private readonly TextField _textField;

        public EventNode(DialogueGraphView view) : base(view, "Event Node", false) {
            AddInPort(Color.cyan);
            AddOutPort(Color.cyan);
            
            _textField = new TextField {
                name = string.Empty,
                value = "event "
            };
            
            _textField.RegisterValueChangedCallback(evt => _eventName = evt.newValue);
            
            var label = new Label("Event Name");
            label.AddToClassList("node-label");
            mainContainer.Add(label);
            mainContainer.Add(_textField);
        }

        public void SetEventName(string value) {
            _eventName = value;
            _textField.value = value;
        }
        
        public string GetEventName() {
            return _eventName;
        }
    }
}