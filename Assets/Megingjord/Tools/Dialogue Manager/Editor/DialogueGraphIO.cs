using System;
using System.Collections.Generic;
using System.Linq;
using Megingjord.Shared;
using Megingjord.Tools.Dialogue_Manager.API.Core.Data;
using Megingjord.Tools.Dialogue_Manager.API.Core.Properties;
using Megingjord.Tools.Dialogue_Manager.Editor.Graph;
using Megingjord.Tools.Dialogue_Manager.Editor.Graph.Nodes;
using Megingjord.Tools.Dialogue_Manager.Editor.Graph.Nodes.Types;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static Megingjord.Tools.Dialogue_Manager.PrimitiveUtils;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace Megingjord.Tools.Dialogue_Manager.Editor {
    
    public static class DialogueGraphIO {

        private const int PasteOffsetX = 100;
        private const int PasteOffsetY = 50;

        /// <summary>
        /// Creates a new DialogueData object, saves the data from a graph
        /// into it and then uses the AssetIO class to save the asset
        /// </summary>
        /// <param name="view"></param>
        /// <param name="fileName"></param>
        public static void SaveAsset(DialogueGraphView view, string fileName) {
            var dialogue = ScriptableObject.CreateInstance<DialogueData>();
            if (!SaveGraphData(ref dialogue, view)) return;
            AssetIO.SaveDialogueAsset(dialogue, fileName);
        }

        /// <summary>
        /// Reads a graph and saves the data into a DialogueData object 
        /// </summary>
        /// <param name="dialogueData">The data object to populate</param>
        /// <param name="graph">The graph to read the data from</param>
        /// <returns></returns>
        private static bool SaveGraphData(ref DialogueData dialogueData, DialogueGraphView graph) {
            var edges = graph.edges.ToList();
            if (!edges.Any()) return false;
            var connectedSockets = edges.Where(x => x.input.node != null).ToArray();
            foreach (var edge in connectedSockets) {
                SaveLink(ref dialogueData, edge);
            }
            
            var nodes = graph.nodes.ToList().Cast<DialogueNode>().ToList();
            foreach (var node in nodes) {
                SaveNode(ref dialogueData, node);
            }
            
            foreach (var property in graph.GetProperties()) {
                SaveProperty(ref dialogueData, property);
            }

            return true;
        }

        /// <summary>
        /// Saves a property into the dialogue
        /// </summary>
        /// <param name="dialogueData"></param>
        /// <param name="property"></param>
        private static void SaveProperty(ref DialogueData dialogueData, DialogueProperty property) {
            switch (property) {
                case StringProperty stringProperty:
                    dialogueData.stringProperties.Add(stringProperty);
                    break;
                
                case IntProperty intProperty:
                    dialogueData.intProperties.Add(intProperty);
                    break;
                
                case BoolProperty boolProperty:
                    dialogueData.boolProperties.Add(boolProperty);
                    break;
            }
        }

        /// <summary>
        /// Saves a link between two ports, known in the unity api as an 'Edge',
        /// the data that is saves is just the ID's of the nodes & the names
        /// for the target ports
        /// </summary>
        /// <param name="dialogueData"></param>
        /// <param name="edge"></param>
        public static void SaveLink(ref DialogueData dialogueData, Edge edge) {
            var outputNode = edge.output.node as DialogueNode;
            var inputNode = edge.input.node as DialogueNode;

            var index = GetOutPortIndex(outputNode, edge);

            dialogueData.linkData.Add(new NodeLinkData {
                outNodeGuid = outputNode!.guid,
                portIndex = index,
                inNodeGuid = inputNode!.guid
            });
        }

        /// <summary>
        /// Converts choice and conditions node ports to index values to
        /// link to by position in a list
        /// Uses a cheeky conversion and  flip approach for condition nodes
        /// Simple yet effective
        /// </summary>
        /// <param name="node">The node the ports belong to</param>
        /// <param name="edge">The edge used to identify the port</param>
        /// <returns></returns>
        private static int GetOutPortIndex(DialogueNode node, Edge edge) {
            switch (node) {
                case ChoiceNode:
                    return Convert.ToInt32(edge.output.portName);
                    
                case ConditionNode: {
                    //  Invert the port name bool because true appears first ib the list
                    var value  = !Convert.ToBoolean(edge.output.portName);
                    return Convert.ToInt32(value);
                }
                default:
                    return 0;
            }
        }
        
        
        /// <summary>
        /// Saves a node into a dialogue data object
        /// </summary>
        /// <param name="dialogueData">The save data object</param>
        /// <param name="node">The node to save</param>
        public static void SaveNode(ref DialogueData dialogueData, DialogueNode node) {
            switch (node) {
                case BasicNode basicNode:
                    dialogueData.basicNodeData.Add(new BasicNodeData {
                        guid = basicNode.guid,
                        position = basicNode.GetPositionVector(),
                        text = basicNode.text,
                        choice = basicNode.GetChoice()
                    });
                    break;
                case FocusActorNode focusNode:
                    dialogueData.focusNodeData.Add(new FocusNodeData {
                        guid = focusNode.guid,
                        position = focusNode.GetPositionVector(),
                        text = focusNode.text,
                        choice = focusNode.GetChoice()
                    });
                    break;
                case ConditionNode conditionNode:
                        dialogueData.conditionNodeData.Add(new ConditionNodeData {
                            guid = conditionNode.guid,
                            position = conditionNode.GetPositionVector(),
                            text = conditionNode.text,
                            variableName = conditionNode.GetVariableName()
                        });
                    break;
                case ChoiceNode choiceNode:
                    dialogueData.choiceNodeData.Add(new ChoiceNodeData {
                        guid = choiceNode.guid,
                        position = choiceNode.GetPositionVector(),
                        text = choiceNode.text,
                        choices = choiceNode.GetOptionList()
                    });
                    break;
                case ExitNode exitNode:
                    dialogueData.exitNodeData.Add(new ExitNodeData {
                        guid = exitNode.guid,
                        position = exitNode.GetPositionVector()
                    });
                    break;
                case EventNode eventNode:
                    dialogueData.eventNodeData.Add(new EventNodeData {
                        guid = eventNode.guid,
                        position = eventNode.GetPositionVector(),
                        eventName = eventNode.GetEventName()
                    });
                    break;
                case EntryNode entryNode:
                    dialogueData.entryGuid = entryNode.guid;
                    dialogueData.entryNodePosition = entryNode.GetPositionVector();
                    break;
            }
        }

        /// <summary>
        /// Loads data into a new graph
        /// </summary>
        /// <param name="data">The data to load</param>
        /// <returns>A populated graph</returns>
        public static DialogueGraphView LoadGraphDataInstantiate(DialogueData data) {
            var graph = new DialogueGraphView(data.name);
            graph.styleSheets.Add(DialogueEditorWindow.graphStyleSheet);
            if (data.firstTimeOpen) {
                graph.AddExampleNodes();
                return graph;
            }
            PopulateGraph(graph, data);
            return graph;
        }

        /// <summary>
        /// Additively load data into an existing graph from a data object
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="data"></param>
        public static void LoadGraphDataAdditive(DialogueData data, DialogueGraphView graph) {
            var guidMap = ReassignAndOffset(ref data);
            RemapGuidsForLinks(ref data, guidMap);
            PopulateGraph(graph, data);
        }

        /// <summary>
        /// Remap the the GUIDs for node links based on a key of remapped GUIDs
        /// </summary>
        /// <param name="data">The data to update</param>
        /// <param name="map">The GUID map</param>
        private static void RemapGuidsForLinks(ref DialogueData data, IReadOnlyDictionary<string, string> map) {
            foreach (var linkData in data.linkData) {
                var inGuid = linkData.inNodeGuid;
                if (map.ContainsKey(inGuid)) {
                    linkData.inNodeGuid = map[inGuid];
                }

                var outGuid = linkData.outNodeGuid;
                if (!map.ContainsKey(outGuid)) continue;
                Debug.Log($"before: {outGuid} after: {map[outGuid]}");
                linkData.outNodeGuid = map[outGuid];
            }
        }
        
        /// <summary>
        /// Reassign the GUIDs of the nodes in a data object and then offset
        /// them by a given amount of the original node positions
        /// </summary>
        /// <param name="data">The data to clone</param>
        /// <returns></returns>
        private static Dictionary<string, string> ReassignAndOffset(ref DialogueData data) {
            Dictionary<string, string> guidMap = new();
            var nodes = data.GetAllNodes();
            foreach (var nodeData in nodes) {
                var currentGuid = nodeData.guid;
                var newGuid = Guid.NewGuid().ToString();
                guidMap.Add(currentGuid, newGuid);
                nodeData.guid = newGuid;
                nodeData.position.x += PasteOffsetX;
                nodeData.position.y += PasteOffsetY;
            }

            return guidMap;
        }
        
        /// <summary>
        /// Populate a graph with data
        /// </summary>
        /// <param name="graph">The graph to populate</param>
        /// <param name="data">The data to populate the graph with</param>
        private static void PopulateGraph(DialogueGraphView graph, DialogueData data) {
            var entry = new EntryNode(graph) {
                guid = data.entryGuid
            };
            entry.PlaceAt(data.entryNodePosition);
            graph.AddElement(entry);

            // Load the basic nodes
            LoadBasicNodes(data, graph);
            LoadChoiceNodes(data, graph);
            LoadFocusNodes(data, graph);
            LoadConditionNodes(data, graph);
            LoadExitNodes(data, graph);
            LoadEventNodes(data, graph);
            
            ConnectDialogueNodes(data, graph);
            LoadProperties(data, graph);
        }
        
        /// <summary>
        /// Load each of the basic dialogue nodes
        /// </summary>
        /// <param name="dialogueData">The data container</param>
        /// <param name="graph">The graph</param>
        private static void LoadBasicNodes(DialogueData dialogueData, DialogueGraphView graph) {
            foreach (var nodeData in dialogueData.basicNodeData) {
                var tempNode = graph.CreateNode<BasicNode>();
                tempNode.SetText (nodeData.text);
                tempNode.guid = nodeData.guid;
                tempNode.PlaceAt(nodeData.position);
                graph.AddElement(tempNode);
                tempNode.SetChoice(nodeData.choice);
            }
        }

        /// <summary>
        /// Load each of the choice dialogue nodes
        /// </summary>
        /// <param name="dialogueData">The data container</param>
        /// <param name="graph">The graph</param>
        private static void LoadChoiceNodes(DialogueData dialogueData, DialogueGraphView graph) {
            foreach (var nodeData in dialogueData.choiceNodeData) {
                var tempNode = graph.CreateNode<ChoiceNode>();
                tempNode.SetText (nodeData.text);
                tempNode.guid = nodeData.guid;
                tempNode.PlaceAt(nodeData.position);
                foreach (var choice in nodeData.choices) {
                    tempNode.AddChoice(choice.Key, choice.Value);
                }

                graph.AddElement(tempNode);
            }
        }
        
        /// <summary>
        /// Load each of the event dialogue nodes
        /// </summary>
        /// <param name="dialogueData">The data container</param>
        /// <param name="graph">The graph</param>
        private static void LoadEventNodes(DialogueData dialogueData, DialogueGraphView graph) {
            foreach (var nodeData in dialogueData.eventNodeData) {
                var tempNode = graph.CreateNode<EventNode>();
                tempNode.guid = nodeData.guid;
                tempNode.SetEventName(nodeData.eventName);
                tempNode.PlaceAt(nodeData.position);
                graph.AddElement(tempNode);
            }
        }
        
        /// <summary>
        /// Load each of the exit dialogue nodes
        /// </summary>
        /// <param name="dialogueData">The data container</param>
        /// <param name="graph">The graph</param>
        private static void LoadExitNodes(DialogueData dialogueData, DialogueGraphView graph) {
            foreach (var nodeData in dialogueData.exitNodeData) {
                var tempNode = graph.CreateNode<ExitNode>();
                tempNode.guid = nodeData.guid;
                tempNode.PlaceAt(nodeData.position);
                graph.AddElement(tempNode);
            }
        }

        /// <summary>
        /// Load each of the focus dialogue nodes
        /// </summary>
        /// <param name="dialogueData">The data container</param>
        /// <param name="graph">The graph</param>
        private static void LoadFocusNodes(DialogueData dialogueData, DialogueGraphView graph) {
            foreach (var nodeData in dialogueData.focusNodeData) {
                var tempNode = graph.CreateNode<FocusActorNode>();
                tempNode.SetText (nodeData.text);
                tempNode.guid = nodeData.guid;
                tempNode.PlaceAt(nodeData.position);
                tempNode.SetChoice(nodeData.choice);
                graph.AddElement(tempNode);
            }
        }

        /// <summary>
        /// Load each of the condition dialogue nodes
        /// </summary>
        /// <param name="dialogueData">The data container</param>
        /// <param name="graph">The graph</param>
        private static void LoadConditionNodes(DialogueData dialogueData, DialogueGraphView graph) {
            foreach (var nodeData in dialogueData.conditionNodeData) {
                var tempNode = graph.CreateNode<ConditionNode>();
                tempNode.SetText (nodeData.text);
                tempNode.guid = nodeData.guid;
                tempNode.PlaceAt(nodeData.position);
                tempNode.SetVariableName(nodeData.variableName);
                graph.AddElement(tempNode);
            }
        }

        /// <summary>
        /// Connect the nodes on a graph
        /// </summary>
        /// <param name="dialogueData">The data container</param>
        /// <param name="graph">The graph</param>
        private static void ConnectDialogueNodes(DialogueData dialogueData, GraphView graph) {
            foreach (var link in dialogueData.linkData) {
                var outNode = GetNode(graph, link.outNodeGuid);
                var inNode = GetNode(graph, link.inNodeGuid);

                if (inNode == null || outNode == null) return;
                
                var outPorts = outNode.outputContainer.Query("connector").ToList();

                var fromPort = outNode.outputContainer[IntegerZero].Q<Port>();
                var toPort = inNode.inputContainer[IntegerZero].Q<Port>();

                if (outPorts.Count > 1) {
                    for (var i = 0; i < outPorts.Count; i++) {
                        var port = outNode.outputContainer[i].Q<Port>();
                        if (i == link.portIndex)
                            fromPort = port;
                    }
                }
                    
                LinkNodesTogether(fromPort, toPort, graph);
            }
        }

        /// <summary>
        /// Load properties from a dialogue data object into the blackboard of a graph
        /// </summary>
        /// <param name="dialogueData">The data</param>
        /// <param name="graph">The graph</param>
        private static void LoadProperties(DialogueData dialogueData, DialogueGraphView graph) {
            foreach (var intProperty in dialogueData.intProperties) {
                graph.AddIntProperty(intProperty);
            }

            foreach (var stringProperty in dialogueData.stringProperties) {
                graph.AddStringProperty(stringProperty);
            }
            
            foreach (var boolProperty in dialogueData.boolProperties) {
                graph.AddBoolProperty(boolProperty);
            }
        }

        /// <summary>
        /// Get a node from a graph from a given GUID 
        /// </summary>
        /// <param name="graph">The graph to search</param>
        /// <param name="guid">The GUID to search for</param>
        /// <returns></returns>
        private static DialogueNode GetNode(GraphView graph, string guid) {
            var nodes = graph.nodes.ToList().Cast<DialogueNode>().ToList();
            return nodes.FirstOrDefault(dialogueNode => dialogueNode.guid.Equals(guid));
        }
        
        /// <summary>
        /// Forces two ports to be linked together on a graph
        /// </summary>
        /// <param name="outputSocket">The output port</param>
        /// <param name="inputSocket">The input port</param>
        /// <param name="graph">The graph</param>
        private static void LinkNodesTogether(Port outputSocket, Port inputSocket, VisualElement graph) {
            var tempEdge = new Edge {
                output = outputSocket,
                input = inputSocket
            };
            tempEdge.input.Connect(tempEdge);
            tempEdge.output.Connect(tempEdge);
            graph.Add(tempEdge);
        }

    }
}