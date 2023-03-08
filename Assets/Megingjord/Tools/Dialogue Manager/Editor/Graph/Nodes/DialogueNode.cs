using System;
using Megingjord.Tools.Dialogue_Manager.Editor.Util;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Node = UnityEditor.Experimental.GraphView.Node;

namespace Megingjord.Tools.Dialogue_Manager.Editor.Graph.Nodes {
    public class DialogueNode : Node {
        private const float Width = 1000f;
        private const float Height = 750f;

        protected readonly DialogueGraphView view;

        private readonly TextField _textField; 
        public string text;
        public string guid;

        protected DialogueNode(DialogueGraphView view, string nodeTitle, bool textConsumer = true) {
            title = nodeTitle;
            this.view = view;
            guid = Guid.NewGuid().ToString();
            
            if (!textConsumer) return;
                _textField = new TextField("") {
                multiline = true
            };
                
            _textField.RegisterValueChangedCallback(evt => {
                text = evt.newValue;
            });
            
            var label = new Label("Dialogue Text");
            label.AddToClassList("node-label");
            mainContainer.Add(label);
            mainContainer.Add(_textField);
        }

        public sealed override string title {
            get => base.title;
            set => base.title = value;
        }

        public void PlaceAt(Vector2 position) {
            SetPosition(new Rect(position.x, position.y, Width, Height));
        }

        public void SetText(string textValue) {
            if (string.IsNullOrWhiteSpace(textValue)) return;
            text = textValue;
            _textField?.SetValueWithoutNotify(textValue);
        }

        protected void AddInPort(Color colour, string portName = "In") {
            var inPort = NodeUtils.GenerateTypelessPort(this, Direction.Input, Port.Capacity.Multi);
            inPort.portName = portName;
            inPort.portColor = colour;
            inputContainer.Add(inPort);
        }

        protected void AddOutPort(Color colour, string portName = "Out") {
            var outPort = NodeUtils.GenerateTypelessPort(this, Direction.Output, Port.Capacity.Multi);
            outPort.portName = portName;
            outPort.portColor = colour;
            outputContainer.Add(outPort);
        }
        
        protected Port BuildPort(Direction nodeDirection,
            Port.Capacity capacity = Port.Capacity.Single) {
            return NodeUtils.GenerateTypelessPort(this, nodeDirection, capacity);
        }

        public Vector2 GetPositionVector() {
            var rect = GetPosition();
            return new Vector2(rect.x, rect.y);
        }

    }
}