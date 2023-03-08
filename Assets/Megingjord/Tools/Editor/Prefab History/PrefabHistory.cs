using System;
using System.Collections.Generic;
using System.Linq;
using Megingjord.Shared.Editor.IO;
using Megingjord.Shared.Editor.Utils;
using Megingjord.Shared.Helpers;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Megingjord.Tools.Editor.Prefab_History {
    public class PrefabHistory : EditorWindow {
        
        private const int ButtonSize = 30;
        private const int PrefabHistorySize = 25;
        private const int SessionHistorySize = 10;
        
        private static readonly List<GameObject> SessionHistory = new();
        
        private static readonly Vector2 ConstrainedSize = new(300, 500);
        private static string _rootDirectory;
        
        private static PrefabHistoryData _data;

        private GUIContent _pingContent;
        private GUIContent _unpinContent;
        private GUIContent _pinContent;

        private Vector2 _scrollPos;

        private void OnEnable() {
            BuildToolbar();

            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            var path = DirectoryUtils.GetAssetDirectoryPath();
            if (path is null) {
                MWarn.CouldNotFindRoot.Show();
                return;
            }

            _rootDirectory = path;

            _pingContent = EditorGUIUtility.IconContent("animationvisibilitytoggleon");
            _pingContent.tooltip = "Show in project";
            _unpinContent = EditorGUIUtility.IconContent("pinned");
            _unpinContent.tooltip = "Unpin";
            _pinContent = EditorGUIUtility.IconContent("pin");
            _pinContent.tooltip = "Pin";
            
            _data = MegingjordIO.Load<PrefabHistoryData>(_rootDirectory);
        }

        private void OnDisable() {
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }

        /// <summary>
        /// Build the UI for a prefab to be added to the window
        /// </summary>
        /// <param name="prefab"></param>
        private void AddPrefabToWindow(PrefabInstance prefab) {
            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(prefab.path);
            var index = _data.prefabCache.IndexOf(prefab);
            EditorGUILayout.BeginHorizontal();

            if(GUILayout.Button(new GUIContent(prefab.name, "Place in scene"), GUILayout.Height(ButtonSize))) {
                PlacePrefab(obj);
            }

            if (GUILayout.Button(prefab.pinned ? _unpinContent : _pinContent, GUILayout.Width(ButtonSize),
                    GUILayout.Height(ButtonSize))) {
                var pinned = _data.prefabCache[index].pinned;
                _data.prefabCache[index].pinned = !pinned;
                Save();
            }
            
            if (GUILayout.Button(_pingContent, GUILayout.Width(ButtonSize), GUILayout.Height(ButtonSize))) {
                EditorGUIUtility.PingObject(obj);
                if (_data.selectOnPing)
                    Selection.activeObject = obj;
            }
            
            if (GUILayout.Button(new GUIContent("X", "Remove"), GUILayout.Width(ButtonSize), GUILayout.Height(ButtonSize))) {
                RemovePrefab(prefab);
            }
            EditorGUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// Build the window toolbar
        /// </summary>
        private void BuildToolbar() {
            var toolbar = new IMGUIContainer(() => {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                if (GUILayout.Button("Options", EditorStyles.toolbarDropDown)) {
                    var toolsMenu = new GenericMenu();
                    toolsMenu.AddItem(new GUIContent("Clear History"), false, () => {
                        var notPinned = GetNotPinned();
                        foreach (var prefab in notPinned) {
                            RemovePrefab(prefab);
                        }
                    });

                    var unpackToggleText = _data.unpack ? "✓ Unpack" : "Unpack";
                    toolsMenu.AddItem(new GUIContent( $"Config/{unpackToggleText}"), false, () => {
                        _data.unpack = !_data.unpack;
                        Save();
                    });
                    
                    var selectToggleText = _data.selectOnPing ? "✓ Select on Ping" : "Select on Ping";
                    toolsMenu.AddItem(new GUIContent($"Config/{selectToggleText}"), false, () => {
                        _data.selectOnPing = !_data.selectOnPing;
                        Save();
                    });

                    var historyCount = SessionHistory.Count;
                    var hasHistory = historyCount > 0;
                    if (hasHistory) {
                        var last = SessionHistory[historyCount - 1];
                        toolsMenu.AddItem(new GUIContent("Undo"), false, () => {
                            SessionHistory.Remove(last);
                            DestroyImmediate(last);
                        });
                    } else {
                        toolsMenu.AddDisabledItem(new GUIContent("Undo"));
                    }

                    toolsMenu.DropDown(new Rect(0, 0, 0, 16));
                }
                
                EditorGUILayout.LabelField($"History: {GetNotPinned().Count}/{PrefabHistorySize}");
                
                GUILayout.EndHorizontal();
            });
            rootVisualElement.Add(toolbar);
        }
        
        /// <summary>
        /// The GUI loop where all of the ui drawing happens
        /// </summary>
        private void OnGUI() {
            var pinned = GetPinned();
            var unpinned = GetNotPinned();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Pinned");
            foreach (var pinnedPrefab in pinned) {
                AddPrefabToWindow(pinnedPrefab);
            }
            
            EditorComponents.DrawUIDivider();
            EditorGUILayout.LabelField("Recent");
            
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandWidth(true));

            foreach (var unpinnedPrefab in unpinned) {
                AddPrefabToWindow(unpinnedPrefab);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Place a prefab in the scene dependant on where the editor
        /// camera currently is in world space
        /// </summary>
        /// <param name="prefab"></param>
        private static void PlacePrefab(Object prefab) {
            var targetPosition = SceneUtils.GetPositionForwardFromCamera();
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if(_data.unpack)
                PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            obj.transform.position = targetPosition;
            Selection.activeObject = obj;
            
            var sessionHistoryCount = SessionHistory.Count;
            if (sessionHistoryCount == SessionHistorySize) {
                var first = SessionHistory[0];
                SessionHistory.Remove(first);
            }
            SessionHistory.Add(obj);
        }
        
        /// <summary>
        /// Add a prefab to the history list if it isn't already in there,
        /// if the list is at max size then remove the one added the longest
        /// time ago. Does not impact pinned prefabs
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        private static void AddPrefab(string name, string path) {
            if (GetNotPinned().Count >= PrefabHistorySize)
                RemoveOldest();
            
            var prefab = _data.prefabCache.Find(instance => instance.name.Equals(name));
            if (prefab != null) return;
            var prefabInstance = new PrefabInstance {
                name = name,
                path = path,
                pinned = false,
                added = DateTime.Now
            };
            _data.prefabCache.Add(prefabInstance);
            Save();
        }

        /// <summary>
        /// Remove prefab by name if found and save the session
        /// </summary>
        /// <param name="name"></param>
        private static void RemovePrefab(string name) {
            var prefab = _data.prefabCache.Find(instance => instance.name.Equals(name));
            if (prefab == null) return;
                RemovePrefab(prefab);
        }

        /// <summary>
        /// Remove a prefab and save the session
        /// </summary>
        /// <param name="prefab"></param>
        private static void RemovePrefab(PrefabInstance prefab) {
            _data.prefabCache.Remove(prefab);
            Save();
        }

        /// <summary>
        /// Remove the oldest prefab from the history list
        /// </summary>
        private static void RemoveOldest() {
            PrefabInstance oldest = null;
            foreach (var instance in _data.prefabCache.Where(
                         instance => oldest == null || oldest.added < instance.added)) {
                oldest = instance;
            }
            RemovePrefab(oldest);
        }      
        
        /// <summary>
        /// Save the session data for this window
        /// </summary>
        private static void Save() {
            MegingjordIO.Save(_data, _rootDirectory);
        }

        /// <summary>
        /// Check if there is a prefab of a given name already cached
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool PrefabIsCached(string name) {
            return _data.prefabCache.Any(prefab => prefab.name.Equals(name));
        }

        /// <summary>
        /// Gets a list of all of the cached prefabs that are pinned
        /// </summary>
        /// <returns></returns>
        private static List<PrefabInstance> GetPinned() {
            return _data.prefabCache.FindAll(prefab => prefab.pinned);
        }
        
        /// <summary>
        /// Gets a list of all of the cached prefabs that are not pinned
        /// </summary>
        /// <returns></returns>
        private static List<PrefabInstance> GetNotPinned() {
            return _data.prefabCache.FindAll(prefab => !prefab.pinned);
        }

        /// <summary>
        /// Catch the hierarchy change event, remove the
        /// object from the history if it was deleted in
        /// the hierarchy or add the prefab to the recent list
        /// </summary>
        private static void OnHierarchyChanged() {
            foreach (var obj in SessionHistory.Where(obj => obj.IsDestroyed())) {
                SessionHistory.Remove(obj);
                break;
            }
            
            var selected = Selection.activeObject;
            if (selected == null || selected is not GameObject) return;
            var target = PrefabUtility.GetCorrespondingObjectFromOriginalSource(selected);
            if (target == null) return;
            var path = AssetDatabase.GetAssetPath(target);
            if (path == null) return;
            AddPrefab(target.name, path);
        }

        /// <summary>
        /// Add context option
        /// </summary>
        /// <returns></returns>
        [MenuItem("Assets/Megingjord/Prefab/Add to menu")]
        private static void AddPrefabToMenu() {
            var selected = Selection.activeObject;
            if (selected is not GameObject) return;
            var path = AssetDatabase.GetAssetPath(selected);
            AddPrefab(selected.name, path);
        }
        
        /// <summary>
        /// Add context option validation
        /// </summary>
        /// <returns></returns>
        [MenuItem("Assets/Megingjord/Prefab/Add to menu", true)]
        private static bool AddValidation() {
            var selected = Selection.activeObject;
            return selected is GameObject && !PrefabIsCached(selected.name);
        }
        
        /// <summary>
        /// Remove context option
        /// </summary>
        /// <returns></returns>
        [MenuItem("Assets/Megingjord/Prefab/Remove from menu")]
        private static void RemovePrefabToMenu() {
            var selected = Selection.activeObject;
            RemovePrefab(selected.name);
        }
        
        /// <summary>
        /// Remove context option validation
        /// </summary>
        /// <returns></returns>
        [MenuItem("Assets/Megingjord/Prefab/Remove from menu", true)]
        private static bool RemoveValidation() {
            var selected = Selection.activeObject;
            return selected is GameObject && PrefabIsCached(selected.name);
        }

        /// <summary>
        /// Add entry to the main unity toolbar
        /// </summary>
        [MenuItem("Megingjord/Prefab Helper")]
        private static void OpenWindow() {
            var window = GetWindow<PrefabHistory>();
            window.titleContent = new GUIContent("Prefab History");
            window.maxSize = ConstrainedSize;
            window.minSize = ConstrainedSize;
        }

    }
}
    