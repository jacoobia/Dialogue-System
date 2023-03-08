using System;
using System.Collections.Generic;
using System.Linq;
using Megingjord.Shared.Helpers;
using Megingjord.Tools.Dialogue_Manager.API.Core.Data;
using Megingjord.Tools.Dialogue_Manager.API.Core.Properties;
using Megingjord.Tools.Dialogue_Manager.Editor.Graph.Nodes;
using Megingjord.Tools.Dialogue_Manager.Editor.Graph.Nodes.Types;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static Megingjord.Tools.Dialogue_Manager.PrimitiveUtils;
using Edge = UnityEditor.Experimental.GraphView.Edge;
using Vector2 = UnityEngine.Vector2;

namespace Megingjord.Tools.Dialogue_Manager.Editor.Graph {
    public class DialogueGraphView : GraphView {
        private static DialogueGraphView _instance;

        private const float SearchWindowWidthOffset = 240f;
        private const string WindowName = "Graph Editor";
        private readonly string _graphName;

        private readonly NodeSearchMenu _nodeSearch;
        private readonly List<DialogueProperty> _properties = new();
        private DialogueEditorWindow _window;
        private Blackboard _blackboard;
        private Vector2 _lastRightClicked;

        public DialogueGraphView(string graphName) {
            _instance = this;
            _graphName = graphName;
            _nodeSearch = ScriptableObject.CreateInstance<NodeSearchMenu>();
            _nodeSearch.Init(this);
            
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            name = WindowName;
            this.StretchToParentSize();
            
            BuildBlackboard();
            AddManipulators();
            AddGrid();
        }

        /// <summary>
        /// Unsubscribe from all of the events and cleanup any memory in use
        /// </summary>
        public void Destruct() {
            canPasteSerializedData -= CanPaste;
            unserializeAndPaste -= Deserialize;
            serializeGraphElements -= Serialize;
            ClearBlackboard();
        }
        
        /// <summary>
        /// Delete a port from a node, needs to sit on the
        /// graph level because it also needs to make sure
        /// it clears any connections too
        /// </summary>
        /// <param name="node"></param>
        /// <param name="socket"></param>
        public void RemovePort(DialogueNode node, Port socket) {
            var targetEdge = edges
                .Where(x => x.output.portName == socket.portName && x.output.node == socket.node).ToList();
            if (targetEdge.Any()) {
                var edge = targetEdge.First();
                edge.input.Disconnect(edge);
                RemoveElement(targetEdge.First());
            }

            node.outputContainer.Remove(socket);
            node.RefreshPorts();
            node.RefreshExpandedState();
        }

        /// <summary>
        /// Add manipulators to the graph view
        /// </summary>
        private void AddManipulators() {
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            canPasteSerializedData += CanPaste;
            unserializeAndPaste += Deserialize;
            serializeGraphElements += Serialize;
        }

        /// <summary>
        /// Add the grid to the graph view
        /// </summary>
        private void AddGrid() {
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();
        }

        /// <summary>
        /// Builds the blackboard that holds the exposed variables to be used
        /// in external scripts/the API
        /// </summary>
        private void BuildBlackboard() {
            if(_blackboard != null) ClearBlackboard();
            _blackboard = new Blackboard(this) {
                title = _graphName
            };

            _blackboard.Add(new BlackboardSection {title = "Variables"});
            _blackboard.addItemRequested = ShowAddPropertyContextMenu;

            _blackboard.editTextRequested = (_, element, newValue) => {
                var oldPropertyName = ((BlackboardField) element).text;
                if (_properties.Any(x => x.propertyName == newValue)) {
                    MWarn.PropertyExists.Show();
                    return;
                }

                var targetIndex = _properties.FindIndex(x => x.propertyName == oldPropertyName);
                _properties[targetIndex].propertyName = newValue;
                ((BlackboardField) element).text = newValue;
            };

            _blackboard.SetPosition(new Rect(10,30,200,300));
            Add(_blackboard);
        }

        /// <summary>
        /// Builds and shows the context menu when a user clicks
        /// the add or '+' button on the blackboard
        /// </summary>
        /// <param name="blackboard"></param>
        private void ShowAddPropertyContextMenu(Blackboard blackboard) {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Int"), false, () => AddPropertyToBlackboard(new IntProperty()));
            menu.AddItem(new GUIContent("String"), false, () => AddPropertyToBlackboard(new StringProperty()));
            menu.AddItem(new GUIContent("Boolean"), false, () => AddPropertyToBlackboard(new BoolProperty()));
            menu.ShowAsContext();
        }

