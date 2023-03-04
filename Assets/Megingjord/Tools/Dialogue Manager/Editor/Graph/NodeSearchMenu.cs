using System;
using System.Collections.Generic;
using Megingjord.Shared.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Megingjord.Tools.Dialogue_Manager.Editor.Graph {
    public class NodeSearchMenu : ScriptableObject, ISearchWindowProvider {

        private const string MenuName = "Nodes";
        private const int DefaultMenuItemLevel = 1;
        
        private DialogueGraphView _graphView;
        
        public void Init(DialogueGraphView graphView) {
            _graphView = graphView;
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
            var searchTree = new List<SearchTreeEntry> { new SearchTreeGroupEntry(new GUIContent(MenuName)) };
            var nodeTypes = ReflectionUtils.GetNodeTypeDictionary();
            foreach (var (type, text) in nodeTypes) {
                searchTree.Add(new SearchTreeEntry(new GUIContent(text)) {
                    level = DefaultMenuItemLevel,
                    userData = Activator.CreateInstance(type, _graphView)
                });
            }
            return searchTree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
            return _graphView.CreateNodeAtMouse(searchTreeEntry.userData.GetType());
        }
            
    }
}