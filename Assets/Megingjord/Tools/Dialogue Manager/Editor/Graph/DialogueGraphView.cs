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
using Vector2 = UnityEngine.Vector2;

namespace Megingjord.Tools.Dialogue_Manager.Editor.Graph {
    public class DialogueGraphView : GraphView {

        private const float SearchWindowWidthOffset = 240f;
        private const string WindowName = "Graph Editor";
        private readonly string _graphName;

        private readonly List<DialogueProperty> _properties = new();

        private DialogueEditorWindow _window;
        private Blackboard _blackboard;
        
        private readonly NodeSearchMenu _nodeSearch;

        public DialogueGraphView(string graphName) {
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
        /// Unsubscribe from all of the events
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
        ///
        /// TODO Finish
        /// </summary>
        private void BuildBlackboard() {
            if(_blackboard != null) ClearBlackboard();
            _blackboard = new Blackboard(this) {
                title = _graphName
            };

            _blackboard.Add(new BlackboardSection {title = "Variables"});
            _blackboard.addItemRequested = AddItemRequested;

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

        private void AddItemRequested(Blackboard blackboard) {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Int"), false, () => AddPropertyToBlackboard(new IntProperty()));
            menu.AddItem(new GUIContent("String"), false, () => AddPropertyToBlackboard(new StringProperty()));
            menu.AddItem(new GUIContent("Boolean"), false, () => AddPropertyToBlackboard(new BoolProperty()));
            menu.ShowAsContext();
        }

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
        /// creation menu
        /// </summary>
        /// <param name="event"></param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent @event) {
            base.BuildContextualMenu(@event);
            var mouse = @event.mousePosition;
            if (@event.target is not GraphView) return;
            @event.menu.AppendAction("Create Node", _ => {
                var offset = new Vector2(mouse.x + SearchWindowWidthOffset, mouse.y);
                SearchWindow.Open(new SearchWindowContext(offset), _nodeSearch);
            });
            @event.menu.AppendSeparator();
        }

        public bool CreateNodeAtMouse(Type type) {
            try {
                var node = (DialogueNode)Activator.CreateInstance(type, this);
                node.PlaceAt(GetMousePosition());
                AddElement(node);
                return true;
            } catch (Exception) {
                return false;
            }
        }
        
        /// <summary>
        /// Creates a new node at the position of the mouse and
        /// then adds it to the grid view
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void CreateNodeAtMouse<T>() where T : DialogueNode {
            var node = CreateNode<T>();
            node.PlaceAt(GetMousePosition());
            AddElement(node);
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
        /// Links two node ports together
        /// </summary>
        /// <param name="outputSocket"></param>
        /// <param name="inputSocket"></param>
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
        /// Adds the entry node and also an example basic node
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
            
            LinkExampleNodes(entry, example, exit);
        }
        
        /// <summary>
        /// Links the example nodes together
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="basic"></param>
        /// <param name="exit"></param>
        private void LinkExampleNodes(Node entry, Node basic, Node exit) {
            var entryPort = entry.outputContainer[IntegerZero].Q<Port>();
            var basicOut = basic.outputContainer[IntegerZero].Q<Port>();
            var basicIn = basic.inputContainer[IntegerZero].Q<Port>();
            var exitPort = exit.inputContainer[IntegerZero].Q<Port>();
            
            LinkNodesTogether(entryPort, basicIn);
            LinkNodesTogether(basicOut, exitPort);
        }

        private bool CanPaste(string serializedData) {
            return true;
        }

        private static void Deserialize(string action, string data) {
            var dataObj = ScriptableObject.CreateInstance<DialogueData>();
            // todo this needs to deserialize the clipboard and place nodes & edges
            Debug.Log(data);
        }

        private static string Serialize(IEnumerable<GraphElement> elements) {
            var data = ScriptableObject.CreateInstance<DialogueData>();
            foreach (var element in elements) {
                switch (element) {
                    case DialogueNode node:
                        node.guid = Guid.NewGuid().ToString();
                        DialogueGraphIO.SaveNode(ref data, node);
                        break;
                    case Edge edge:
                        DialogueGraphIO.SaveConnection(ref data, edge);
                        break;
                }
            }

            return data.ToString();
        }

        private Vector2 GetMousePosition() {
            return _window.GetMousePosition();
        }

        public void SetWindowReference(DialogueEditorWindow window) {
            _window = window;
        }

        public List<DialogueProperty> GetProperties() {
            return _properties;
        }
    }
}