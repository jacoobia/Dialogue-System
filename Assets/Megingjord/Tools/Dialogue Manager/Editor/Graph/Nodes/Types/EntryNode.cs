using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Megingjord.Tools.Dialogue_Manager.Editor.Graph.Nodes.Types {
    public sealed class EntryNode : DialogueNode {
        
        public static readonly Vector2 ExamplePosition = new(500, 350);

        public EntryNode(DialogueGraphView view) : base(view, "Entry Point", false) {
            AddOutPort(Color.green, "Start");

            capabilities = Capabilities.Selectable 
                           | Capabilities.Movable
                           | Capabilities.Ascendable 
                           | Capabilities.Copiable
                           | Capabilities.Snappable 
                           | Capabilities.Groupable;
            
            RefreshExpandedState();
            RefreshPorts();
        }

    }
}