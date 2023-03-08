using System.Collections.Generic;
using System.Linq;
using Megingjord.Tools.Dialogue_Manager.API.Core.Data;
using Megingjord.Tools.Dialogue_Manager.Editor.Graph.Nodes;
using Megingjord.Tools.Dialogue_Manager.Editor.Graph.Nodes.Types;

namespace Megingjord.Tools.Dialogue_Manager.Editor.Graph {
    /// <summary>
    /// A loader base class to be inherited by subclasses that require some
    /// kind of custom data manipulation, this approach is a little bit
    /// over-engineered probably but I hated having a bunch of for loop functions
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TQ"></typeparam>
    public class NodeLoader<T, TQ> where T : DialogueNode where TQ : DialogueNodeData {

        private readonly DialogueGraphView _graph;

        public NodeLoader(DialogueGraphView graph) {
            _graph = graph;
        }

        /// <summary>
        /// Load the data into the graph and return a list of nodes created
        /// </summary>
        /// <param name="source">The source list</param>
        /// <param name="fullNodeList">The node list to add the nodes to</param>
        public void Load(ref List<TQ> source, ref List<DialogueNode> fullNodeList) {
            fullNodeList.AddRange(source.Select(BuildNode).ToList());
        }

        /// <summary>
        /// Build a node and add it to the graph,
        /// injecting the values it requires 
        /// </summary>
        /// <param name="data">The data for the node</param>
        /// <returns></returns>
        protected virtual T BuildNode(TQ data) {
            var node = _graph.CreateNode<T>();
            node.SetText (data.text);
            node.guid = data.guid;
            node.PlaceAt(data.position);
            
            _graph.AddElement(node);
            return node;
        }
        
    }
    
    /// <summary>
    /// Load a basic nodes
    /// </summary>
    public class BasicNodeLoader : NodeLoader<BasicNode, BasicNodeData> {
        public BasicNodeLoader(DialogueGraphView graph) : base(graph) { }
        protected override BasicNode BuildNode(BasicNodeData data) {
            var node = base.BuildNode(data);
            node.SetChoice(data.choice);
            return node;
        }
    }

    /// <summary>
    /// Load a focus node
    /// </summary>
    public class FocusNodeLoader : NodeLoader<FocusActorNode, FocusNodeData> {
        public FocusNodeLoader(DialogueGraphView graph) : base(graph) { }
        protected override FocusActorNode BuildNode(FocusNodeData data) {
            var node = base.BuildNode(data);
            node.SetChoice(data.choice);
            return node;
        }
    }

    /// <summary>
    /// Load a choice node
    /// </summary>
    public class ChoiceNodeLoader : NodeLoader<ChoiceNode, ChoiceNodeData> {
        public ChoiceNodeLoader(DialogueGraphView graph) : base(graph) { }
        protected override ChoiceNode BuildNode(ChoiceNodeData data) {
            var node = base.BuildNode(data);
            foreach (var choice in data.choices) {
                node.AddChoice(choice.Key, choice.Value);
            }
            return node;
        }
    }

    /// <summary>
    /// Load a condition node
    /// </summary>
    public class ConditionNodeLoader : NodeLoader<ConditionNode, ConditionNodeData> {
        public ConditionNodeLoader(DialogueGraphView graph) : base(graph) { }
        protected override ConditionNode BuildNode(ConditionNodeData data) {
            var node = base.BuildNode(data);
            node.SetVariableName(data.variableName);
            return node;
        }
    }

    /// <summary>
    /// Load an event node
    /// </summary>
    public class EventNodeLoader : NodeLoader<EventNode, EventNodeData> {
        public EventNodeLoader(DialogueGraphView graph) : base(graph) { }
        protected override EventNode BuildNode(EventNodeData data) {
            var node = base.BuildNode(data);
            node.SetEventName(data.eventName);
            return node;
        }
    }

}