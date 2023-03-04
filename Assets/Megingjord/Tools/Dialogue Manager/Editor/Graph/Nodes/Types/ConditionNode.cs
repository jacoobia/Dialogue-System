using Megingjord.Shared.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Megingjord.Tools.Dialogue_Manager.Editor.Graph.Nodes.Types {
    [Node("Condition Node")]
    public sealed class ConditionNode : DialogueNode {

        private readonly TextField _variableNameField;

        public ConditionNode(DialogueGraphView view) : base(view, "Condition Node", false) {
            _variableNameField = new TextField();
            var label = new Label("Variable Name");
            label.AddToClassList("node-label");
            mainContainer.Add(label);
            mainContainer.Add(_variableNameField);
            
            AddInPort(Color.cyan);
            AddOutPort(Color.cyan, "True");
            AddOutPort(Color.cyan, "False");     
            
            RefreshPorts();
            RefreshExpandedState();
        }

        public void SetVariableName(string variableName) {
            _variableNameField.value = variableName;
        }

        public string GetVariableName() {
            return _variableNameField.value;
        }
        
    }
}