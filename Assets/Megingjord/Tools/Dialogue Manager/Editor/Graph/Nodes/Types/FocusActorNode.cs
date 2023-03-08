using System.Collections.Generic;
using Megingjord.Shared.Reflection;
using Megingjord.Shared.Reflection.Attributes;
using UnityEditor.UIElements;
using UnityEngine;

namespace Megingjord.Tools.Dialogue_Manager.Editor.Graph.Nodes.Types {
    /// <summary>
    /// Focus on an actor
    /// </summary>
    [Node("Focus Actor Node")]
    public sealed class FocusActorNode : DialogueNode {

        public static readonly Vector2 ExamplePosition = new(650, 350);

        private static readonly List<string> Choices = new() { "Player", "Actor" };
        
        private readonly PopupField<string> _target;
        private int _choice;
        
        public FocusActorNode(DialogueGraphView view) : base(view, "Focus Actor", false) {
            _target = new PopupField<string>("", Choices, _choice);
            titleButtonContainer.Add(_target);
            
            AddInPort(Color.cyan);
            AddOutPort(Color.cyan);

            RefreshExpandedState();
            RefreshPorts();
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