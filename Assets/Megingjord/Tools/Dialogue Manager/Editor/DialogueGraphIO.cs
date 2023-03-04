using System;
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

        public static void SaveAsset(DialogueGraphView view, string fileName) {
            var dialogue = ScriptableObject.CreateInstance<DialogueData>();
            if (!SaveGraphData(ref dialogue, view)) return;
            AssetIO.SaveDialogueAsset(dialogue, fileName);
        }


        private static bool SaveGraphData(ref DialogueData dialogueData, DialogueGraphView graph) {
            var edges = graph.edges.ToList();
            if (!edges.Any()) return false;
            var connectedSockets = edges.Where(x => x.input.node != null).ToArray();
            foreach (var edge in connectedSockets) {
                SaveConnection(ref dialogueData, edge);
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

        public static void SaveConnection(ref DialogueData dialogueData, Edge edge) {
            var outputNode = edge.output.node as DialogueNode;
            var inputNode = edge.input.node as DialogueNode;

            var index = GetOutPortIndex(outputNode, edge);

            dialogueData.linkData.Add(new NodeLinkData {
                outNodeGuid = outputNode!.guid,
                portIndex = index,
                inNodeGuid = inputNode!.guid
            });
        }

        private static int GetOutPortIndex(DialogueNode node, Edge edge) {
            switch (node) {
                case ChoiceNode:
                    return Convert.ToInt32(edge.output.portName);
                    
                case ConditionNode: {
                    var value  = !Convert.ToBoolean(edge.output.portName);
                    return Convert.ToInt32(value);
                }
                default:
                    return 0;
            }
        }
        
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

        public static DialogueGraphView LoadAsset(DialogueData dialogueData, string graphName) {
            var graph = new DialogueGraphView(graphName);
            graph.styleSheets.Add(DialogueEditorWindow.graphStyleSheet);
            if (dialogueData.firstTimeOpen) {
                graph.AddExampleNodes();
                return graph;
            }
            
            // Add the entry node
            var entry = new EntryNode(graph) {
                guid = dialogueData.entryGuid
            };
            entry.PlaceAt(dialogueData.entryNodePosition);
            graph.AddElement(entry);

            // Load the basic nodes
            LoadBasicNodes(dialogueData, graph);
            LoadChoiceNodes(dialogueData, graph);
            LoadFocusNodes(dialogueData, graph);
            LoadConditionNodes(dialogueData, graph);
            LoadExitNodes(dialogueData, graph);
            LoadEventNodes(dialogueData, graph);
            
            ConnectDialogueNodes(dialogueData, graph);
            LoadProperties(dialogueData, graph);
            return graph;
        }
        
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
        
        private static void LoadEventNodes(DialogueData dialogueData, DialogueGraphView graph) {
            foreach (var nodeData in dialogueData.eventNodeData) {
                var tempNode = graph.CreateNode<EventNode>();
                tempNode.guid = nodeData.guid;
                tempNode.SetEventName(nodeData.eventName);
                tempNode.PlaceAt(nodeData.position);
                graph.AddElement(tempNode);
            }
        }

        private static void LoadExitNodes(DialogueData dialogueData, DialogueGraphView graph) {
            foreach (var nodeData in dialogueData.exitNodeData) {
                var tempNode = graph.CreateNode<ExitNode>();
                tempNode.guid = nodeData.guid;
                tempNode.PlaceAt(nodeData.position);
                graph.AddElement(tempNode);
            }
        }

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

        private static void ConnectDialogueNodes(DialogueData dialogueData, GraphView graph) {
            foreach (var link in dialogueData.linkData) {
                var outNode = GetNode(graph, link.outNodeGuid);
                var inNode = GetNode(graph, link.inNodeGuid);

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

        private static DialogueNode GetNode(GraphView graph, string guid) {
            var nodes = graph.nodes.ToList().Cast<DialogueNode>().ToList();
            return nodes.FirstOrDefault(dialogueNode => dialogueNode.guid.Equals(guid));
        }
        
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