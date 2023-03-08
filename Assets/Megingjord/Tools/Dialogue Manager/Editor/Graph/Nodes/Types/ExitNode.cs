using Megingjord.Shared.Reflection;
using Megingjord.Shared.Reflection.Attributes;
using UnityEngine;

namespace Megingjord.Tools.Dialogue_Manager.Editor.Graph.Nodes.Types {
    [Node("Exit Node")]
    public sealed class ExitNode : DialogueNode {
        
        public static readonly Vector2 ExamplePosition = new(900, 350);
        public ExitNode(DialogueGraphView view) : base(view, "Exit Node", false) {
            AddInPort(Color.red, "Exit");
            RefreshExpandedState();
            RefreshPorts();
        }
        
    }
}