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
        
        /// <summary>
        /// Use reflection to get all of the Node base classes and
        /// create a GUI content for each of them, encapsulate it in
        /// a SearchTreeGroupEntry and add it to the tree list.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
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

        /// <summary>
        /// When an element is selected in the search window,
        /// Then create a node based on the option selected
        /// </summary>
        /// <param name="searchTreeEntry"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
            return _graphView.TryCreateNode(searchTreeEntry.userData.GetType());
        }
            
    }
}