using Megingjord.Tools.Dialogue_Manager.API.Core.Data;
using UnityEditor;

namespace Megingjord.Shared {
    public abstract class AssetIO {
        
        public static void SaveDialogueAsset(DialogueData dialogue, string fileName) {
            var loadedAsset =
                AssetDatabase.LoadAssetAtPath(fileName, typeof(DialogueData)) as DialogueData;

            if (loadedAsset == null || !AssetDatabase.Contains(loadedAsset)) {
                AssetDatabase.CreateAsset(dialogue, fileName);
                EditorUtility.SetDirty(dialogue);
            } else {
                loadedAsset.firstTimeOpen = false;
                // Node lists
                loadedAsset.basicNodeData = dialogue.basicNodeData;
                loadedAsset.eventNodeData = dialogue.eventNodeData;
                loadedAsset.focusNodeData = dialogue.focusNodeData;
                loadedAsset.conditionNodeData = dialogue.conditionNodeData;
                loadedAsset.choiceNodeData = dialogue.choiceNodeData;
                loadedAsset.exitNodeData = dialogue.exitNodeData;
                loadedAsset.linkData = dialogue.linkData;
                // Entry node info
                loadedAsset.entryGuid = dialogue.entryGuid;
                loadedAsset.entryNodePosition = dialogue.entryNodePosition;

                loadedAsset.intProperties = dialogue.intProperties;
                loadedAsset.stringProperties = dialogue.stringProperties;
                loadedAsset.boolProperties = dialogue.boolProperties;
                EditorUtility.SetDirty(loadedAsset);
            }
            AssetDatabase.SaveAssets();
        }

    }
}