using UnityEditor;
using UnityEngine;

namespace Megingjord.Tools.Editor.Scene_Management {
    public class SceneSwitcher : EditorWindow {
        
        private static readonly Vector2 ConstrainedSize = new(400, 500);

        private string _currentSelected;

        private void OnEnable() {
            //_currentSelected = EditorSceneManager.loaded;
        }

        private void OnGUI() {
            if (_currentSelected == null) return;
            
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.LabelField(new GUIContent(_currentSelected));
            
            
            
            EditorGUILayout.EndVertical();
        }
        
        /*private static string SceneName(string scene) {
            return scene.Replace(".unity", "").FirstCharacterToUpper();
        }
        
        private static IEnumerable<string> GetScenes() {
            return Directory.GetFiles("Assets/Scenes/")
                .Select(Path.GetFileName).Where(file => file.EndsWith(".unity"))
                .ToArray();
        }*/

        /// <summary>
        /// Add entry to the main unity toolbar
        /// </summary>
        [MenuItem("Megingjord/Scene Switcher")]
        private static void OpenWindow() {
            var window = GetWindow<SceneSwitcher>();
            window.titleContent = new GUIContent("Scene Switcher");
            window.maxSize = ConstrainedSize;
            window.minSize = ConstrainedSize;
        }
        
    }
}