        /// <summary>
        /// Adds a property to the blackboard, type agnostic
        /// </summary>
        /// <param name="property"></param>
        private void AddPropertyToBlackboard(DialogueProperty property) {
            if (_properties.Contains(property)) return;
            switch (property) {
                case IntProperty intProperty:
                    AddIntProperty(intProperty);
                    break;
                case StringProperty stringProperty:
                    AddStringProperty(stringProperty);
                    break;
                case BoolProperty boolProperty:
                    AddBoolProperty(boolProperty);
                    break;
            }
        }

        /// <summary>
        /// Adds an integer property tot he blackboard
        /// </summary>
        /// <param name="property"></param>
        public void AddIntProperty(IntProperty property) {
            var container = new VisualElement();
            var field = new BlackboardField {text = property.propertyName, typeText = "int"};
            container.Add(field);

            var propertyValueTextField = new TextField("Value:") {
                value = property.propertyValue.ToString()
            };
            propertyValueTextField.RegisterValueChangedCallback(evt => {
                var success = int.TryParse(evt.newValue, out var result);
                if (success) {
                    var foundProperty = (IntProperty)_properties.First(element => element.guid == property.guid);
                    foundProperty.propertyValue = result;
                } else {
                    propertyValueTextField.value = string.Empty;
                }
            });
            var row = new BlackboardRow(field, propertyValueTextField);
            container.Add(row);
            _blackboard.Add(container);
            _properties.Add(property);
        }

        /// <summary>
        /// Adds a string property to the blackboard
        /// </summary>
        /// <param name="property"></param>
        public void AddStringProperty(StringProperty property) {
            var container = new VisualElement();
            var field = new BlackboardField {text = property.propertyName, typeText = "string"};
            container.Add(field);

            var propertyValueTextField = new TextField("Value:") {
                value = property.propertyValue
            };
            propertyValueTextField.RegisterValueChangedCallback(evt => {
                var foundProperty = (StringProperty)_properties.First(element => element.guid == property.guid);
                foundProperty.propertyValue = evt.newValue;
            });
            var row = new BlackboardRow(field, propertyValueTextField);
            container.Add(row);
            _blackboard.Add(container);
            _properties.Add(property);
        }

        /// <summary>
        /// Adds a boolean property to the blackboard
        /// </summary>
        /// <param name="property"></param>
        public void AddBoolProperty(BoolProperty property) {
            var container = new VisualElement();
            var field = new BlackboardField {text = property.propertyName, typeText = "bool"};
            container.Add(field);
            
            var value = new Label(property.propertyValue.ToString());
            value.AddToClassList(BaseField<string>.labelUssClassName);
            
            var row = new BlackboardRow(field, value);
            container.Add(row);
            _blackboard.Add(container);
            _properties.Add(property);
        }
        
        /// <summary>
        /// Completely clears the blackboard of all properties
        /// and removes it
        /// </summary>
        private void ClearBlackboard() {
            _properties.Clear();
            _blackboard.Clear();
            _blackboard = null;
        }
        
        /// <summary>
        /// Gets a list of ports that are compatible with the out port
        /// that a connection is currently being drawn from
        /// </summary>
        /// <param name="startPort"></param>
        /// <param name="nodeAdapter"></param>
        /// <returns></returns>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            return ports.Where(port => startPort != port && startPort.node != port.node).ToList();
        }

