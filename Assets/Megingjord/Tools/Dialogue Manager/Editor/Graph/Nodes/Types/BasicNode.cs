using System.Collections.Generic;
using Megingjord.Shared.Reflection;
using UnityEditor.UIElements;
using UnityEngine;

namespace Megingjord.Tools.Dialogue_Manager.Editor.Graph.Nodes.Types {
    [Node("Basic Dialogue Node")]
    public sealed class BasicNode : DialogueNode {
        
        private static readonly List<string> Choices = new() { "Don't Focus", "Focus Player", "Focus Actor" };
        
        private readonly PopupField<string> _target;
        private int _choice;
        
        public BasicNode(DialogueGraphView view) : base(view, "Dialogue Node") {
            _target = new PopupField<string>("", Choices, _choice);
            titleButtonContainer.Add(_target);
            text = "";
                        
            AddInPort(Color.cyan);
            AddOutPort(Color.cyan);
            
            RefreshPorts();
            RefreshExpandedState();
        }
        
        public void SetChoice(int nodeDataChoice) {
            _choice = nodeDataChoice;
            _target.value = Choices[_choice];
        }

        public int GetChoice() {
            return Choices.IndexOf(_target.value);
        }
        
    }
}