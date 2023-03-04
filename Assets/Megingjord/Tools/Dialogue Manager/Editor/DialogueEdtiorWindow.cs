using Megingjord.Tools.Dialogue_Manager.API.Core.Data;
using Megingjord.Tools.Dialogue_Manager.Editor.Graph;
using Megingjord.Tools.Dialogue_Manager.Editor.Util;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Megingjord.Tools.Dialogue_Manager.Editor {
    public class DialogueEditorWindow : EditorWindow {

        private const string Title = "Dialogue Editor";
        private const string GraphStyleSheetName = "Graph";
        private const string NodeStyleSheetName = "Graph";

        public static StyleSheet graphStyleSheet;
        private static StyleSheet _windowStyleSheet;
        private static StyleSheet _nodeStyleSheet;
        
        private string _graphPath;
        private DialogueGraphView _graphView;
        private NodeSearchMenu _nodeSearch;
        private DialogueData _dialogueData;

        private string _mousePositionString;
        private Vector2 _mousePosition;
        private bool _showGrid = true;
        private bool _showMousePosition;

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line) {
            var graph = EditorUtility.InstanceIDToObject(instanceID) as DialogueData;
            if (graph == null) return false;

            var window = GetWindow<DialogueEditorWindow>();
            window.titleContent = new GUIContent(Title);
            window.minSize = new Vector2(500, 300);
            window.LoadGraph(graph, AssetDatabase.GetAssetPath(graph), graph.name);
            
            return true;
        }

        private void OnGUI() {
            if (_graphView is null) return;
            var @event = Event.current;
            var worldMousePosition = rootVisualElement.ChangeCoordinatesTo(rootVisualElement.parent, @event.mousePosition - position.position);
            _mousePosition = _graphView.contentViewContainer.WorldToLocal(worldMousePosition);

            if (_showMousePosition) _mousePositionString = $"X: {_mousePosition.x} Y: {_mousePosition.y}";
        }

        private void LoadGraph(DialogueData dialogueData, string pathName, string graphName) {
            if(graphStyleSheet == null)  
                graphStyleSheet = Resources.Load<StyleSheet>(GraphStyleSheetName);
            if(_nodeStyleSheet == null)  
                _nodeStyleSheet = Resources.Load<StyleSheet>(NodeStyleSheetName);
            rootVisualElement.styleSheets.Remove(_windowStyleSheet);
            rootVisualElement.Clear();
            if (_graphView != null) {
                _graphView.Clear();
                _graphView = null;
            }
            _dialogueData = dialogueData;
            _graphPath = pathName;
            _graphView = DialogueGraphIO.LoadAsset(dialogueData, graphName);
            _graphView.styleSheets.Add(_nodeStyleSheet);
            _graphView.SetWindowReference(this);
            rootVisualElement.Add(_graphView);
            BuildToolbar();
        }

        private void OnEnable() {
            if(_windowStyleSheet == null)  
                _windowStyleSheet = Resources.Load<StyleSheet>("Window");
            
            rootVisualElement.styleSheets.Add(_windowStyleSheet);

            var button = new Button(() => {
                var created = CreateInstance<DialogueData>();
                var fileName = FileUtils.IndexedFilename("Dialogue");
                LoadGraph(created, $"Assets/{fileName}.asset", fileName);
            }) {
                text = "Creat Dialogue"
            };

            rootVisualElement.Add(new Label("Double click a dialogue asset to open or create a new one"));
            rootVisualElement.Add(button);
        }

        private void OnDisable() {
            _graphView?.Destruct();
            if ( rootVisualElement.Contains(_graphView)) {
                rootVisualElement.Remove(_graphView);
            }
            rootVisualElement.Clear();
        }

        private void BuildToolbar() {
            var toolbar = new IMGUIContainer(() => {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                if (GUILayout.Button("Save Asset", EditorStyles.toolbarButton)) {
                    DialogueGraphIO.SaveAsset(_graphView, _graphPath);
                }

                GUILayout.Space(6);
                if (GUILayout.Button("Show In Project", EditorStyles.toolbarButton)) {
                    EditorGUIUtility.PingObject(_dialogueData);
                }

                GUILayout.FlexibleSpace();
                if (_showMousePosition)
                    GUILayout.TextArea(_mousePositionString, EditorStyles.toolbarTextField);

                if (GUILayout.Button(_showMousePosition ? "Hide Mouse Pos" : "Show Mouse Pos", EditorStyles.toolbarButton)) {
                    _showMousePosition= !_showMousePosition;
                }
                
                if (GUILayout.Button(_showGrid ? "Hide Grid" : "Show Grid", EditorStyles.toolbarButton)) {
                    _showGrid = !_showGrid;
                    if(!_showGrid) _graphView.styleSheets.Remove(graphStyleSheet);
                    else _graphView.styleSheets.Add(graphStyleSheet);
                }
                
                GUILayout.EndHorizontal();
            });
            rootVisualElement.Add(toolbar);
        }

        public Vector2 GetMousePosition() {
            return _mousePosition;
        }
        
    }
}