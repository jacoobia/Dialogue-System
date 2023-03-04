using System.Collections.Generic;
using Megingjord.Shared.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Megingjord.Tools.Dialogue_Manager.Editor.Graph.Nodes.Types {
    [Node("Choice Node")]
    public sealed class ChoiceNode : DialogueNode {

        private readonly List<TextField> _textFields = new();

        public ChoiceNode(DialogueGraphView view) : base(view, "Choice Node") {
            titleButtonContainer.Add(new Button(() => AddChoice()) { text = "Add Choice" });
            
            AddInPort(Color.cyan);
        }
        
        public void AddChoice(int indexOverride = -1, string portValueOverride = "") {
            var outputPortCount = outputContainer.Query("connector").ToList().Count;
            var index = indexOverride != -1 ? indexOverride : outputPortCount;
            var outputPortValue = string.IsNullOrEmpty(portValueOverride) ?
                $"Option {index + 1}" : portValueOverride;
            
            var generatedPort = BuildPort(Direction.Output);
            var portLabel = generatedPort.contentContainer.Q<Label>("type");
            generatedPort.contentContainer.Remove(portLabel);
            
            var textField = new TextField {
                name = string.Empty,
                value = outputPortValue
            };
            _textFields.Add(textField);
            
            generatedPort.contentContainer.Add(new Label("  "));
            generatedPort.contentContainer.Add(textField);
            
            var deleteButton = new Button(() => {
                view.RemovePort(this, generatedPort);
                _textFields.Remove(textField);
            }) {
                text = "Remove"
            };
            
            generatedPort.contentContainer.Add(deleteButton);
            generatedPort.portName = index.ToString();
            outputContainer.Add(generatedPort);
            RefreshPorts();
            RefreshExpandedState();
        }

        public SerializedDictionary<int, string> GetOptionList() {
            var dictionary = new SerializedDictionary<int, string>();
            foreach (var field in _textFields) dictionary.Add(_textFields.IndexOf(field), field.value);
            return dictionary;
        }

    }
}   