        /// <summary>
        /// An override for the context menu that adds the node
        /// creation menu option to the bottom
        /// </summary>
        /// <param name="event"></param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent @event) {
            base.BuildContextualMenu(@event);
            _lastRightClicked = GetMousePosition();
            var mouse = @event.mousePosition;
            if (@event.target is not GraphView) return;
            @event.menu.AppendAction("Create Node", _ => {
                var offset = new Vector2(mouse.x + SearchWindowWidthOffset, mouse.y);
                SearchWindow.Open(new SearchWindowContext(offset), _nodeSearch);
            });
            @event.menu.AppendSeparator();
        }

        /// <summary>
        /// Creates a node of a given type at the last cached
        /// mouse position
        /// </summary>
        /// <param name="type">The type of node to create</param>
        /// <returns>Creation result</returns>
        public bool TryCreateNode(Type type) {
            try {
                var node = (DialogueNode)Activator.CreateInstance(type, this);
                node.PlaceAt(_lastRightClicked);
                AddElement(node);
                return true;
            } catch (Exception) {
                return false;
            }
        }

        /// <summary>
        /// Creates a node in memory
        /// </summary>
        /// <typeparam name="T">The type of node</typeparam>
        /// <returns></returns>
        public T CreateNode<T>() where T : DialogueNode {
            return (T)Activator.CreateInstance(typeof(T), this);
        }

        /// <summary>
        /// Links two ports together
        /// </summary>
        /// <param name="outputSocket">The out port of the output node</param>
        /// <param name="inputSocket">The in port of the input node</param>
        private void LinkNodesTogether(Port outputSocket, Port inputSocket) {
            var tempEdge = new Edge {
                output = outputSocket,
                input = inputSocket
            };
            tempEdge.input.Connect(tempEdge);
            tempEdge.output.Connect(tempEdge);
            Add(tempEdge);
        }

        /// <summary>
        /// Adds an example template of nodes to the graph,
        /// used for the first time opening a new dialogue graph
        /// </summary>
        public void AddExampleNodes() {
            var entry = new EntryNode(this);
            var example = new BasicNode(this);
            var exit = new ExitNode(this);

            entry.PlaceAt(EntryNode.ExamplePosition);
            example.PlaceAt(FocusActorNode.ExamplePosition);
            exit.PlaceAt(ExitNode.ExamplePosition);
            
            example.SetText("This is an example dialogue stage!");
            
            AddElement(entry);
            AddElement(example);
            AddElement(exit);
            
            var entryPort = entry.outputContainer[IntegerZero].Q<Port>();
            var basicOut = example.outputContainer[IntegerZero].Q<Port>();
            var basicIn = example.inputContainer[IntegerZero].Q<Port>();
            var exitPort = exit.inputContainer[IntegerZero].Q<Port>();
            
            LinkNodesTogether(entryPort, basicIn);
            LinkNodesTogether(basicOut, exitPort);
        }

        /// <summary>
        /// Can the user paste the clipboard data onto the graph?
        /// TODO Change this to check if it's a parseable graph data object
        /// </summary>
        /// <param name="serializedData"></param>
        /// <returns></returns>
        private static bool CanPaste(string serializedData) {
            return true;
        }

        /// <summary>
        /// Deserializes the string payload in the clipboard from a paste
        /// command, this assumed the data is in a serialized JSON format
        /// </summary>
        /// <param name="action">The action description</param>
        /// <param name="data">The data in the clipboard</param>
        private static void Deserialize(string action, string data) {
            var dataObj = ScriptableObject.CreateInstance<DialogueData>();
            JsonUtility.FromJsonOverwrite(data, dataObj);
            DialogueGraphIO.LoadGraphDataAdditive(dataObj, _instance);
        }

        /// <summary>
        /// Serializes the selected data to JSON on a copy or cut
        /// command on the graph
        /// </summary>
        /// <param name="elements">The graph elements to serialize</param>
        /// <returns></returns>
        private static string Serialize(IEnumerable<GraphElement> elements) {
            var data = ScriptableObject.CreateInstance<DialogueData>();
            foreach (var element in elements) {
                switch (element) {
                    case DialogueNode node:
                        DialogueGraphIO.SaveNode(ref data, node);
                        break;
                    case Edge edge:
                        DialogueGraphIO.SaveLink(ref data, edge);
                        break;
                }
            }
            return JsonUtility.ToJson(data);
        }

        /// <summary>
        /// Shorthand for getting the mouse position from the window
        /// </summary>
        /// <returns>The mouse position including offset</returns>
        private Vector2 GetMousePosition() {
            return _window.GetMousePosition();
        }

        /// <summary>
        /// An init method to add a reference to the window
        /// that contains this graph
        /// </summary>
        /// <param name="window">The parent window</param>
        public void SetWindowReference(DialogueEditorWindow window) {
            _window = window;
        }

        /// <summary>
        /// Gets all of the properties currently on a graph
        /// </summary>
        /// <returns></returns>
        public List<DialogueProperty> GetProperties() {
            return _properties;
        }
    }
}